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

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class Data : IBodySection<byte[]>
   {
      public static readonly ulong DescriptorCode = 0x0000000000000075UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:data:binary");

      public SectionType Type => SectionType.Data;
      public IProtonBuffer payload;

      public byte[] Value
      {
         get
         {
            byte[] result = null;

            if (payload?.IsReadable ?? false)
            {
               result = new byte[payload.ReadableBytes];
               payload.CopyInto(payload.ReadOffset, result, 0, result.LongLength);
            }

            return result;
         }
         set
         {
            this.payload = value != null ? ProtonByteBufferAllocator.Instance.Wrap(value) : null;
         }
      }

      /// <summary>
      /// Provides a reference to the underlying proton buffer for this Data section
      /// </summary>
      public IProtonBuffer Buffer
      {
         get => payload;
         set => payload = value;
      }

      public Data() : base()
      {
      }

      public Data(byte[] value) : this()
      {
         if (value != null)
         {
            payload = ProtonByteBufferAllocator.Instance.Wrap(value);
         }
      }

      public Data(IProtonBuffer value) : this()
      {
         payload = value;
      }

      public Data(Data other) : this()
      {
         if (other.Buffer != null)
         {
            payload = other.Buffer?.Copy();
         }
      }

      public object Clone()
      {
         return Copy();
      }

      public Data Copy()
      {
         return new Data(this);
      }

      public override string ToString()
      {
         return "Data{ " + Buffer + " }";
      }

      public override int GetHashCode()
      {
         const int prime = 31;
         int result = 1;
         result = prime * result + ((Buffer == null) ? 0 : Buffer.GetHashCode());
         return result;
      }

      public override bool Equals(object other)
      {
         if (other == null || !this.GetType().Equals(other.GetType()))
         {
            return false;
         }
         else
         {
            return Equals((Data)other);
         }
      }

      public bool Equals(Data other)
      {
         if (this == other)
         {
            return true;
         }
         else if (other == null)
         {
            return false;
         }
         else if (Value == null && other.Buffer == null)
         {
            return true;
         }
         else
         {
            return Buffer != null && Buffer.Equals(other.Buffer);
         }
      }

      /// <summary>
      /// Implicit cast operator for Data section to a byte array.
      /// </summary>
      /// <param name="data">The Data section that is being cast</param>
      public static implicit operator byte[](Data data)
      {
         return data.Value;
      }
   }
}