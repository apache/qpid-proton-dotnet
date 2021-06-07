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
   public interface IProtonBuffer
   {
      /// <summary>
      /// Returns if this buffer implementation has a backing byte array.  If it does
      /// than the various array access methods will allow calls, otherwise an exception
      /// will be thrown if there is no backing array and an access operation occurs.
      /// </summary>
      /// <returns>true if the buffer has a backing byte array</returns>
      bool HasArray();

      /// <summary>
      /// If the buffer implementation has a backing array this method returns that array
      /// this method returns it, otherwise it will throw an exception to indicate that.
      /// </summary>
      byte[] Array { get; }

      /// <summary>
      /// If the buffer implementation has a backing array this method returns that array
      /// offset used to govern where reads and writes start in the wrapped array, otherwise 
      /// it will throw an exception to indicate that.
      /// </summary>
      int ArrayOffset { get; }

   }
}