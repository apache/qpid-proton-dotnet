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

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Creates instance of delivery tags that have an empty byte buffer body
   /// </summary>
   public sealed class ProtonEmptyTagGenerator : IDeliveryTagGenerator
   {
      private static readonly byte[] EMPTY_BYTE_ARRAY = System.Array.Empty<byte>();
      private static readonly IDeliveryTag EMPTY_DELIVERY_TAG = new DeliveryTag(EMPTY_BYTE_ARRAY);

      public static ProtonEmptyTagGenerator Instance = new();

      public IDeliveryTag NextTag()
      {
         return EMPTY_DELIVERY_TAG;
      }
   }
}