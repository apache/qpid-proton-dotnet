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
using System.Linq;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Types
{
   /// <summary>
   /// A representation of the byte value that comprises the delivery tag
   /// that is assigned to the first transfer frame of each new delivery.
   /// </summary>
   public sealed class DeliveryTag : IDeliveryTag
   {
      private static readonly byte[] EmptyTagArray = new byte[0];

      private IProtonBuffer tagBuffer;
      private byte[] tagBytes;

      public DeliveryTag()
      {
         this.tagBytes = EmptyTagArray;
      }

      public DeliveryTag(byte[] tagBytes)
      {
         if (tagBytes == null)
         {
            throw new ArgumentNullException("Provided tag bytes cannot be null");
         }

         this.tagBytes = tagBytes;
      }

      public DeliveryTag(IProtonBuffer tagBuffer)
      {
         if (tagBuffer == null)
         {
            throw new ArgumentNullException("Provided tag buffer cannot be null");
         }

         this.tagBuffer = tagBuffer;
      }

      public object Clone()
      {
         if (tagBuffer != null)
         {
            return new DeliveryTag(tagBuffer.Copy());
         }
         else
         {
            byte[] tagBytesCopy = new byte[tagBytes.Length];
            Array.Copy(tagBytes, tagBytesCopy, tagBytes.Length);
            return new DeliveryTag(tagBytesCopy);
         }
      }

      public int Length => (int)(tagBytes != null ? tagBytes.Length : tagBuffer.ReadableBytes);

      public byte[] TagBytes
      {
         get
         {
            if (tagBytes != null)
            {
               return tagBytes;
            }
            else
            {
               byte[] tagBytesCopy = new byte[tagBuffer.ReadableBytes];
               tagBuffer.CopyInto(tagBuffer.ReadOffset, tagBytesCopy, 0, tagBytesCopy.LongLength);
               return tagBytesCopy;
            }
         }
      }

      public IProtonBuffer TagBuffer
      {
         get
         {
            if (tagBytes != null)
            {
               return ProtonByteBufferAllocator.Instance.Wrap(tagBytes);
            }
            else
            {
               return tagBuffer;
            }
         }
      }

      public bool Equals(IDeliveryTag other)
      {
         if (this == other)
         {
            return true;
         }
         else if (tagBuffer != null)
         {
            return tagBuffer.Equals(other.TagBuffer);
         }
         else
         {
            return tagBytes.SequenceEqual(other.TagBytes);
         }
      }

      public void Release()
      {
         // Nothing to do in this implementation.
      }

      public void WriteTo(IProtonBuffer buffer)
      {
         buffer.EnsureWritable(Length);

         if (tagBytes != null)
         {
            buffer.WriteBytes(tagBytes);
         }
         else
         {
            tagBuffer.CopyInto(tagBuffer.ReadableBytes, buffer, buffer.WriteOffset, tagBuffer.ReadableBytes);
            buffer.WriteOffset += Length;
         }
      }

      public override String ToString()
      {
         return "DeliveryTag: {" + TagBuffer + "}";
      }
   }
}