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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// AMQP Sender link resource.
   /// </summary>
   public interface ISender : ILink<ISender>
   {
      /// <summary>
      /// Called when the remote receiver has requested a drain of credit and
      /// the sender has sent all available messages.
      /// </summary>
      /// <returns>This sender instance</returns>
      /// <exception cref="InvalidOperationException">If the link is not draining.</exception>
      ISender Drained();

      /// <summary>
      /// Checks if the sender has credit and the session window allows for any bytes
      /// to be written currently.
      /// </summary>
      bool IsSendable { get; }

      /// <summary>
      /// Gets the current outgoing delivery for this sender if one is available.
      /// <para/>
      /// The sender only tracks a current delivery in the case that the next method has
      /// bee called and if any bytes are written to the delivery using the streaming based
      /// API outgoing delivery stream bytes methods which allows for later writing of
      /// additional bytes to the delivery. Once the write bytes method or the complete API
      /// is called the final transfer is written indicating that the delivery is complete
      /// and the current delivery value is reset. An outgoing delivery that is being streamed
      /// may also be completed by calling the abort method of the outgoing delivery API.
      /// </summary>
      IOutgoingDelivery Current { get; }

      /// <summary>
      /// When there has been no deliveries so far or the current delivery has reached a complete
      /// state this method updates the current delivery to a new instance and returns that value.
      /// If the current delivery has not been completed by either calling a completing API method
      /// then this method will throw an exception to indicate the sender state cannot allow a new
      /// delivery to be started.
      /// </summary>
      /// <exception cref="InvalidOperationException">If a current delivery is still incomplete</exception>
      IOutgoingDelivery Next();

      /// <summary>
      /// For each unsettled outgoing delivery that is pending in the sender apply the given
      /// predicate and if it matches then apply the given delivery state and settled value to it.
      /// </summary>
      /// <param name="filter">The filter predicate that controls when disposition is applied</param>
      /// <param name="state">The delivery state to apply when the predicate matches</param>
      /// <param name="settle">Should the delivery be settled when the predicate matches.</param>
      /// <returns></returns>
      ISender Disposition(Predicate<IOutgoingDelivery> filter, IDeliveryState state, bool settle);

      /// <summary>
      /// For each unsettled outgoing delivery that is pending in the sender apply the given
      /// predicate and if it matches then settle the delivery.
      /// </summary>
      /// <param name="filter"></param>
      /// <returns></returns>
      ISender Settle(Predicate<IOutgoingDelivery> filter);

      /// <summary>
      /// Retrieves the list of unsettled deliveries sent from this sender. The deliveries in the
      /// enumerator cannot be written to but can have their settled state and disposition updated.
      /// Only when this sender settles on its end are the outgoing delivery instances removed from
      /// the unsettled tracking.
      /// </summary>
      IEnumerable<IOutgoingDelivery> Unsettled { get; }

      /// <summary>
      /// Returns true if the sender link is tracking any unsettled sent deliveries.
      /// </summary>
      bool HasUnsettled { get; }

      /// <summary>
      /// Configures a delivery tag generator that will be used to create and set a delivery tag
      /// value on each new outgoing delivery that is created and returned from the sender next
      /// delivery method.
      /// </summary>
      IDeliveryTagGenerator DeliveryTagGenerator { get; set; }

      #region Sender event point APIs

      /// <summary>
      /// Handler for updates for deliveries that have previously been sent.
      /// <para/>
      /// Updates can happen when the remote settles or otherwise modifies the delivery
      /// and the user needs to act on those changes.
      /// </summary>
      /// <param name="handler"></param>
      /// <returns></returns>
      ISender DeliveryStateUpdatedHandler(Action<IOutgoingDelivery> handler);

      #endregion

   }
}