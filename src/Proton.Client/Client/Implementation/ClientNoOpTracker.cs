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
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientNoOpTracker : ITracker
   {
      private readonly ClientSender sender;

      private IDeliveryState state;
      private bool settled;

      private readonly Task<ITracker> completed;

      internal ClientNoOpTracker(ClientSender sender)
      {
         this.sender = sender;
         this.completed = Task.FromResult<ITracker>(this);
      }

      public ISender Sender => sender;

      public bool Settled => settled;

      public IDeliveryState State => state;

      public bool RemoteSettled => true;

      public IDeliveryState RemoteState => ClientAccepted.Instance;

      public Task<ITracker> SettlementTask => completed;

      public ITracker AwaitAccepted()
      {
         return this;
      }

      public ITracker AwaitAccepted(TimeSpan timeout)
      {
         return this;
      }

      public ITracker AwaitSettlement()
      {
         return this;
      }

      public ITracker AwaitSettlement(TimeSpan timeout)
      {
         return this;
      }

      public ITracker Disposition(IDeliveryState state, bool settle)
      {
         this.state = state;
         this.settled = settle;

         return this;
      }

      public ITracker Settle()
      {
         this.settled = true;

         return this;
      }
   }
}