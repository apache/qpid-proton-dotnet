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

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Defines a state object that is used with the AMQP Encoder type to hold
   /// intermediate state and provide additional functionality that can be used
   /// during the encode process.
   /// </summary>
   public interface IEncoderState
   {
      /// <summary>
      /// Resets the encoder after a complete encode operation freeing any held 
      /// resources and preparing for a new encode operation.
      /// </summary>
      void Reset();

      /// <summary>
      /// Gets the IEncoder instance that was used when creating this encoder state object.
      /// </summary>
      IEncoder Encoder { get; }

      /// <summary>
      /// Encodes the given string into UTF-8 bytes that are written into the provided buffer.
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="value">the string that is to be encoded as a UTF-8 byte sequence</param>
      /// <returns>A reference to the passed buffer where the encoding was done.</returns>
      IProtonBuffer EncodeUtf8(IProtonBuffer buffer, string value);

   }
}
