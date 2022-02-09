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

using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Sender implementation that send complete messages on a remote link.
   /// </summary>
   public sealed class ClientSender : ClientAbstractSender
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientSender>();

      private readonly SenderOptions options;

      internal ClientSender(ClientSession session, SenderOptions options, string senderId, Engine.ISender protonSender)
         : base(session, senderId, protonSender)
      {
         this.options = new SenderOptions(options);
      }

      internal override SenderOptions Options => options;

      internal override ITracker CreateTracker(IOutgoingDelivery delivery)
      {
         return new ClientTracker(this, delivery);
      }

      internal override ITracker CreateNoOpTracker()
      {
         return new ClientNoOpTracker(this);
      }
   }
}