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
   /// <summary>
   /// A representation of the byte value that comprises the delivery tag
   /// that is assigned to the first transfer frame of each new delivery.
   /// </summary>
   public interface IDeliveryTag : ICloneable
   {
      /// <summary>
      /// The number of bytes that comprise the delivery tag body
      /// </summary>
      int Length { get; }

      /// <summary>
      /// Returns a view of this {@link DeliveryTag} object as a byte array.  The returned
      /// array may be the actual underlying tag bytes or a synthetic view based on the
      /// value used to generate  the tag.  It is advised not to modify the returned value
      /// and copy if such modification are necessary to the caller.
      /// </summary>
      /// <returns></returns>
      byte[] TagBytes { get; }

      /// <summary>
      /// Returns a view of this IDeliveryTag} object as a IProtonBuffer. The returned array
      /// may be the actual underlying tag bytes or a synthetic view based on the value used
      /// to generate the tag.  It is advised not to modify the returned value and copy if
      /// such modification are necessary to the caller.
      /// </summary>
      IProtonBuffer TagBuffer { get; }

      /// <summary>
      /// Optional method used by tag implementations that provide pooling of tags.
      /// Implementations can do nothing here if no release mechanics are needed.
      /// </summary>
      void Release();

      /// <summary>
      /// Writes the tag as a sequence of bytes into the given buffer in the manner most
      /// efficient for the underlying IDeliveryTag implementation.
      /// </summary>
      /// <param name="buffer"></param>
      void WriteTo(IProtonBuffer buffer);

   }
}