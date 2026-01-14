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
   /// Marker interface for Modified delivery states and outcomes
   /// </summary>
   public interface IModified : IDeliveryState
   {
      /// <summary>
      /// Quick access to determine if the modified outcome indicates the delivery has failed.
      /// </summary>
      bool DeliveryFailed { get; }

      /// <summary>
      /// Quick access to determine if the modified outcome indicates the delivery cannot be redelivered to the target.
      /// </summary>
      bool UndeliverableHere { get; }

      /// <summary>
      /// Returns any delivery annotations that were applied to this modified outcome
      /// </summary>
      IReadOnlyDictionary<string, object> MessageAnnotations { get; }

   }
}