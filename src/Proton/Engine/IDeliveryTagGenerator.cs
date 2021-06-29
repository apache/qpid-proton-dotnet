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

using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Delivery tag generators can be assigned to sender links in order to
   /// allow the link to automatically assign a transfer tag to each outbound
   /// delivery. Depending on the Sender different tag generators can operate in
   /// a fashion that is most efficient for that link such as caching tags for
   /// links that will produce a large number of messages to avoid GC overhead,
   /// while for other links simpler generator types could be used.
   /// </summary>
   public interface IDeliveryTagGenerator
   {
      /// <summary>
      /// Creates and returns the next Delivery Tag that should be used when
      /// populating an outgoing delivery.
      /// </summary>
      /// <returns>The next delivery tag for use when sending a delivery</returns>
      IDeliveryTag NextTag();

   }
}