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

using Apache.Qpid.Proton.Codec.Encoders;
using Apache.Qpid.Proton.Codec.Decoders;

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// A CodecFactory provides a means of creating built in or custom
   /// Proton AMQP type encoders and decoders.
   /// </summary>
   public static class CodecFactory
   {
      private static IDecoder amqpDecoder;
      private static IEncoder amqpEncoder;
      private static IDecoder saslDecoder;
      private static IEncoder saslEncoder;

      /// <summary>
      /// Get and Set the AMQP type Encoder to use when encoding AMQP types.  If no default
      /// Encoder is set then the library will return a default Proton implementation when
      /// the get method is called.
      /// </summary>
      public static IEncoder Encoder
      {
         get
         {
            return amqpEncoder ?? DefaultEncoder;
         }

         set
         {
            amqpEncoder = value;
         }
      }

      /// <summary>
      /// Get and Set the AMQP type Decoder to use when encoding AMQP types.  If no default
      /// Decoder is set then the library will return a default Proton implementation when
      /// the get method is called.
      /// </summary>
      public static IDecoder Decoder
      { 
         get
         {
            return amqpDecoder ?? DefaultDecoder;
         }

         set
         {
            amqpDecoder = value;
         }         
      }

      /// <summary>
      /// Get and Set the SASL type Encoder to use when encoding SASL types.  If no default
      /// Encoder is set then the library will return a default Proton implementation when
      /// the get method is called.
      /// </summary>
      public static IEncoder SaslEncoder
      {
         get
         {
            return saslEncoder ?? DefaultSaslEncoder;
         }

         set
         {
            saslEncoder = value;
         }
      }

      /// <summary>
      /// Get and Set the SASL type Decoder to use when encoding SASL types.  If no default
      /// Encoder is set then the library will return a default Proton implementation when
      /// the get method is called.
      /// </summary>
      public static IDecoder SaslDecoder
      { 
         get
         {
            return saslDecoder ?? DefaultSaslDecoder;
         }

         set
         {
            saslDecoder = value;
         }         
      }

      /// <summary>
      /// Creates a new AMQP type encoder using the Proton builtin Encoder implementation.
      /// </summary>
      public static IEncoder DefaultEncoder
      {
         get
         {
            return ProtonEncoderFactory.Create();
         }
      }

      /// <summary>
      /// Creates a new AMQP type decoder using the Proton builtin Decoder implementation.
      /// </summary>
      public static IDecoder DefaultDecoder
      {
         get
         {
            return ProtonDecoderFactory.Create();
         }
      }

      /// <summary>
      /// Creates a new SASL type encoder using the Proton builtin Encoder implementation.
      /// </summary>
      public static IEncoder DefaultSaslEncoder
      {
         get
         {
            return ProtonEncoderFactory.CreateSasl();
         }
      }

      /// <summary>
      /// Creates a new SASL type decoder using the Proton builtin Decoder implementation.
      /// </summary>
      public static IDecoder DefaultSaslDecoder
      {
         get
         {
            return ProtonDecoderFactory.CreateSasl();
         }
      }
   }
}
