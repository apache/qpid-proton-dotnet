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

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Defines a Decoder that handles translating the encoded AMQP performative
   /// bytes into the appropriate Proton AMQP types.
   /// </summary>
   public interface IDecoder
   {
      /// <summary>
      /// Creates and returns a new decoder state object that should be used when decoding
      /// values with the decoder instance.
      /// </summary>
      /// <returns></returns>
      IDecoderState NewDecoderState();

      /// <summary>
      /// Returns a cached decoder state instance that can be used be single threaded readers that
      /// use this decoder instance.
      /// </summary>
      /// <returns>A cached decoder state object that can be used by single threaded readerss</returns>
      IDecoderState CachedDecoderState();

   }
}
