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

namespace Apache.Qpid.Proton.Types
{
   public sealed class Decimal32 : IComparable<Decimal32>, IEquatable<Decimal32>
   {
      private readonly uint bits;

      public Decimal32(uint bits)
      {
         this.bits = bits;
      }

      public int CompareTo(Decimal32 value)
      {
        return (this.bits < value.bits) ? -1 : ((this.bits == value.bits) ? 0 : 1);
      }

      public bool Equals(Decimal32 obj)
      {
         if (obj == null)
         {
            return false;
         }
         else
         {
            return this.bits == obj.bits;
         }
      }

      public override bool Equals(object obj)
      {
         if (obj == null || GetType() != obj.GetType())
         {
            return false;
         }
         else
         {
            return Equals(obj as Decimal32);
         }
      }

      public override int GetHashCode()
      {
         return this.bits.GetHashCode();
      }
   }
}