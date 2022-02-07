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
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientStreamTracker : ClientTrackable, IStreamTracker
   {
      internal ClientStreamTracker(ClientStreamSender sender, IOutgoingDelivery delivery) : base(sender, delivery)
      {
      }

      IStreamSender IStreamTracker.Sender => (IStreamSender)base.Sender;

      public override IStreamTracker Disposition(IDeliveryState state, bool settle)
      {
         return (IStreamTracker)base.Disposition(state, settle);
      }

      public override IStreamTracker Settle()
      {
         return (IStreamTracker)base.Settle();
      }

      public override IStreamTracker AwaitAccepted()
      {
         return (IStreamTracker)base.AwaitAccepted();
      }

      public override IStreamTracker AwaitAccepted(TimeSpan timeout)
      {
         return (IStreamTracker)base.AwaitAccepted(timeout);
      }

      public override IStreamTracker AwaitSettlement()
      {
         return (IStreamTracker)base.AwaitSettlement();
      }

      public override IStreamTracker AwaitSettlement(TimeSpan timeout)
      {
         return (IStreamTracker)base.AwaitSettlement(timeout);
      }
   }
}