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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Primitives
{
   public sealed class Decimal128 : IComparable, IComparable<Decimal128>, IEquatable<Decimal128>
   {
      public static readonly int Bytes = 16;

      private ulong lsb;
      private ulong msb;

      public Decimal128(ulong msb, ulong lsb)
      {
         this.lsb = lsb;
         this.msb = msb;
      }

      public ulong MostSignificantBits
      {
         get => msb;
         set => msb = value;
      }

      public ulong LeastSignificantBits
      {
         get => lsb;
         set => lsb = value;
      }

      public int CompareTo(object value)
      {
         return CompareTo((Decimal128)value);
      }

      public int CompareTo(Decimal128 value)
      {
         return -1;  // TODO
      }

      public bool Equals(Decimal128 obj)
      {
         if (obj == null)
         {
            return false;
         }
         else
         {
            return this.msb == obj.msb && this.lsb == obj.lsb;
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
            return Equals(obj as Decimal128);
         }
      }

      public override int GetHashCode()
      {
         int result = (int)(msb ^ (msb >> 32));
         result = 31 * result + (int)(lsb ^ (lsb >> 32));
         return result;
      }
   }
}