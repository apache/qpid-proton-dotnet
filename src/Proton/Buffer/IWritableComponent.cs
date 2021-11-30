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
using System.Text;

namespace Apache.Qpid.Proton.Buffer
{
   public interface IWritableComponent
   {
      /// <summary>
      /// Determines if the component is back by a byte array that has writable bytes.
      /// </summary>
      bool HasWritableArray { get; }

      /// <summary>
      /// Access the Writable array that backs this component if one exists otherwise
      /// throw an invalid operation exception to indicate that there is no writable
      /// byte array.
      /// </summary>
      /// <remarks>
      /// If a byte array is returned from this method it should never be used to
      /// make modifications to the proton buffer data.
      /// </remarks>
      byte[] WritableArray { get; }

      /// <summary>
      /// Access the offset into the writable backing array if one exists otherwise
      /// throws an invalid operation exception to indicate that there is no writable
      /// byte array.
      /// </summary>
      int WritableArrayOffset { get; }

      /// <summary>
      /// Access the usable length of the writable backing array if one exists otherwise
      /// throws an invalid operation exception to indicate that there is no writable
      /// byte array.
      /// </summary>
      int WritableArrayLength { get; }

   }
}