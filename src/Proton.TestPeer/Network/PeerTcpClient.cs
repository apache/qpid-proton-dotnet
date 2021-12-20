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
   public sealed class PeerTcpClient
   {
      private readonly Socket clientSocket;
      private readonly ChannelReader<Stream> channelOutputSink;
      private readonly ChannelWriter<Stream> channelOutputSource;
      private readonly bool serverClientConnection;
      private readonly AtomicBoolean closed = new AtomicBoolean();

      private Action<PeerTcpClient> connectedHandler;
      private Action<PeerTcpClient, Exception> connectFailedHandler;
      private Action<PeerTcpClient> disconnectedHandler;
      private Action<PeerTcpClient, byte[]> readHandler;

      private Stream streamReader;
      private Stream streamWriter;

      private Task readLoop;
      private Task writeLoop;

      private ILogger<PeerTcpClient> logger;

      /// <summary>
      /// Create a new peer Tcp client instance that can be used to connect to a remote.
      /// </summary>
      public PeerTcpClient(in ILoggerFactory loggerFactory) : this(loggerFactory, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), false)
      {
      }

      public PeerTcpClient(in ILoggerFactory loggerFactory, in Socket socket) : this(loggerFactory, socket, true)
      {
      }

      private PeerTcpClient(in ILoggerFactory loggerFactory, in Socket socket, bool serverClientConnection)
      {
         Statics.RequireNonNull(socket, "Client Socket cannot be null");

         this.clientSocket = socket;
         this.serverClientConnection = serverClientConnection;
         this.logger = loggerFactory.CreateLogger<PeerTcpClient>();

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

      public PeerTcpClient TransportConnectedHandler(Action<PeerTcpClient> connectedHandler)
      {
         Statics.RequireNonNull(connectedHandler, "Cannot set a null connected handler");
         this.connectedHandler = connectedHandler;
         return this;
      }

      public PeerTcpClient TransportConnectFailedHandler(Action<PeerTcpClient, Exception> connectFailedHandler)
      {
         Statics.RequireNonNull(connectFailedHandler, "Cannot set a null connect failed handler");
         this.connectFailedHandler = connectFailedHandler;
         return this;
      }

      public PeerTcpClient TransportDisconnectedHandler(Action<PeerTcpClient> disconnectedHandler)
      {
         Statics.RequireNonNull(disconnectedHandler, "Cannot set a null disconnected handler");
         this.disconnectedHandler = disconnectedHandler;
         return this;
      }

      public PeerTcpClient TransportReadHandler(Action<PeerTcpClient, byte[]> readHandler)
      {
         Statics.RequireNonNull(disconnectedHandler, "Cannot set a null read handler");
         this.readHandler = readHandler;
         return this;
      }

      public IPEndPoint LocalEndpoint => (IPEndPoint)clientSocket.LocalEndPoint;

      public IPEndPoint RemoteEndpoint => (IPEndPoint)clientSocket.RemoteEndPoint;

      public void Close()
      {
         if (closed.CompareAndSet(false, true))
         {
            Task.Delay(20).GetAwaiter().GetResult();

            try
            {
               // Stop additional writes but wait for queued writes to complete.
               channelOutputSource?.TryComplete();
               writeLoop?.GetAwaiter().GetResult();
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

      public void Connect(IPEndPoint endpoint)
      {
         if (clientSocket.Connected || serverClientConnection)
         {
            throw new InvalidOperationException("Cannot connect when socket is already connected");
         }

         if (closed)
         {
            throw new InvalidOperationException("Cannot connect after client connection was closed");
         }

         Statics.RequireNonNull(endpoint, "Cannot connect when the end point given is null");
         Statics.RequireNonNull(connectedHandler, "Cannot connect until a connected handler is registered");
         Statics.RequireNonNull(connectFailedHandler, "Cannot connect until a connect failed handler is registered");
         Statics.RequireNonNull(disconnectedHandler, "Cannot connect until a disconnected handler is registered");
         Statics.RequireNonNull(readHandler, "Cannot connect when a read handler is registered");

         try
         {
            clientSocket.Connect(endpoint);
         }
         catch (Exception)
         {
            try
            {
               clientSocket.Close();
            }
            catch (Exception)
            {
            }
            throw;
         }

         streamReader = new BufferedStream(new NetworkStream(clientSocket));
         streamWriter = new BufferedStream(new NetworkStream(clientSocket));

         readLoop = Task.Factory.StartNew(ChannelReadLoop, TaskCreationOptions.LongRunning);
         writeLoop = Task.Factory.StartNew(ChannelWriteLoop, TaskCreationOptions.LongRunning);
      }

      public void Connect(string address, int port)
      {
         IPHostEntry entry = Dns.GetHostEntry(address);
         foreach (IPAddress ipAddress in entry.AddressList)
         {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
               Connect(new IPEndPoint(ipAddress, port));
               return;
            }
         }

         throw new InvalidOperationException("Could not resolve the address into an IPV4 IP Address");
      }

      public void Start()
      {
         if (!serverClientConnection)
         {
            throw new InvalidOperationException("Start is only valid on server created remote connections");
         }

         if (closed)
         {
            throw new InvalidOperationException("Cannot start after client connection was closed");
         }

         Statics.RequireNonNull(disconnectedHandler, "Cannot connect until a disconnected handler is registered");
         Statics.RequireNonNull(readHandler, "Cannot connect when a read handler is registered");

         streamReader = new BufferedStream(new NetworkStream(clientSocket));
         streamWriter = new BufferedStream(new NetworkStream(clientSocket));

         readLoop = Task.Factory.StartNew(ChannelReadLoop, TaskCreationOptions.LongRunning);
         writeLoop = Task.Factory.StartNew(ChannelWriteLoop, TaskCreationOptions.LongRunning);
      }

      public PeerTcpClient Write(Stream stream)
      {
         CheckClosed();
         channelOutputSource.TryWrite(stream);
         return this;
      }

      private void CheckClosed()
      {
         if (closed)
         {
            throw new InvalidOperationException("Peer TCP channel is closed");
         }
      }

      #region Channel Read Async pump

      private void ChannelReadLoop()
      {
         while (!closed)
         {
            byte[] readBuffer = new byte[1024];

            try
            {
               int bytesRead = streamReader.Read(readBuffer, 0, readBuffer.Length);
               if (bytesRead == 0)
               {
                  _ = channelOutputSource.TryComplete();

                  // End of stream
                  if (!closed)
                  {
                     logger.LogTrace("{0} TCP client read end of stream when not already closed.",
                                    serverClientConnection ? "Server" : "Client");
                     disconnectedHandler(this);
                  }

                  break;
               }
               else
               {
                  logger.LogTrace("{0} TCP client read {1} bytes from incoming read event",
                                 serverClientConnection ? "Server" : "Client", bytesRead);
                  if (bytesRead < readBuffer.Length)
                  {
                     readBuffer = Statics.CopyOf(readBuffer, bytesRead);
                  }

                  readHandler(this, readBuffer);
               }
            }
            catch (IOException)
            {
               _ = channelOutputSource.TryComplete();

               // End of stream
               if (!closed)
               {
                  logger.LogTrace("{0} TCP client read end of stream when not already closed.",
                                 serverClientConnection ? "Server" : "Client");
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
                  logger.LogTrace("{0} TCP client writing {1} bytes to output stream",
                                  serverClientConnection ? "Server" : "Client", stream.Length);
                  await stream.CopyToAsync(streamWriter, 8192);
               }

               await streamWriter.FlushAsync().ConfigureAwait(false);
            }
         }
         catch (Exception)
         {
            if (!closed)
            {
               logger.LogTrace("{0} TCP client write failed when not already closed.",
                              serverClientConnection ? "Server" : "Client");
               disconnectedHandler(this);
            }
         }
      }

      #endregion
   }
}