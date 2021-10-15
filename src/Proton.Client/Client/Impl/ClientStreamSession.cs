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

using System.Collections.Generic;
using Apache.Qpid.Proton.Client.Exceptions;

namespace Apache.Qpid.Proton.Client.Impl
{
   public sealed class ClientStreamSession : ClientSession
   {
      internal ClientStreamSession(ClientConnection connection, SessionOptions options, string sessionId, Engine.ISession session)
        : base(connection, options, sessionId, session)
      {
      }

      public override IReceiver OpenDurableReceiver(string address, string subscriptionName)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a receiver from a streaming resource session");
      }

      public override IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a receiver from a streaming resource session");
      }

      public override IReceiver OpenDynamicReceiver()
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a receiver from a streaming resource session");
      }

      public override IReceiver OpenDynamicReceiver(ReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a receiver from a streaming resource session");
      }

      public override IReceiver OpenDynamicReceiver(IDictionary<string, object> dynamicNodeProperties, ReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a receiver from a streaming resource session");
      }

      public override IReceiver OpenReceiver(string address)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a receiver from a streaming resource session");
      }

      public override IReceiver OpenReceiver(string address, ReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a receiver from a streaming resource session");
      }

      public override ISender OpenSender(string address)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a sender from a streaming resource session");
      }

      public override ISender OpenSender(string address, SenderOptions options)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a sender from a streaming resource session");
      }

      public override ISender OpenAnonymousSender()
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a sender from a streaming resource session");
      }

      public override ISender OpenAnonymousSender(SenderOptions options)
      {
         CheckClosedOrFailed();
         throw new ClientUnsupportedOperationException("Cannot create a sender from a streaming resource session");
      }
   }
}