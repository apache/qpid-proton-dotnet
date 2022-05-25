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

using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class Released : IOutcome, IDeliveryState
   {
      public static readonly ulong DescriptorCode = 0x0000000000000026UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:released:list");

      /// <summary>
      /// The singleton instance of Released outcomes and delivery states
      /// </summary>
      public static Released Instance { get; } = new Released();

      private Released()
      {
      }

      public DeliveryStateType Type => DeliveryStateType.Released;

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public override bool Equals(object other)
      {
         return other != null && other.GetType() == GetType();
      }

      public bool Equals(IDeliveryState state)
      {
         return state != null && state.GetType() == GetType();
      }

      public override string ToString()
      {
         return "Released{}";
      }

      public object Clone()
      {
         return this.MemberwiseClone();
      }
   }
}