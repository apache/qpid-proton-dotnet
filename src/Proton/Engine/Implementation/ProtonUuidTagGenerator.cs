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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// A builtin proton delivery tag generator that creates tag value backed by a generated
   /// Uuid (Guid) instance.
   /// </summary>
   public sealed class ProtonUuidTagGenerator : IDeliveryTagGenerator
   {
      public static readonly IDeliveryTagGenerator Instance = new ProtonUuidTagGenerator();

      public IDeliveryTag NextTag()
      {
         return new ProtonUuidDeliveryTag();
      }
   }

   internal sealed class ProtonUuidDeliveryTag : IDeliveryTag
   {
      private static readonly int GUID_BYTES = 16;

      private readonly byte[] tagBytes;

      public ProtonUuidDeliveryTag()
      {
         this.tagBytes = Guid.NewGuid().ToByteArray();
      }

      public int Length => GUID_BYTES;

      public byte[] TagBytes => tagBytes;

      public IProtonBuffer TagBuffer => ProtonByteBufferAllocator.Instance.Wrap(tagBytes);

      public object Clone()
      {
         return this.MemberwiseClone();
      }

      public override int GetHashCode()
      {
         return new Guid(tagBytes).GetHashCode();
      }

      public bool Equals(IDeliveryTag other)
      {
         return TagBuffer.Equals(other.TagBuffer);
      }

      public void Release()
      {
      }

      public override string ToString()
      {
         return new Guid(tagBytes).ToString();
      }

      public void WriteTo(IProtonBuffer buffer)
      {
         buffer.EnsureWritable(GUID_BYTES);
         buffer.WriteBytes(tagBytes);
      }
   }
}