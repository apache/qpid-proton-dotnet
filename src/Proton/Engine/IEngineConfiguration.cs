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

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Configuration options for the Engine
   /// </summary>
   public interface IEngineConfiguration
   {
      /// <summary>
      /// Sets the ProtonBufferAllocator used by this Engine.
      /// </summary>
      /// <remarks>
      /// When copying data, encoding types or otherwise needing to allocate memory
      /// storage the Engine will use the assigned IProtonBufferAllocator. If no
      /// allocator is assigned the Engine will use the default allocator.
      /// </remarks>
      IProtonBufferAllocator BufferAllocator { get; set; }

      /// <summary>
      /// Enables AMQP frame tracing from engine to the system output.  Depending
      /// on the underlying engine composition frame tracing may not be possible
      /// in which case this method will have no effect and the access method
      /// that read the state will return false.
      /// </summary>
      bool TraceFrames { get; set; }

   }
}
