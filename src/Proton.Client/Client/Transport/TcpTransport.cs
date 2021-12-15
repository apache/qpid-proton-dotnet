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

      private readonly ChannelReader<ChannelWrite> channelOutputSink;
      private readonly ChannelWriter<ChannelWrite> channelOutputSource;

      private readonly AtomicBoolean closed = new AtomicBoolean();

      private readonly IEventLoop eventLoop;
      private readonly TransportOptions options;
      private readonly SslOptions sslOptions;

      private EndPoint channelEndpoint;
      private Task readLoop;
      private Task writeLoop;
      private Socket channel;
      private Stream socketReader;
      private Stream socketWriter;
      private volatile bool connected;

      private Action<ITransport> connectedHandler;
      private Action<ITransport, Exception> connectFailedHandler;
      private Action<ITransport> disconnectedHandler;
      private Action<ITransport, IProtonBuffer> readHandler;

      #region Transport property access APIs

      public bool IsConnected => connected;

      public IEventLoop EventLoop => eventLoop;

      public EndPoint EndPoint => channelEndpoint;

      public IPrincipal LocalPrincipal => null; // TODO

      #endregion

      public TcpTransport(TransportOptions options, SslOptions sslOptions, IEventLoop eventLoop)
      {
         this.eventLoop = eventLoop;
         this.options = options;
         this.sslOptions = sslOptions;

         Channel<ChannelWrite> outputChannel = Channel.CreateUnbounded<ChannelWrite>(
            new UnboundedChannelOptions
            {
               AllowSynchronousContinuations = false,
               SingleReader = true,
               SingleWriter = false
            });

         this.channelOutputSink = outputChannel.Reader;
         this.channelOutputSource = outputChannel.Writer;
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
               channelOutputSource?.TryComplete();
               writeLoop?.GetAwaiter().GetResult();
            }
            catch (Exception)
            {
            }

            try
            {
               channel?.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }

            try
            {
               channel?.Close();
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

         IPAddress address = Dns.GetHostEntry(host).AddressList[0];

         channel = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
         channel.BeginConnect(address, port, new AsyncCallback(ConnectCallback), this);

         return this;
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
         LOG.Trace("Transport write dispatching buffer of size : {0}", buffer.ReadableBytes);

         try
         {
            channelOutputSource.TryWrite(new ChannelWrite(buffer, writeCompleteAction));
         }
         catch (Exception)
         {
         }

         return this;
      }

      #region Async callbacks for socket operations

      private void CompleteConnection()
      {
         socketReader = new NetworkStream(channel);
         socketWriter = new NetworkStream(channel);

         // TODO: This currently creates two threads for each transport which could
         //       be reduced to one or none at some point using async Tasks
         readLoop = Task.Factory.StartNew(ChannelReadLoop, TaskCreationOptions.LongRunning);
         writeLoop = Task.Factory.StartNew(ChannelWriteLoop, TaskCreationOptions.LongRunning);

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

      #region Channel Read Task

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
               { }

               // End of stream
               // TODO mark as disconnected to fail writes
               if (!closed)
               {
                  eventLoop.Execute(() => disconnectedHandler(this));
               }

               break;  // TODO more graceful error handling.
            }
            else
            {
               // TODO : Use a wrapped buffer that accepts offset and size
               if (bytesRead < readBuffer.Length)
               {
                  readBuffer = Statics.CopyOf(readBuffer, bytesRead);
               }

               IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(readBuffer);

               eventLoop.Execute(() => readHandler(this, buffer));
            }
         }
      }

      #endregion

      #region Channel Write holder and channel write loop

      private sealed class ChannelWrite
      {
         private readonly IProtonBuffer buffer;
         private readonly Action completion;
         private readonly bool flush;

         public ChannelWrite(IProtonBuffer buffer, Action completion, bool flush = true)
         {
            this.buffer = buffer;
            this.completion = completion;
            this.flush = flush;
         }

         public IProtonBuffer Buffer => buffer;

         public bool IsFlushRequired => flush;

         public bool HasCompletion => completion != null;

         public Action Completion => completion;
      }

      // The write loop using an async channel write pattern to wait on new writes and then
      // fires those bytes into the socket output stream as they arrive.
      private async Task ChannelWriteLoop()
      {
         while (await channelOutputSink.WaitToReadAsync().ConfigureAwait(false))
         {
            ChannelWrite write = null;

            while (channelOutputSink.TryRead(out write))
            {
               write.Buffer.ForEachReadableComponent(0, (idx, x) => WriteComponent(x));

               // The bytes have hit the socket layer so we can now trigger any
               // write completion callbacks from this write,
               if (write.HasCompletion)
               {
                  eventLoop.Execute(write.Completion);
               }
            }

            if (write?.IsFlushRequired ?? false)
            {
               await socketWriter.FlushAsync().ConfigureAwait(false);
            }
         }
      }

      private bool WriteComponent(IReadableComponent component)
      {
         if (component.HasReadableArray)
         {
            socketWriter.Write(component.ReadableArray,
                               component.ReadableArrayOffset,
                               component.ReadableArrayLength);
         }
         else
         {
            throw new NotImplementedException("Need a buffer copy operation in the readable component");
         }

         return true;
      }

      #endregion
   }
}