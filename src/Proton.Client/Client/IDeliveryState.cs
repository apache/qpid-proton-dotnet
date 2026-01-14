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
using Apache.Qpid.Proton.Client.Implementation;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Delivery state instance have a type (e.g. Accepted, Rejected ...) and can in
   /// case of a transactional delivery state, carry an outcome and in those cases the is
   /// outcome APIs return the value of the nested outcome carried in the transactional
   /// </summary>
   public interface IDeliveryState
   {
      /// <summary>
      /// Returns an enumeration value which indicates what type of DeliveryState
      /// this instance represents.
      /// </summary>
      DeliveryStateType Type { get; }

      /// <summary>
      /// Quick access to determine if the state value indicates the delivery was accepted.
      /// </summary>
      bool IsAccepted { get; }

      /// <summary>
      /// Quick access to determine if the state value indicates the delivery was rejected.
      /// </summary>
      bool IsRejected { get; }

      /// <summary>
      /// Quick access to determine if the state value indicates the delivery was released.
      /// </summary>
      bool IsReleased { get; }

      /// <summary>
      /// Quick access to determine if the state value indicates the delivery was modified.
      /// </summary>
      bool IsModified { get; }

      /// <summary>
      /// Quick access to determine if the state value indicates the delivery was transactional.
      /// </summary>
      bool IsTransactional { get; }

      /// <summary>
      /// Returns an instance of a delivery state that accepts a delivery
      /// </summary>
      /// <returns>An accepted delivery state type</returns>
      static IDeliveryState Accepted() => ClientAccepted.Instance;

      /// <summary>
      /// Returns an instance of a delivery state that releases a delivery
      /// </summary>
      /// <returns>An released delivery state type</returns>
      static IDeliveryState Released() => ClientReleased.Instance;

      /// <summary>
      /// Returns an instance of a delivery state that rejects a delivery
      /// </summary>
      /// <returns>An rejected delivery state type</returns>
      static IDeliveryState Rejected(string condition, string description = null, IDictionary<string, object> info = null) => new ClientRejected(condition, description, info);

      /// <summary>
      /// Returns an instance of a delivery state that modifies a delivery
      /// </summary>
      /// <returns>An modified delivery state type</returns>
      static IDeliveryState Modified(bool deliveryFailed, bool undeliverableHere = false, IDictionary<string, object> annotations = null) => new ClientModified(deliveryFailed, undeliverableHere, annotations);

   }
}