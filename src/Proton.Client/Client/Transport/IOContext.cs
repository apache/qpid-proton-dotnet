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
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Client.Transport
{
   /// <summary>
   /// Represents an IO context used by clients to commicate with remote
   /// services and provides a single threaded event loop used to manage
   /// IO based work and connection related services.
   /// </summary>
   public sealed class IOContext
   {
      // TODO Add shutdown quiesse timeouts
      // private static readonly int SHUTDOWN_TIMEOUT = 50;

      private readonly IEventLoop eventLoop;
      private readonly TransportOptions transportOptions;
      private readonly SslOptions sslOptions;

      public IOContext(TransportOptions transportOptions, SslOptions sslOptions)
      {
         Statics.RequireNonNull(transportOptions, "Transport Options cannot be null");
         Statics.RequireNonNull(sslOptions, "Transport SSL Options cannot be null");

         this.eventLoop = new DefaultEventLoop();
         this.transportOptions = transportOptions;
         this.sslOptions = sslOptions;
      }

      /// <summary>
      /// Provides access to the event loop used to process all IO related
      /// work done within a client instance.
      /// </summary>
      public IEventLoop EventLoop => eventLoop;

      public void Shutdown()
      {
         eventLoop.Shutdown();  // TODO graceful shutdown with quiesce
      }

      public ITransport NewTransport()
      {
         if (eventLoop.IsShutdown || eventLoop.IsTerminated)
         {
            throw new InvalidOperationException("Cannot create new transport when context is shutdown.");
         }

         // TODO - WebSockets

         return new TcpTransport(transportOptions, sslOptions, eventLoop);
      }
   }
}
