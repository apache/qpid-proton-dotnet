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

namespace Apache.Qpid.Proton.Types
{
   public sealed class DeliveryTag : IDeliveryTag
   {
      private static readonly byte[] EmptyTagArray = new byte[0];

      private byte[] tagBytes;
      private IProtonBuffer tagView;
      private int hashCode;

      public DeliveryTag()
      {
         this.tagBytes = EmptyTagArray;
      }

      public DeliveryTag(byte[] tagBytes)
      {
         if (tagBytes == null)
         {
            throw new ArgumentNullException("Cannot create a tag with null bytes");
         }

         this.tagBytes = tagBytes;
      }

      public DeliveryTag(IProtonBuffer tagBytes)
      {
         if (tagBytes == null)
         {
            throw new ArgumentNullException("Cannot create a tag with null bytes");
         }

         this.tagBytes = new byte[tagBytes.ReadableBytes];
         tagBytes.CopyInto(tagBytes.ReadOffset, this.tagBytes, 0, tagBytes.ReadableBytes);

         this.tagView = tagBytes;
      }

      public int Length => tagBytes.Length;

      public byte[] TagBytes => (byte[])tagBytes.Clone();

      public IProtonBuffer TagBuffer
      {
         get
         {
            if (tagView == null)
            {
               tagView = ProtonByteBufferAllocator.INSTANCE.Wrap(tagBytes);
            }

            tagView.ReadOffset = 0;
            tagView.WriteOffset = tagBytes.Length;

            return tagView;
         }
      }

      public object Clone()
      {
         return new DeliveryTag((byte[])this.tagBytes.Clone());
      }

      public void Release()
      {
      }

      public void WriteTo(IProtonBuffer buffer)
      {
         buffer.WriteBytes(tagBytes);
      }

      public override string ToString()
      {
         return "DeliveryTag: {" + tagBytes + "}";
      }

      public override int GetHashCode()
      {
         if (hashCode == 0)
         {
            hashCode = TagBuffer.GetHashCode();
         }

         return hashCode;
      }

      public override bool Equals(object obj)
      {
         if (obj is not IDeliveryTag)
         {
            return false;
         }
         else
         {
            return this.Equals((IDeliveryTag)obj);
         }
      }

      public bool Equals(IDeliveryTag other)
      {
         return Array.Equals(tagBytes, other?.TagBytes);
      }
   }
}