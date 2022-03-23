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
   /// A single AMQP stream receiver instance
   /// </summary>
   public interface IStreamReceiver : IReceiver
   {
      /// <summary>
      /// Blocking receive method that waits forever for the remote to provide some or all of a delivery
      /// for consumption. This method returns a streamed delivery instance that allows for consumption
      /// or the incoming delivery as it arrives.
      /// </summary>
      /// <remarks>
      /// Receive calls will only grant credit on their own if a credit window is configured in the options
      /// which by default will have been configured.  If the client application has not configured a credit
      /// window then this method won't grant or extend the credit window but will wait for a delivery
      /// regardless. The application needs to arrage for credit to be granted in that case.
      /// </remarks>
      /// <returns>The next available delivery</returns>
      new IStreamDelivery Receive();

      /// <summary>
      /// Blocking receive method that waits for the specified time period for the remote to provide a
      /// delivery for consumption before returning null if none was received. This method returns a
      /// streamed delivery instance that allows for consumption or the incoming delivery as it arrives.
      /// </summary>
      /// <remarks>
      /// Receive calls will only grant credit on their own if a credit window is configured in the options
      /// which by default will have been configured.  If the client application has not configured a credit
      /// window then this method won't grant or extend the credit window but will wait for a delivery
      /// regardless. The application needs to arrage for credit to be granted in that case.
      /// </remarks>
      new IStreamDelivery Receive(TimeSpan timeout);

      /// <summary>
      /// Non-blocking receive method that either returns a delivery is one is immediately available
      /// or returns null if none is currently at hand. This method returns a streamed delivery instance
      /// that allows for consumption or the incoming delivery as it arrives.
      /// </summary>
      /// <returns>A delivery if one is immediately available or null if not</returns>
      new IStreamDelivery TryReceive();

      /// <inheritdoc cref="IReceiver.AddCredit(int)"/>
      new IStreamReceiver AddCredit(uint credit);

      /// <inheritdoc cref="IReceiver.Drain()"/>
      new IStreamReceiver Drain();

      #region Defaults for hidden IReceiver APIs

      IReceiver IReceiver.AddCredit(uint credit)
      {
         return this.AddCredit(credit);
      }

      IReceiver IReceiver.Drain()
      {
         return this.Drain();
      }

      IDelivery IReceiver.Receive()
      {
         return this.Receive();
      }

      IDelivery IReceiver.Receive(TimeSpan timeout)
      {
         return this.Receive(timeout);
      }

      IDelivery IReceiver.TryReceive()
      {
         return this.TryReceive();
      }

      #endregion
   }
}