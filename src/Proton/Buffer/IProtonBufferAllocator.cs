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

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// Defines the interface for a IProtonBuffer allocator that can be used by
   /// the Proton library to allow customization of the buffer types used for
   /// IO and application level buffer management.
   /// </summary>
   public interface IProtonBufferAllocator
   {
      /// <summary>
      /// Create a new output IProtonBuffer instance with the given initial capacity and the
      /// maximum capacity should be that of the underlying buffer implementations limit.  The
      /// buffer implementation should support growing the buffer on an as needed basis to allow
      /// writes without the user needing to code extra capacity and buffer reallocation checks.
      ///
      /// The returned buffer will be used for frame output from the Proton engine and
      /// can be a pooled buffer which the IO handler will then need to release once
      /// the buffer has been written.
      /// </summary>
      /// <param name="initialCapacity">The initial capacity to use when creating the buffer</param>
      /// <returns>A new buffer instance that has the given initial capacity</returns>
      IProtonBuffer OutputBuffer(long initialCapacity);

      /// <summary>
      /// Create a new output IProtonBuffer instance with the given initial capacity and the
      /// maximum capacity should that of the value specified by the caller.
      ///
      /// The returned buffer will be used for frame output from the Proton engine and
      /// can be a pooled buffer which the IO handler will then need to release once
      /// the buffer has been written.
      /// </summary>
      /// <param name="initialCapacity">The initial capacity to use when creating the buffer</param>
      /// <param name="maxCapacity">The maximum capacity limit for the newly created buffer</param>
      /// <returns>A new buffer instance that has the given initial capacity limits</returns>
      IProtonBuffer OutputBuffer(long initialCapacity, long maxCapacity);

      /// <summary>
      /// Create a new IProtonBuffer instance with default initial capacity.  The buffer
      /// implementation should support growing the buffer on an as needed basis to allow
      /// writes without the user needing to code extra capacity and buffer reallocation
      /// checks.
      ///
      /// It is not recommended that these buffers be backed by a pooled resource as there
      /// is no defined release point within the buffer API and if used by an AMQP engine
      /// they could be lost as buffers are copied or aggregated together.
      /// </summary>
      /// <returns>A new buffer instance that allocates default capacity</returns>
      IProtonBuffer Allocate();

      /// <summary>
      /// Create a new IProtonBuffer instance with the given initial capacity and the
      /// maximum capacity should be that of the underlying buffer implementations
      /// limit.
      ///
      /// It is not recommended that these buffers be backed by a pooled resource as there
      /// is no defined release point within the buffer API and if used by an AMQP engine
      /// they could be lost as buffers are copied or aggregated together.
      /// </summary>
      /// <param name="initialCapacity">The initial capacity to use when creating the buffer</param>
      /// <returns>A new buffer instance that has the given initial capacity</returns>
      IProtonBuffer Allocate(long initialCapacity);

      /// <summary>
      /// Create a new IProtonBuffer instance with the given initial capacity and the
      /// maximum capacity should that of the value specified by the caller.
      ///
      /// It is not recommended that these buffers be backed by a pooled resource as there
      /// is no defined release point within the buffer API and if used by an AMQP engine
      /// they could be lost as buffers are copied or aggregated together.
      /// </summary>
      /// <param name="initialCapacity">The initial capacity to use when creating the buffer</param>
      /// <param name="maxCapacity">The maximum capacity limit for the newly created buffer</param>
      /// <returns>A new buffer instance that has the given initial capacity limits</returns>
      IProtonBuffer Allocate(long initialCapacity, long maxCapacity);

      /// <summary>
      /// Create a new IProtonBuffer that wraps the given byte array.
      ///
      /// The capacity and maximum capacity for the resulting ProtonBuffer should equal
      /// to the length of the wrapped array and the returned array offset is zero.
      /// </summary>
      /// <param name="array">The byte array that will be wrapped</param>
      /// <returns>A new buffer instance that wraps the given byte array</returns>
      IProtonBuffer Wrap(byte[] array);

      /// <summary>
      /// Create a new IProtonBuffer that wraps the given byte array using the provided
      /// offset and length values to confine the view of that array.  The maximum capacity
      /// of the buffer should be that of the length value provided.
      ///
      /// The capacity and maximum capacity for the resulting ProtonBuffer should equal
      /// to the length value provided and the offset should return the value given.
      /// </summary>
      /// <param name="array">The byte array that will be wrapped</param>
      /// <param name="offset">The offset into the array where the buffer begins</param>
      /// <param name="length">The number of bytes that can be operated on by the buffer</param>
      /// <returns>A new buffer instance that wraps the given byte array</returns>
      IProtonBuffer Wrap(byte[] array, int offset, int length);

   }
}
