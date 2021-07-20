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
using System.Collections;
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Defines an Encoder type that translates AMQP performatives into their
   /// encoded byte representations.
   /// </summary>
   public interface IEncoder
   {
      /// <summary>
      /// Creates and returns a new encoder state object that should be used when encoding
      /// values with the encoder instance.
      /// </summary>
      /// <returns>A new encoder state instance.</returns>
      IEncoderState NewEncoderState();

      /// <summary>
      /// Returns an encoder state instance that lives with this encoder instance. The cached
      /// encoder state should be used with this encoder and only by a single thread at a time.
      /// </summary>
      /// <returns>A cached encoder state instance that can be used by single thread writers</returns>
      IEncoderState CachedEncoderState { get; }

      /// <summary>
      /// Write the indicated AMQP null type encoding into the buffer instance.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteNull(IProtonBuffer buffer, IEncoderState state);

      /// <summary>
      /// Writes the AMQP boolean encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteBoolean(IProtonBuffer buffer, IEncoderState state, bool value);

      /// <summary>
      /// Writes the AMQP unsigned byte encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteUnsignedByte(IProtonBuffer buffer, IEncoderState state, byte value);

      /// <summary>
      /// Writes the AMQP unsigned short encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteUnsignedShort(IProtonBuffer buffer, IEncoderState state, ushort value);

      /// <summary>
      /// Writes the AMQP unsigned int encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteUnsignedInteger(IProtonBuffer buffer, IEncoderState state, uint value);

      /// <summary>
      /// Writes the AMQP unsigned long encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteUnsignedLong(IProtonBuffer buffer, IEncoderState state, ulong value);

      /// <summary>
      /// Writes the AMQP byte encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteByte(IProtonBuffer buffer, IEncoderState state, sbyte value);

      /// <summary>
      /// Writes the AMQP short encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteShort(IProtonBuffer buffer, IEncoderState state, short value);

      /// <summary>
      /// Writes the AMQP integer encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteInteger(IProtonBuffer buffer, IEncoderState state, int value);

      /// <summary>
      /// Writes the AMQP long encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteLong(IProtonBuffer buffer, IEncoderState state, long value);

      /// <summary>
      /// Writes the AMQP float encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteFloat(IProtonBuffer buffer, IEncoderState state, float value);

      /// <summary>
      /// Writes the AMQP double encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteDouble(IProtonBuffer buffer, IEncoderState state, double value);

      /// <summary>
      /// Writes the AMQP Decimal32 encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteDecimal32(IProtonBuffer buffer, IEncoderState state, Decimal32 value);

      /// <summary>
      /// Writes the AMQP Decimal64 encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteDecimal64(IProtonBuffer buffer, IEncoderState state, Decimal64 value);

      /// <summary>
      /// Writes the AMQP Decimal128 encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteDecimal128(IProtonBuffer buffer, IEncoderState state, Decimal128 value);

      /// <summary>
      /// Writes the AMQP Character encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteCharacter(IProtonBuffer buffer, IEncoderState state, char value);

      /// <summary>
      /// Writes the AMQP Timestamp encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteTimestamp(IProtonBuffer buffer, IEncoderState state, long value);

      /// <summary>
      /// Writes the AMQP Timestamp encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteTimestamp(IProtonBuffer buffer, IEncoderState state, ulong value);

      /// <summary>
      /// Writes the AMQP UUID encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteGuid(IProtonBuffer buffer, IEncoderState state, Guid value);

      /// <summary>
      /// Writes the AMQP Binary encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteBinary(IProtonBuffer buffer, IEncoderState state, IProtonBuffer value);

      /// <summary>
      /// Writes the AMQP String encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteString(IProtonBuffer buffer, IEncoderState state, String value);

      /// <summary>
      /// Writes the AMQP Symbol encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteSymbol(IProtonBuffer buffer, IEncoderState state, Symbol value);

      /// <summary>
      /// Writes the AMQP Symbol encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteSymbol(IProtonBuffer buffer, IEncoderState state, String value);

      /// <summary>
      /// Writes the AMQP List encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteList(IProtonBuffer buffer, IEncoderState state, IList value);

      /// <summary>
      /// Writes the AMQP List encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteList<T>(IProtonBuffer buffer, IEncoderState state, IList<T> value);

      /// <summary>
      /// Writes the AMQP Map encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteMap(IProtonBuffer buffer, IEncoderState state, IDictionary value);

      /// <summary>
      /// Writes the AMQP Map encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteMap<K, V>(IProtonBuffer buffer, IEncoderState state, IDictionary<K, V> value);

      /// <summary>
      /// Writes the AMQP Map encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteMap<K, V>(IProtonBuffer buffer, IEncoderState state, IReadOnlyDictionary<K, V> value);

      /// <summary>
      /// Writes the contents of the given IDeliveryTag value into the provided proton buffer
      /// instance as an AMQP Binary type.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The delivery tag to encode as an AMQP Binary</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteDeliveryTag(IProtonBuffer buffer, IEncoderState state, IDeliveryTag value);

      /// <summary>
      /// Writes the AMQP Described Type encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteDescribedType(IProtonBuffer buffer, IEncoderState state, IDescribedType value);

      /// <summary>
      /// Writes the AMQP encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteObject(IProtonBuffer buffer, IEncoderState state, object value);

      /// <summary>
      /// Writes the AMQP array encoding for the given value to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the value encoding to</param>
      /// <param name="state">An encoder state instance to use when encoding</param>
      /// <param name="value">The value to be written</param>
      /// <exception cref="EncodeException">If an error occurs during the encode operation</exception>
      void WriteArray(IProtonBuffer buffer, IEncoderState state, Array value);

      /// <summary>
      /// Allows registration of the given AMQP type encoder into this encoder to customize the
      /// writing of the Type managed by the encoder.
      /// </summary>
      /// <param name="encoder">The encoder type instance to register with this encoder</param>
      /// <returns>This encoder instance</returns>
      IEncoder RegisterDescribedTypeEncoder(IDescribedTypeEncoder encoder);

      /// <summary>
      /// Lookup a Type encoder using an instance of the type to be encoded.
      /// </summary>
      /// <param name="value">an instance of the type whose encoder is being looked up</param>
      /// <returns></returns>
      ITypeEncoder LookupTypeEncoder(Object value);

      /// <summary>
      /// Lookup a Type encoder using Type value of the type to be encoded.
      /// </summary>
      /// <param name="value">a Type instance for the type whose encoder is being looked up</param>
      /// <returns></returns>
      ITypeEncoder LookupTypeEncoder(Type typeClass);

   }
}
