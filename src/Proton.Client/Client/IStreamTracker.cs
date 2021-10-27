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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Special streaming sender related tracker that is linked to a stream
   /// sender message and provides the tracker functions for those types of
   /// messages.
   /// </summary>
   public interface IStreamTracker : ITracker
   {
      /// <inheritdoc cref="ITracker.Sender"/>
      new IStreamSender Sender { get; }

      /// <inheritdoc cref="ITracker.Settle"/>
      new IStreamTracker Settle();

      /// <inheritdoc cref="ITracker.Disposition(IDeliveryState, bool)"/>
      new IStreamTracker Disposition(IDeliveryState state, bool settle);

      /// <inheritdoc cref="ITracker.AwaitSettlement"/>
      new IStreamTracker AwaitSettlement();

      /// <inheritdoc cref="ITracker.AwaitSettlement(TimeSpan)"/>
      new IStreamTracker AwaitSettlement(TimeSpan timeout);

      /// <inheritdoc cref="ITracker.AwaitAccepted"/>
      new IStreamTracker AwaitAccepted();

      /// <inheritdoc cref="ITracker.AwaitAccepted(TimeSpan)"/>
      new IStreamTracker AwaitAccepted(TimeSpan timeout);

      #region Defaults methods for hidden ITracer APIs

      ISender ITracker.Sender => this.Sender;

      ITracker ITracker.Settle()
      {
         return this.Settle();
      }

      ITracker ITracker.Disposition(IDeliveryState state, bool settle)
      {
         return this.Disposition(state, settle);
      }

      ITracker ITracker.AwaitSettlement()
      {
         return this.AwaitSettlement();
      }

      ITracker ITracker.AwaitSettlement(TimeSpan timeout)
      {
         return this.AwaitSettlement(timeout);
      }

      ITracker ITracker.AwaitAccepted()
      {
         return this.AwaitAccepted();
      }

      ITracker ITracker.AwaitAccepted(TimeSpan timeout)
      {
         return this.AwaitAccepted(timeout);
      }

      #endregion
   }
}