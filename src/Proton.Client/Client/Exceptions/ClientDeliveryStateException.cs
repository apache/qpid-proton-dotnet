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

namespace Apache.Qpid.Proton.Client.Exceptions
{
   /// <summary>
   /// Thrown from client API that deal with a Delivery or Tracker where the outcome
   /// that results from that API can affect whether the API call succeeded or failed.
   /// Such a case might be that a sent message is awaiting a remote Accepted outcome
   /// but instead the remote sends Rejected outcome.
   /// </summary>
   public class ClientDeliveryStateException : ClientIllegalStateException
   {
      private readonly IDeliveryState deliveryState;

      /// <summary>
      /// Create a new instance of the client delivery state error.
      /// </summary>
      /// <param name="message">The message that describes the cause of the error</param>
      /// <param name="deliveryState">The DeliveryState that caused the error</param>
      public ClientDeliveryStateException(string message, IDeliveryState deliveryState) : base(message)
      {
         this.deliveryState = deliveryState;
      }

      /// <summary>
      /// Create a new instance of the client delivery state error.
      /// </summary>
      /// <param name="message">The message that describes the cause of the error</param>
      /// <param name="innerException">The exception that initially triggered this error</param>
      /// <param name="deliveryState">The DeliveryState that caused the error</param>
      public ClientDeliveryStateException(string message, Exception innerException, IDeliveryState deliveryState) : base(message, innerException)
      {
         this.deliveryState = deliveryState;
      }

      /// <summary>
      /// Returns the delivery state object that triggered this error.
      /// </summary>
      public IDeliveryState DeliveryState => deliveryState;

   }
}