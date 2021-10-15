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
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Utilities;

namespace Apache.Qpid.Proton.Client.Impl
{
   // TODO
   /// <summary>
   /// The session builder is used by client connection instances to create
   /// AMQP sessions and wrap those within a client session instance.
   /// </summary>
   internal class ClientSessionBuilder
   {
      private readonly AtomicInteger sessionCounter = new AtomicInteger();
      private readonly ClientConnection connection;
      private readonly ConnectionOptions connectionOptions;

      private volatile SessionOptions defaultSessionOptions;

      public ClientSessionBuilder(ClientConnection connection)
      {
         this.connection = connection;
         this.connectionOptions = new ConnectionOptions(connection.Options);
      }

      public SessionOptions DefaultSessionOptions => GetOrCreateDefaultSessionOptions();

      public ClientSession Session(SessionOptions sessionOptions)
      {
         SessionOptions options = sessionOptions != null ? sessionOptions : GetOrCreateDefaultSessionOptions();
         string sessionId = NextSessionId();
         Engine.ISession protonSession = CreateSession(connection.ProtonConnection, options);

         return new ClientSession(connection, options, sessionId, protonSession);
      }

    public ClientStreamSession StreamSession(SessionOptions sessionOptions)
     {
         SessionOptions options = sessionOptions != null ? sessionOptions : GetOrCreateDefaultSessionOptions();
         string sessionId = NextSessionId();
         Engine.ISession protonSession = CreateSession(connection.ProtonConnection, options);

        return new ClientStreamSession(connection, options, sessionId, protonSession);
    }

      public static Engine.ISession RecreateSession(ClientConnection connection, Engine.ISession previousSession, SessionOptions options)
      {
         Engine.ISession session = connection.ProtonConnection.Session();

         session.IncomingCapacity = options.IncomingCapacity;
         session.OutgoingCapacity = options.OutgoingCapacity;

         return session;
      }

      private static Engine.ISession CreateSession(Engine.IConnection connection, SessionOptions options)
      {
         Engine.ISession session = connection.Session();

         session.IncomingCapacity = options.IncomingCapacity;
         session.OutgoingCapacity = options.OutgoingCapacity;

         return session;
      }

      private String NextSessionId()
      {
         return connection.ConnectionId + ":" + sessionCounter.IncrementAndGet();
      }

      private SessionOptions GetOrCreateDefaultSessionOptions()
      {
         SessionOptions sessionOptions = defaultSessionOptions;
         if (sessionOptions == null)
         {
            lock (connectionOptions)
            {
               sessionOptions = defaultSessionOptions;
               if (sessionOptions == null)
               {
                  sessionOptions = new SessionOptions();
                  sessionOptions.OpenTimeout = connectionOptions.OpenTimeout;
                  sessionOptions.CloseTimeout = connectionOptions.CloseTimeout;
                  sessionOptions.RequestTimeout = connectionOptions.RequestTimeout;
                  sessionOptions.SendTimeout = connectionOptions.SendTimeout;
                  sessionOptions.DrainTimeout = connectionOptions.DrainTimeout;
               }

               defaultSessionOptions = sessionOptions;
            }
         }

         return sessionOptions;
      }
   }
}