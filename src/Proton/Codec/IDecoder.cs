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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

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

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      bool? ReadBoolean(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      bool ReadBoolean(IProtonBuffer buffer, IDecoderState state, bool defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      sbyte? ReadByte(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      sbyte ReadByte(IProtonBuffer buffer, IDecoderState state, sbyte defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      byte? ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      byte ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state, byte defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      char? ReadCharacter(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      char ReadCharacter(IProtonBuffer buffer, IDecoderState state, char defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Decimal32 ReadDecimal32(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Decimal64 ReadDecimal64(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Decimal128 ReadDecimal128(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      short? ReadShort(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      short ReadShort(IProtonBuffer buffer, IDecoderState state, short defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ushort? ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ushort ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state, ushort defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      int? ReadInt(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      int ReadInt(IProtonBuffer buffer, IDecoderState state, int defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      uint? ReadUnsignedInt(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      uint ReadUnsignedInt(IProtonBuffer buffer, IDecoderState state, uint defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      long? ReadLong(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      long ReadLong(IProtonBuffer buffer, IDecoderState state, long defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ulong? ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ulong ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state, ulong defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      float? ReadFloat(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      float ReadFloat(IProtonBuffer buffer, IDecoderState state, float defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      double? ReadDouble(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      double ReadDouble(IProtonBuffer buffer, IDecoderState state, double defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IProtonBuffer ReadBinary(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      string ReadString(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Symbol ReadSymbol(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      string ReadSymbolAsString(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ulong? ReadTimestamp(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Guid? ReadGuid(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      object ReadObject(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      T ReadObject<T>(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      T[] ReadMultiple<T>(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IDictionary<K, V> ReadMap<K, V>(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte buffer and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IList<V> ReadList<V>(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads from the given IProtonBuffer instance and returns a ITypeDecoder that can read the
      /// next encoded AMQP type from the byte stream.  If an error occurs while attempting to read
      /// the encoded type a DecodeException is thrown.
      /// </summary>
      /// <typeparam name="T">The type that the decoder handles</typeparam>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ITypeDecoder ReadNextTypeDecoder(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Peeks ahead in the given IProtonBuffer instance and returns a ITypeDecoder that can read the
      /// next encoded AMQP type from the byte stream. If an error occurs while attempting to read
      /// the encoded type a DecodeException is thrown. The underlying buffer state is not modified as
      /// a result of calling the peek ahead operation and the returned decoder will not correctly
      /// be able to read the encoded type until the type encoding bytes are read.
      /// </summary>
      /// <typeparam name="T">The type that the decoder handles</typeparam>
      /// <param name="buffer">The buffer to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ITypeDecoder PeekNextTypeDecoder(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Allows for a custom described type decoder to be registered with this decoder instance
      /// for use when decoding AMQP described types from incoming byte streams.
      /// </summary>
      /// <typeparam name="V">Type that the decoder manages</typeparam>
      /// <param name="decoder">A described type decoder to register</param>
      /// <returns>This IDecoder instance.</returns>
      IDecoder RegisterDescribedTypeDecoder(IDescribedTypeDecoder decoder);

   }
}
