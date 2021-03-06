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
using System.IO;
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Defines a Decoder that handles translating the encoded AMQP performative
   /// bytes into the appropriate Proton AMQP types.
   /// </summary>
   public interface IStreamDecoder
   {
      /// <summary>
      /// Creates and returns a new decoder state object that should be used when decoding
      /// values with the decoder instance.
      /// </summary>
      /// <returns></returns>
      IStreamDecoderState NewDecoderState();

      /// <summary>
      /// Returns a cached decoder state instance that can be used be single threaded readers that
      /// use this decoder instance.
      /// </summary>
      /// <returns>A cached decoder state object that can be used by single threaded readerss</returns>
      IStreamDecoderState CachedDecoderState { get; }

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      bool? ReadBoolean(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      bool ReadBoolean(Stream stream, IStreamDecoderState state, bool defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      sbyte? ReadByte(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      sbyte ReadByte(Stream stream, IStreamDecoderState state, sbyte defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      byte? ReadUnsignedByte(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      byte ReadUnsignedByte(Stream stream, IStreamDecoderState state, byte defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      char? ReadCharacter(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      char ReadCharacter(Stream stream, IStreamDecoderState state, char defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Decimal32 ReadDecimal32(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Decimal64 ReadDecimal64(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Decimal128 ReadDecimal128(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      short? ReadShort(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      short ReadShort(Stream stream, IStreamDecoderState state, short defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ushort? ReadUnsignedShort(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ushort ReadUnsignedShort(Stream stream, IStreamDecoderState state, ushort defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      int? ReadInt(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      int ReadInt(Stream stream, IStreamDecoderState state, int defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      uint? ReadUnsignedInteger(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      uint ReadUnsignedInteger(Stream stream, IStreamDecoderState state, uint defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      long? ReadLong(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      long ReadLong(Stream stream, IStreamDecoderState state, long defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ulong? ReadUnsignedLong(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ulong ReadUnsignedLong(Stream stream, IStreamDecoderState state, ulong defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      float? ReadFloat(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      float ReadFloat(Stream stream, IStreamDecoderState state, float defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      double? ReadDouble(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it or the default if the
      /// encoding type is null. If the next value in  the byte stream is not of the requested type
      /// an error is thrown.  If the caller wishes to recover from errors due to unexpected types
      /// the byte stream should be marked and reset in order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <param name="defaultValue">The default value to return for null encodings</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      double ReadDouble(Stream stream, IStreamDecoderState state, double defaultValue);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IProtonBuffer ReadBinary(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      string ReadString(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Symbol ReadSymbol(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      string ReadSymbolAsString(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      ulong? ReadTimestamp(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      Guid? ReadGuid(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      object ReadObject(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      T ReadObject<T>(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      T[] ReadMultiple<T>(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IDictionary<K, V> ReadMap<K, V>(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded value from the given byte stream and returns it.  If the next value in
      /// the byte stream is not of the requested type an error is thrown.  If the caller wishes to
      /// recover from errors due to unexpected types the byte stream should be marked and reset in
      /// order to make additional read attempts.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IList<V> ReadList<V>(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// This method expects to read a Binary encoded type from the provided stream and
      /// constructs a IDeliveryTag type that wraps the bytes encoded. If the encoding is
      /// NULL AMQP type then this method returns null.
      /// </summary>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <returns>The decoded object or null if the encoding was null.</returns>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IDeliveryTag ReadDeliveryTag(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads from the given IProtonstream instance and returns a ITypeDecoder that can read the
      /// next encoded AMQP type from the byte stream.  If an error occurs while attempting to read
      /// the encoded type a DecodeException is thrown.
      /// </summary>
      /// <typeparam name="T">The type that the decoder handles</typeparam>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IStreamTypeDecoder ReadNextTypeDecoder(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Peeks ahead in the given IProtonstream instance and returns a ITypeDecoder that can read the
      /// next encoded AMQP type from the byte stream. If an error occurs while attempting to read
      /// the encoded type a DecodeException is thrown. The underlying stream state is not modified as
      /// a result of calling the peek ahead operation and the returned decoder will not correctly
      /// be able to read the encoded type until the type encoding bytes are read.
      /// </summary>
      /// <typeparam name="T">The type that the decoder handles</typeparam>
      /// <param name="stream">The stream to read the encoded value from</param>
      /// <param name="state">A decoder state instance to use when decoding</param>
      /// <exception cref="DecodeException">If an error occurs during the decode operation</exception>
      IStreamTypeDecoder PeekNextTypeDecoder(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Allows for a custom described type decoder to be registered with this decoder instance
      /// for use when decoding AMQP described types from incoming byte streams.
      /// </summary>
      /// <param name="decoder">A described type decoder to register</param>
      /// <returns>This IDecoder instance.</returns>
      IStreamDecoder RegisterDescribedTypeDecoder(IStreamDescribedTypeDecoder decoder);

   }
}
