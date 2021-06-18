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

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// An buffer allocator instance that creates heap based buffer objects
   /// </summary>
   public class ProtonByteBufferAllocator : IProtonBufferAllocator
   {
      public static readonly ProtonByteBufferAllocator INSTANCE = new ProtonByteBufferAllocator();

      public IProtonBuffer OutputBuffer(int initialCapacity)
      {
         return OutputBuffer(initialCapacity, Int32.MaxValue);
      }

      public IProtonBuffer OutputBuffer(int initialCapacity, int maxCapacity)
      {
         return null; // TODO
      }

      public IProtonBuffer Allocate()
      {
         return null; // TODO
      }

      public IProtonBuffer Allocate(int initialCapacity)
      {
         return Allocate(initialCapacity, Int32.MaxValue);
      }

      public IProtonBuffer Allocate(int initialCapacity, int maxCapacity)
      {
         return null; // TODO
      }

      public IProtonBuffer Wrap(byte[] buffer)
      {
         return Wrap(buffer, 0, buffer.Length);
      }

      public IProtonBuffer Wrap(byte[] buffer, int offset, int length)
      {
         return null; // TODO
      }
   }
}