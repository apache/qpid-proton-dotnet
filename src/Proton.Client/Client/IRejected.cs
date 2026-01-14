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
   /// Marker interface for Rejected delivery states and outcomes
   /// </summary>
   public interface IRejected : IDeliveryState
   {
      /// <summary>
      /// Quick access to any condition value provided in the error condition sent with the rejected outcome.
      /// </summary>
      string Condition { get; }

      /// <summary>
      /// Quick access to any description value provided in the error condition sent with the rejected outcome.
      /// </summary>
      string Description { get; }

      /// <summary>
      /// Returns any information entries that were applied to this rejected outcome in the error condition.
      /// </summary>
      IReadOnlyDictionary<string, object> Info { get; }

   }
}