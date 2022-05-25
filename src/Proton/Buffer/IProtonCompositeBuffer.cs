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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// A composite buffer is used to make a collection of other proton buffer instances
   /// appear as one cohesive buffer which allows the user to remain ignorant of the
   /// underlying buffer structure and in most cases does not require any special handling
   /// by the user.
   /// </summary>
   public interface IProtonCompositeBuffer : IProtonBuffer
   {
      /// <summary>
      /// Creates a new empty composite buffer instance that can be extended with
      /// additional buffers or used directly to write initial data at which point
      /// the provided allocator will be used to create a buffer instance. At any
      /// point should the composite buffer need to allocate a new buffer in order
      /// to accommodate buffer writes a default buffer allocator will be used.
      /// </summary>
      /// <returns>a new empty composite buffer instance.</returns>
      /// <exception cref="NotImplementedException"></exception>
      public static IProtonCompositeBuffer Compose()
      {
         return new ProtonCompositeBuffer(ProtonByteBufferAllocator.Instance);
      }

      /// <summary>
      /// Creates a new empty composite buffer instance that can be extended with
      /// additional buffers or used directly to write initial data at which point
      /// the provided allocator will be used to create a buffer instance. At any
      /// point should the composite buffer need to allocate a new buffer in order
      /// to accommodate buffer writes a default buffer allocator will be used.
      /// </summary>
      /// <param name="maxCapacity">The maximum capacity this composite can contain</param>
      /// <returns>a new empty composite buffer instance.</returns>
      /// <exception cref="NotImplementedException"></exception>
      public static IProtonCompositeBuffer Compose(long maxCapacity)
      {
         return new ProtonCompositeBuffer(maxCapacity);
      }

      /// <summary>
      /// Creates a new empty composite buffer instance that can be extended with
      /// additional buffers or used directly to write initial data at which point
      /// the provided allocator will be used to create a buffer instance. At any
      /// point should the composite buffer need to allocate a new buffer in order
      /// to accommodate buffer writes the provided allocator will be used.
      /// </summary>
      /// <param name="allocator">A buffer allocator that the composite should use</param>
      /// <returns>a new empty composite buffer instance.</returns>
      /// <exception cref="NotImplementedException"></exception>
      public static IProtonCompositeBuffer Compose(IProtonBufferAllocator allocator)
      {
         return new ProtonCompositeBuffer(allocator);
      }

      /// <summary>
      /// Creates a new composite buffer that is composed of the given enumeration of buffers.
      /// </summary>
      /// <remarks>
      /// The provided buffers must adhere to the buffer consistency guidelines of the buffer
      /// implementation that is used to create the composite buffer, if they do not an exception
      /// may be thrown that indicates the nature of the violation.
      /// </remarks>
      /// <param name="allocator">A buffer allocator that the composite should use</param>
      /// <param name="buffers">The enumeration of buffers to compose the new composite</param>
      /// <returns>a new empty composite buffer instance.</returns>
      public static IProtonCompositeBuffer Compose(IProtonBufferAllocator allocator, IEnumerable<IProtonBuffer> buffers)
      {
         return new ProtonCompositeBuffer(allocator, buffers);
      }

      /// <summary>
      /// Creates a new composite buffer that is composed of the given enumeration of buffers.
      /// </summary>
      /// <remarks>
      /// The provided buffers must adhere to the buffer consistency guidelines of the buffer
      /// implementation that is used to create the composite buffer, if they do not an exception
      /// may be thrown that indicates the nature of the violation.
      /// </remarks>
      /// <param name="allocator">A buffer allocator that the composite should use</param>
      /// <param name="buffers">The enumeration of buffers to compose the new composite</param>
      /// <returns>a new empty composite buffer instance.</returns>
      public static IProtonCompositeBuffer Compose(IProtonBufferAllocator allocator, params IProtonBuffer[] buffers)
      {
         return new ProtonCompositeBuffer(allocator, buffers);
      }

      /// <summary>
      /// Creates a new composite buffer that is composed of the given enumeration of buffers.
      /// </summary>
      /// <remarks>
      /// The provided buffers must adhere to the buffer consistency guidelines of the buffer
      /// implementation that is used to create the composite buffer, if they do not an exception
      /// may be thrown that indicates the nature of the violation.
      /// </remarks>
      /// <param name="buffers">The enumeration of buffers to compose the new composite</param>
      /// <returns>a new empty composite buffer instance.</returns>
      public static IProtonCompositeBuffer Compose(params IProtonBuffer[] buffers)
      {
         return new ProtonCompositeBuffer(ProtonByteBufferAllocator.Instance, buffers);
      }

      /// <summary>
      /// Creates a new composite buffer that is composed of the given enumeration of buffers.
      /// </summary>
      /// <remarks>
      /// The provided buffers must adhere to the buffer consistency guidelines of the buffer
      /// implementation that is used to create the composite buffer, if they do not an exception
      /// may be thrown that indicates the nature of the violation.
      /// </remarks>
      /// <param name="buffers">The enumeration of buffers to compose the new composite</param>
      /// <returns>a new empty composite buffer instance.</returns>
      public static IProtonCompositeBuffer Compose(IEnumerable<IProtonBuffer> buffers)
      {
         return new ProtonCompositeBuffer(ProtonByteBufferAllocator.Instance, buffers);
      }

      /// <summary>
      /// Simpler helper API for user to check if a given buffer is indeed a composite
      /// buffer instance.
      /// </summary>
      /// <param name="buffer">The buffer to check</param>
      /// <returns>true if the given buffer implements the composite buffer interface.</returns>
      public static bool IsComposite(IProtonBuffer buffer)
      {
         return buffer is IProtonCompositeBuffer;
      }

      /// <summary>
      /// Appends the provided buffer to the end of the list of composite buffers already
      /// managed by this buffer.  The given buffer becomes the property of the composite
      /// and the constraints placed upon buffers managed by the composite implementation
      /// are extended to this appended buffer.
      /// </summary>
      /// <param name="extension">The buffer to append to the end of the composite set</param>
      /// <returns>This composite buffer instance.</returns>
      public IProtonCompositeBuffer Append(IProtonBuffer extension);

      /// <summary>
      /// Serves as a means of breaking up a composite buffer into an enumeration of the
      /// constituent buffers that is manages. Decomposing a buffer serves to consume the
      /// buffer leaving it an empty state, and the returned buffers are considered the
      /// property of the caller.
      /// </summary>
      /// <returns></returns>
      IEnumerable<IProtonBuffer> DecomposeBuffer();

   }
}