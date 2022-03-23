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
   /// AMQP Receiver link resource.
   /// </summary>
   public interface IReceiver : ILink<IReceiver>
   {
      /// <summary>
      /// Adds the given amount of credit to the receiver's already existing credit
      /// if any.
      /// </summary>
      /// <param name="amount">The amount of credit to add to the existing credit</param>
      /// <returns>This receiver instance</returns>
      IReceiver AddCredit(uint amount);

      /// <summary>
      /// Initiate a drain of all remaining credit of this link, the remote sender
      /// will then either send enough deliveries to fulfill the drain request or
      /// report that it cannot and all remaining link credit will be consumed.
      /// </summary>
      /// <returns>True if a drain request was started or false if no credit exists</returns>
      /// <exception cref="InvalidOperationException">If a drain is already in progress</exception>
      bool Drain();

      /// <summary>
      /// Initiate a drain of the given credit from this this receiver} link.  If the
      /// credit given is greater than the current link credit the current credit is
      /// increased, however if the amount of credit given is less that the current
      /// amount of link credit an exception is thrown.
      /// </summary>
      /// <param name="credit">The amount of credit to drain</param>
      /// <returns>True if a drain request was started or false if no credit exists</returns>
      /// <exception cref="InvalidOperationException">If a drain is already in progress</exception>
      bool Drain(uint credit);

      /// <summary>
      /// Configures a default DeliveryState to be used if a received delivery is
      /// settled/freed without any disposition state having been previously applied.
      /// </summary>
      IDeliveryState DefaultDeliveryState { get; set; }

      /// <summary>
      /// For each unsettled outgoing delivery that is pending in the receiver apply the given
      /// predicate and if it matches then apply the given delivery state and settled value to it.
      /// </summary>
      /// <param name="filter">The filter predicate that controls when disposition is applied</param>
      /// <param name="state">The delivery state to apply when the predicate matches</param>
      /// <param name="settle">Should the delivery be settled when the predicate matches.</param>
      /// <returns>This receiver instance</returns>
      IReceiver Disposition(Predicate<IIncomingDelivery> filter, IDeliveryState state, bool settle);

      /// <summary>
      /// For each unsettled outgoing delivery that is pending in the receiver apply the given
      /// predicate and if it matches then settle the delivery.
      /// </summary>
      /// <param name="filter"></param>
      /// <returns>This receiver instance</returns>
      IReceiver Settle(Predicate<IIncomingDelivery> filter);

      /// <summary>
      /// Retrieves the list of unsettled deliveries sent from this receiver. The deliveries in the
      /// enumerator cannot be written to but can have their settled state and disposition updated.
      /// Only when this receiver settles on its end are the outgoing delivery instances removed from
      /// the unsettled tracking.
      /// </summary>
      IReadOnlyCollection<IIncomingDelivery> Unsettled { get; }

      /// <summary>
      /// Returns true if the receiver link is tracking any unsettled sent deliveries.
      /// </summary>
      bool HasUnsettled { get; }

      #region Receiver delivery event points

      /// <summary>
      /// Handler for incoming deliveries that is called for each incoming transfer frame
      /// that comprises either one complete delivery or a chunk of a split framed transfer.
      /// The handler should check that the delivery being read is partial or not and act
      /// accordingly, as partial deliveries expect additional updates as more frames
      /// comprising that delivery arrive or the remote aborts the transfer.
      /// </summary>
      /// <param name="handler">A delegate that will handle this event</param>
      /// <returns>This receiver instance</returns>
      IReceiver DeliveryReadHandler(Action<IIncomingDelivery> handler);

      /// <summary>
      /// Handler for aborted deliveries that is called for each aborted in-progress delivery.
      /// <para/>
      /// This handler is an optional convenience handler that supplements the standard
      /// delivery read event handler in cases where the users wishes to break out the
      /// processing of inbound delivery data from abort processing. If this handler is not
      /// set the receiver will call the registered delivery read handler if one is set.
      /// </summary>
      /// <param name="handler">A delegate that will handle this event</param>
      /// <returns>This receiver instance</returns>
      IReceiver DeliveryAbortedHandler(Action<IIncomingDelivery> handler);

      /// <summary>
      /// Handler for updates to the remote state of incoming deliveries that have previously
      /// been received.
      /// <para/>
      /// Remote state updates for an previously received delivery can happen when the remote
      /// settles a complete delivery or otherwise modifies the delivery outcome and the user needs
      /// to act on those changes such as a spontaneous update to the delivery state.
      /// </summary>
      /// <param name="handler">A delegate that will handle this event</param>
      /// <returns>This receiver instance</returns>
      IReceiver DeliveryStateUpdatedHandler(Action<IIncomingDelivery> handler);

      #endregion

   }
}