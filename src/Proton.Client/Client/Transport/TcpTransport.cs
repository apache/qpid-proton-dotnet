/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Threading.Channels;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Client.Transport
{
   /// <summary>
   /// A TCP only transport implementation that provides extension points
   /// for SSL and or WS based transports to add their handlers. These transports
   /// are registered with an event loop where all transport work and events
   /// are processed in serial fashion with the same thread.
   /// </summary>
   public class TcpTransport : ITransport
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<TcpTransport>();

      private readonly ChannelReader<IChannelTask> channelOutputSink;
      private readonly ChannelWriter<IChannelTask> channelOutputSource;

      private readonly AtomicBoolean closed = new AtomicBoolean();

      private readonly IEventLoop eventLoop;
      private readonly TransportOptions options;
      private readonly SslOptions sslOptions;

      private Task readLoop;
      private Task writeLoop;
      private Socket channel;
      private Stream socketReader;
      private Stream socketWriter;
      private volatile bool connected;
      private string host;
      private int port = -1;
      private bool traceBytes;

      private Action<ITransport> connectedHandler;
      private Action<ITransport, Exception> connectFailedHandler;
      private Action<ITransport> disconnectedHandler;
      private Action<ITransport, IProtonBuffer> readHandler;

      #region Transport property access APIs

      public bool IsConnected => connected;

      public string Host => host;

      public int Port => port;

      public IEventLoop EventLoop => eventLoop;

      public EndPoint EndPoint => channel?.RemoteEndPoint;

      public IPrincipal LocalPrincipal => null; // TODO

      #endregion

      public TcpTransport(TransportOptions options, SslOptions sslOptions, IEventLoop eventLoop)
      {
         this.eventLoop = eventLoop;
         this.options = options;
         this.sslOptions = sslOptions;

         Channel<IChannelTask> outputChannel = Channel.CreateUnbounded<IChannelTask>(
            new UnboundedChannelOptions
            {
               AllowSynchronousContinuations = false,
               SingleReader = true,
               SingleWriter = false
            });

         this.channelOutputSink = outputChannel.Reader;
         this.channelOutputSource = outputChannel.Writer;
         this.traceBytes = options.TraceBytes;
      }

      public void Close()
      {
         if (closed.CompareAndSet(false, true))
         {
            // Could have been completed by a channel disconnect, we shutdown
            // the writer channel and attempt to allow the output task to finish
            // the quiesced channel. If any errors occur on close we ignore them
            // since we are shutting down anyway.
            try
            {
               ChannelTermination termination = new ChannelTermination();
               if ((!channelOutputSource?.TryWrite(termination) ?? true) || !connected)
               {
                  termination.Execute();
               }
               channelOutputSource?.TryComplete();
               termination.Completion.ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
            }

            try
            {
               // Only stop reads as we might have queued writes we want to allow to fire.
               channel?.Shutdown(SocketShutdown.Receive);
            }
            catch (Exception)
            {
            }

            try
            {
               // Close with a bit of time to allow queued writes to complete.
               channel?.Close(100);
            }
            catch (Exception)
            {
            }
         }
      }

      public ITransport Connect(string host, int port)
      {
         Statics.RequireNonNull(host, "Cannot connect when the host given is null");
         Statics.RequireNonNull(connectedHandler, "Cannot connect until a connected handler is registered");
         Statics.RequireNonNull(connectFailedHandler, "Cannot connect until a connect failed handler is registered");
         Statics.RequireNonNull(disconnectedHandler, "Cannot connect until a disconnected handler is registered");
         Statics.RequireNonNull(readHandler, "Cannot connect when a read handler is registered");

         if (port < 0 && options.DefaultTcpPort < 0 && (sslOptions.SslEnabled && sslOptions.DefaultSslPort < 0))
         {
            throw new ArgumentOutOfRangeException("Transport port value must be a non-negative int value or a default port configured");
         }

         if (port < 0)
         {
            port = sslOptions.SslEnabled ? sslOptions.DefaultSslPort : options.DefaultTcpPort;
         }

         LOG.Debug("Transport attempting connection to: {0}:{1}", host, port);

         IPAddress remote = ResolveIPAddress(host);

         channel = new Socket(remote.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

         if (!String.IsNullOrEmpty(options.LocalAddress) || options.LocalPort > 0)
         {
            IPAddress localAddress = String.IsNullOrEmpty(options.LocalAddress) ?
               IPAddress.Any : ResolveIPAddress(options.LocalAddress);
            int localPort = options.LocalPort > 0 ? options.LocalPort : 0;

            channel.Bind(new IPEndPoint(localAddress, localPort));
         }

         ConfigureSocket(channel);

         channel.BeginConnect(remote, port, new AsyncCallback(ConnectCallback), this);

         this.host = host;
         this.port = port;

         return this;
      }

      internal void ConfigureSocket(Socket socket)
      {
         socket.NoDelay = options.TcpNoDelay;
         socket.SendBufferSize = options.SendBufferSize;
         socket.ReceiveBufferSize = options.ReceiveBufferSize;
         socket.LingerState = new LingerOption(options.SoLinger > 0, (int)options.SoLinger);
         socket.SendTimeout = (int)options.SendTimeout;
         socket.ReceiveTimeout = (int)options.ReceiveTimeout;
      }

      public ITransport TransportConnectedHandler(Action<ITransport> connectedHandler)
      {
         Statics.RequireNonNull(connectedHandler, "Cannot set a null connected handler");
         this.connectedHandler = connectedHandler;
         return this;
      }

      public ITransport TransportConnectFailedHandler(Action<ITransport, Exception> connectFailedHandler)
      {
         Statics.RequireNonNull(connectFailedHandler, "Cannot set a null connect failed handler");
         this.connectFailedHandler = connectFailedHandler;
         return this;
      }

      public ITransport TransportDisconnectedHandler(Action<ITransport> disconnectedHandler)
      {
         Statics.RequireNonNull(disconnectedHandler, "Cannot set a null disconnected handler");
         this.disconnectedHandler = disconnectedHandler;
         return this;
      }

      public ITransport TransportReadHandler(Action<ITransport, IProtonBuffer> readHandler)
      {
         Statics.RequireNonNull(disconnectedHandler, "Cannot set a null read handler");
         this.readHandler = readHandler;
         return this;
      }

      public ITransport Write(IProtonBuffer buffer, Action writeCompleteAction)
      {
         CheckConnected(buffer);
         LOG.Trace("Transport write dispatching buffer of size : {0}", buffer.ReadableBytes);

         try
         {
            channelOutputSource.TryWrite(new ChannelWrite(this, buffer, writeCompleteAction));
         }
         catch (Exception)
         {
         }

         return this;
      }

      public override string ToString()
      {
         return "TcpTransport:[remote = " + host + ":" + port + "]";
      }

      #region Transport private API

      private IPAddress ResolveIPAddress(string address)
      {
         IPHostEntry entry = Dns.GetHostEntry(address);
         IPAddress result = default(IPAddress);

         foreach (IPAddress ipAddress in entry.AddressList)
         {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
               result = ipAddress;
            }
         }

         if (result == default(IPAddress))
         {
            throw new IOException(
               string.Format("Could not resolve a remote address from the given host: {0}", address));
         }

         return result;
      }

      private void CheckConnected(IProtonBuffer output)
      {
         if (!connected || !channel.Connected)
         {
            throw new IOException("Cannot send to a non-connected transport.");
         }
      }

      private bool WriteComponent(IReadableComponent component)
      {
         if (component.HasReadableArray)
         {
            if (traceBytes)
            {
               LOG.Trace("IO Transport writing bytes: {0}",
                  BitConverter.ToString(component.ReadableArray, component.ReadableArrayOffset, component.ReadableArrayLength));
            }

            try
            {
               socketWriter.Write(component.ReadableArray,
                                  component.ReadableArrayOffset,
                                  component.ReadableArrayLength);
            }
            catch (Exception writeError)
            {
               LOG.Trace("Failed to write to IO layer with error: {0}", writeError.Message);
               throw;
            }
         }
         else
         {
            throw new NotImplementedException("Need a buffer copy operation in the readable component");
         }

         return true;
      }

      #endregion

      #region Async callbacks for socket operations

      private void CompleteConnection()
      {
         socketReader = new NetworkStream(channel);
         socketWriter = new NetworkStream(channel);

         readLoop = Task.Factory.StartNew(ChannelReadLoop, TaskCreationOptions.LongRunning);
         writeLoop = Task.Factory.StartNew(ChannelWriteLoop, TaskCreationOptions.LongRunning);

         connected = true;

         eventLoop.Execute(() => connectedHandler(this));
      }

      private static void ConnectCallback(IAsyncResult connectResult)
      {
         TcpTransport transport = (TcpTransport)connectResult.AsyncState;

         try
         {
            transport.channel.EndConnect(connectResult);
            transport.CompleteConnection();
         }
         catch (Exception ex)
         {
            transport.eventLoop.Execute(() => transport.connectFailedHandler(transport, ex));
         }
      }

      #endregion

      #region Channel Read and Write loops

      private void ChannelReadLoop()
      {
         while (!closed)
         {
            byte[] readBuffer = new byte[1024];

            int bytesRead = socketReader.Read(readBuffer, 0, readBuffer.Length);
            if (bytesRead == 0)
            {
               _ = channelOutputSource.TryComplete();
               try
               {
                  channel.Shutdown(SocketShutdown.Both);
               }
               catch (Exception)
               {
               }

               connected = false;

               // End of stream
               if (!closed)
               {
                  eventLoop.Execute(() => disconnectedHandler(this));
               }

               break;
            }
            else
            {
               if (traceBytes)
               {
                  LOG.Trace("IO Transport read {0}", BitConverter.ToString(readBuffer, 0, bytesRead));
               }

               eventLoop.Execute(() => readHandler(
                  this, ProtonByteBufferAllocator.Instance.Wrap(readBuffer, 0, bytesRead)));
            }
         }
      }

      // The write loop using an async channel write pattern to wait on new writes and then
      // fires those bytes into the socket output stream as they arrive.
      private async Task ChannelWriteLoop()
      {
         while (await channelOutputSink.WaitToReadAsync().ConfigureAwait(false))
         {
            IChannelTask task = null;

            while (channelOutputSink.TryRead(out task))
            {
               task.Execute();
            }
         }
      }

      #endregion

      #region Channel Write Tasks that can be written into the write channel

      private interface IChannelTask
      {
         /// <summary>
         /// Execute the task from the transport write channel
         /// </summary>
         public void Execute();
      }

      private sealed class ChannelTermination : IChannelTask
      {
         private readonly TaskCompletionSource writesCompleted = new TaskCompletionSource();

         public void Execute()
         {
            writesCompleted.TrySetResult();
         }

         public Task Completion => writesCompleted.Task;

      }

      private sealed class ChannelWrite : IChannelTask
      {
         private readonly IProtonBuffer buffer;
         private readonly Action completion;
         private readonly bool flush;
         private readonly TcpTransport transport;

         public ChannelWrite(TcpTransport transport, IProtonBuffer buffer, Action completion, bool flush = true)
         {
            this.transport = transport;
            this.buffer = buffer;
            this.completion = completion;
            this.flush = flush;
         }

         public void Execute()
         {
            if (buffer != null)
            {
               buffer.ForEachReadableComponent(0, (idx, x) => transport.WriteComponent(x));
            }

            // The bytes have hit the socket layer so we can now trigger any
            // write completion callbacks from this write,
            if (HasCompletion)
            {
               transport.eventLoop.Execute(Completion);
            }

            if (IsFlushRequired)
            {
               transport.socketWriter.Flush();
            }
         }

         public IProtonBuffer Buffer => buffer;

         public bool IsFlushRequired => flush;

         public bool HasCompletion => completion != null;

         public Action Completion => completion;
      }

      #endregion
   }
}