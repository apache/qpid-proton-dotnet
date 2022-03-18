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
using System.Threading.Channels;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Test.Driver.Utilities;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver.Network
{
   public enum PeerTransportRole : byte
   {
      Client,
      Server
   };

   /// <summary>
   /// IO Level transport that provides read event points and write entry points
   /// </summary>
   public sealed class PeerTcpTransport
   {
      private readonly Socket clientSocket;
      private readonly ChannelReader<Stream> channelOutputSink;
      private readonly ChannelWriter<Stream> channelOutputSource;
      private readonly AtomicBoolean closed = new AtomicBoolean();

      private Action<PeerTcpTransport> connectedHandler;
      private Action<PeerTcpTransport, Exception> connectFailedHandler;
      private Action<PeerTcpTransport> disconnectedHandler;
      private Action<PeerTcpTransport, byte[]> readHandler;

      private readonly PeerTransportRole role;

      private Stream streamReader;
      private Stream streamWriter;

      private Task readLoop;
      private Task writeLoop;

      private bool traceBytes;

      private ILogger<PeerTcpTransport> logger;

      internal PeerTcpTransport(in ILoggerFactory loggerFactory, PeerTransportRole role, Socket socket, Stream networkStream)
      {
         Statics.RequireNonNull(socket, "Network Socket cannot be null");
         Statics.RequireNonNull(networkStream, "Network stream cannot be null");

         this.logger = loggerFactory?.CreateLogger<PeerTcpTransport>();

         this.streamReader = networkStream;
         this.streamWriter = networkStream;
         this.clientSocket = socket;
         this.role = role;

         Channel<Stream> outputChannel = Channel.CreateUnbounded<Stream>(
            new UnboundedChannelOptions
            {
               AllowSynchronousContinuations = false,
               SingleReader = true,
               SingleWriter = false
            });

         this.channelOutputSink = outputChannel.Reader;
         this.channelOutputSource = outputChannel.Writer;
      }

      public bool TraceBytes
      {
         get => traceBytes;
         set => traceBytes = value;
      }

      public IPEndPoint LocalEndpoint => (IPEndPoint)clientSocket.LocalEndPoint;

      public IPEndPoint RemoteEndpoint => (IPEndPoint)clientSocket.RemoteEndPoint;

      public void Start()
      {
         readLoop = Task.Factory.StartNew(ChannelReadLoop, TaskCreationOptions.LongRunning);
         writeLoop = Task.Factory.StartNew(ChannelWriteLoop, TaskCreationOptions.LongRunning);
      }

      public void Close()
      {
         if (closed.CompareAndSet(false, true))
         {
            Task.Delay(20).ConfigureAwait(false).GetAwaiter().GetResult();

            try
            {
               // Stop additional writes but wait for queued writes to complete.
               channelOutputSource?.TryComplete();
               writeLoop?.ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
            }

            try
            {
               clientSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }

            try
            {
               clientSocket.Close(10);
            }
            catch (Exception)
            {
            }
         }
      }

      public PeerTcpTransport Write(Stream stream)
      {
         CheckClosed();
         channelOutputSource.TryWrite(stream);
         return this;
      }

      public PeerTcpTransport TransportConnectedHandler(Action<PeerTcpTransport> connectedHandler)
      {
         Statics.RequireNonNull(connectedHandler, "Cannot set a null connected handler");
         this.connectedHandler = connectedHandler;
         return this;
      }

      public PeerTcpTransport TransportConnectFailedHandler(Action<PeerTcpTransport, Exception> connectFailedHandler)
      {
         Statics.RequireNonNull(connectFailedHandler, "Cannot set a null connect failed handler");
         this.connectFailedHandler = connectFailedHandler;
         return this;
      }

      public PeerTcpTransport TransportDisconnectedHandler(Action<PeerTcpTransport> disconnectedHandler)
      {
         Statics.RequireNonNull(disconnectedHandler, "Cannot set a null disconnected handler");
         this.disconnectedHandler = disconnectedHandler;
         return this;
      }

      public PeerTcpTransport TransportReadHandler(Action<PeerTcpTransport, byte[]> readHandler)
      {
         Statics.RequireNonNull(disconnectedHandler, "Cannot set a null read handler");
         this.readHandler = readHandler;
         return this;
      }

      #region Private Transport helper methods

      private string RoleToDebugString()
      {
         return role == PeerTransportRole.Server ? "Server" : "Client";
      }

      private void CheckClosed()
      {
         if (closed)
         {
            throw new InvalidOperationException("Peer TCP channel is closed");
         }
      }

      #endregion

      #region Channel Read Async pump

      private void ChannelReadLoop()
      {
         while (!closed)
         {
            byte[] readBuffer = new byte[1024];

            try
            {
               int bytesRead = streamReader.Read(readBuffer, 0, readBuffer.Length);
               if (bytesRead == readBuffer.Length)
               {
                  logger.LogTrace("Read a chunk");
               }
               if (bytesRead == 0)
               {
                  _ = channelOutputSource.TryComplete();

                  // End of stream
                  if (!closed)
                  {
                     logger.LogTrace("{0} TCP client read end of stream when not already closed.", RoleToDebugString());
                     disconnectedHandler(this);
                  }

                  break;
               }
               else
               {
                  logger.LogTrace("{0} TCP client read {1} bytes from incoming read event", RoleToDebugString(), bytesRead);
                  if (bytesRead < readBuffer.Length)
                  {
                     readBuffer = Statics.CopyOf(readBuffer, bytesRead);
                  }

                  if (traceBytes)
                  {
                     logger.LogTrace("IO Transport read {0}", BitConverter.ToString(readBuffer));
                  }

                  try
                  {
                     readHandler(this, readBuffer);
                  }
                  catch (Exception readError)
                  {
                     logger.LogWarning("I/O Read handler failed with error: {0}", readError.Message);
                     throw new IOException("Read handler threw unexpected error", readError);
                  }
               }
            }
            catch (IOException)
            {
               _ = channelOutputSource.TryComplete();

               // End of stream
               if (!closed)
               {
                  logger.LogTrace("{0} TCP client read end of stream when not already closed.", RoleToDebugString());
                  disconnectedHandler(this);
               }
            }
         }
      }

      #endregion

      #region Channel Writes async pump

      // The write loop using an async channel write pattern to wait on new writes and then
      // fires those bytes into the socket output stream as they arrive.
      private async Task ChannelWriteLoop()
      {
         try
         {
            while (await channelOutputSink.WaitToReadAsync().ConfigureAwait(false))
            {
               while (channelOutputSink.TryRead(out Stream stream))
               {
                  logger.LogTrace("{0} TCP client writing {1} bytes to output stream", RoleToDebugString(), stream.Length);
                  await stream.CopyToAsync(streamWriter, 8192);
               }

               await streamWriter.FlushAsync().ConfigureAwait(false);
            }
         }
         catch (Exception)
         {
            if (!closed)
            {
               logger.LogTrace("{0} TCP client write failed when not already closed.", RoleToDebugString());
               disconnectedHandler(this);
            }
         }
      }

      #endregion
   }
}