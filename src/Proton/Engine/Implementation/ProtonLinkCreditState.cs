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

using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   public sealed class ProtonLinkCreditState : ILinkCreditState
   {
      private uint credit;
      private uint deliveryCount;

      private bool drain;
      private bool echo;

      private bool deliveryCountInitialized;

      private uint remoteDeliveryCount;
      private uint remoteLinkCredit;

      public ProtonLinkCreditState()
      {
         // Nothing to initialize here.
      }

      public ProtonLinkCreditState(uint deliveryCount)
      {
         InitializeDeliveryCount(deliveryCount);
      }

      public uint Credit => credit;

      public uint DeliveryCount => deliveryCount;

      public bool IsDrain => drain;

      public bool IsEcho => echo;

      #region Internal link credit state management

      /// <summary>
      /// Creates a snapshot of the current credit state, a subclass should implement this
      /// method and provide an appropriately populated snapshot of the current state.
      /// </summary>
      internal ILinkCreditState Snapshot()
      {
         return new UnmodifiableLinkCreditState(credit, deliveryCount, drain, echo);
      }

      internal bool HasCredit => credit > 0;

      internal void ClearDrain() => drain = false;

      internal void ClearEcho() => echo = false;

      internal void ClearCredit() => credit = 0;

      internal void IncrementCredit(uint credit) => this.credit += credit;

      internal void DecrementCredit() => credit = credit == 0 ? 0 : credit - 1;

      internal uint IncrementDeliveryCount() => deliveryCount++;

      internal uint IncrementDeliveryCount(uint amount) => deliveryCount += amount;

      internal uint DecrementDeliveryCount() => deliveryCount--;

      internal bool IsDeliveryCountInitialized => deliveryCountInitialized;

      internal void InitializeDeliveryCount(uint deliveryCount)
      {
         this.deliveryCount = deliveryCount;
         deliveryCountInitialized = true;
      }

      internal void UpdateCredit(uint effectiveCredit) => credit = effectiveCredit;

      internal void UpdateDeliveryCount(uint newDeliveryCount) => deliveryCount = newDeliveryCount;

      internal void RemoteFlow(Flow flow)
      {
         remoteDeliveryCount = flow.DeliveryCount;
         remoteLinkCredit = flow.LinkCredit;
         echo = flow.Echo;
         drain = flow.Drain;
      }

      #endregion
   }

   /// <summary>
   /// An unmodifiable snapshot view of link credit state.
   /// </summary>
   public sealed class UnmodifiableLinkCreditState : ILinkCreditState
   {
      private readonly uint credit;
      private readonly uint deliveryCount;
      private readonly bool drain;
      private readonly bool echo;

      public UnmodifiableLinkCreditState(uint credit, uint deliveryCount, bool drain, bool echo)
      {
         this.credit = credit;
         this.deliveryCount = deliveryCount;
         this.drain = drain;
         this.echo = echo;
      }

      public uint Credit => credit;

      public uint DeliveryCount => deliveryCount;

      public bool IsDrain => drain;

      public bool IsEcho => echo;

   }
}