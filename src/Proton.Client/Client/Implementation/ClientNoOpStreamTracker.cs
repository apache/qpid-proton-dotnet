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
   public sealed class ClientNoOpStreamTracker : IStreamTracker
   {
      private readonly IStreamSender sender;

      private IDeliveryState state;
      private bool settled;

      private readonly Task<IStreamTracker> completed;

      internal ClientNoOpStreamTracker(IStreamSender sender)
      {
         this.sender = sender;
         this.completed = Task.FromResult<IStreamTracker>(this);
      }

      public IStreamSender Sender => sender;

      public bool Settled => settled;

      public IDeliveryState State => state;

      public bool RemoteSettled => true;

      public IDeliveryState RemoteState => ClientAccepted.Instance;

      public Task<IStreamTracker> SettlementTask => completed;

      public IStreamTracker AwaitAccepted()
      {
         return this;
      }

      public IStreamTracker AwaitAccepted(TimeSpan timeout)
      {
         return this;
      }

      public IStreamTracker AwaitSettlement()
      {
         return this;
      }

      public IStreamTracker AwaitSettlement(TimeSpan timeout)
      {
         return this;
      }

      public IStreamTracker Disposition(IDeliveryState state, bool settle)
      {
         this.state = state;
         this.settled = settle;

         return this;
      }

      public Task<IStreamTracker> DispositionAsync(IDeliveryState state, bool settle)
      {
         return Task.FromResult<IStreamTracker>(this);
      }

      public IStreamTracker Settle()
      {
         this.settled = true;

         return this;
      }

      public Task<IStreamTracker> SettleAsync()
      {
         return Task.FromResult<IStreamTracker>(this);
      }
   }
}