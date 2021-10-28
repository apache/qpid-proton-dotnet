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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// A builtin proton delivery tag generator that creates tag values from an ever increasing
   /// sequence id value.
   /// </summary>
   public class ProtonSequentialTagGenerator : IDeliveryTagGenerator
   {
      protected ulong nextTagId = 0ul;

      public virtual IDeliveryTag NextTag()
      {
         return new ProtonNumericDeliveryTag(nextTagId++);
      }

      /// <summary>
      /// Internal tag value accessor for testing validation
      /// </summary>
      public ulong NextTagId
      {
         get => nextTagId;
         set => nextTagId = value;
      }
   }

   internal class ProtonNumericDeliveryTag : IDeliveryTag
   {
      protected readonly ulong tagValue;

      public ProtonNumericDeliveryTag(ulong tagValue)
      {
         this.tagValue = tagValue;
      }

      public int Length
      {
         get
         {
            if (tagValue <= 0x00000000000000FFul)
            {
               return sizeof(byte);
            }
            else if (tagValue <= 0x000000000000FFFFul)
            {
               return sizeof(ushort);
            }
            else if (tagValue <= 0x00000000FFFFFFFFul)
            {
               return sizeof(uint);
            }
            else
            {
               return sizeof(ulong);
            }
         }
      }

      public byte[] TagBytes
      {
         get
         {
            if (tagValue <= 0x00000000000000FFul)
            {
               return ProtonByteUtils.ToByteArray((byte)tagValue);
            }
            else if (tagValue <= 0x000000000000FFFFul)
            {
               return ProtonByteUtils.ToByteArray((short)tagValue);
            }
            else if (tagValue <= 0x00000000FFFFFFFFul)
            {
               return ProtonByteUtils.ToByteArray((int)tagValue);
            }
            else
            {
               return ProtonByteUtils.ToByteArray(tagValue);
            }
         }
      }

      public IProtonBuffer TagBuffer => ProtonByteBufferAllocator.Instance.Wrap(TagBytes);

      public object Clone()
      {
         return this.MemberwiseClone();
      }

      public override int GetHashCode()
      {
         return tagValue.GetHashCode();
      }

      public bool Equals(IDeliveryTag other)
      {
         if (this == other)
         {
            return true;
         }
         if (other == null)
         {
            return false;
         }
         if (GetType() != other.GetType())
         {
            return false;
         }

         ProtonNumericDeliveryTag numericTag = (ProtonNumericDeliveryTag)other;
         if (tagValue != numericTag.tagValue)
         {
            return false;
         }

         return true;
      }

      public virtual void Release()
      {
      }

      public void WriteTo(IProtonBuffer buffer)
      {
         if (tagValue <= 0x00000000000000FFul)
         {
            buffer.WriteUnsignedByte((byte)tagValue);
         }
         else if (tagValue <= 0x000000000000FFFFul)
         {
            buffer.WriteUnsignedShort((ushort)tagValue);
         }
         else if (tagValue <= 0x00000000FFFFFFFFul)
         {
            buffer.WriteUnsignedInt((uint)tagValue);
         }
         else
         {
            buffer.WriteUnsignedLong(tagValue);
         }
      }
   }
}