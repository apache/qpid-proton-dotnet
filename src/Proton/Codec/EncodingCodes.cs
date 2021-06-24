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
   /// Enumeration of the AMQP type encoding codes
   /// </summary>
   public enum EncodingCodes : byte
   {
      /// <summary>
      /// Indicates that the value that follows will be an AMQP Described Type
      /// </summary>
      DescribedTypeIndicator = 0x0,

      /// <summary>
      /// Indicates that the value encoded was null
      /// </summary>
      Null = 0x40,

      /// <summary>
      /// Indicates the byte that follows indicates a Boolean true or false value.
      /// </summary>
      Boolean = 0x56,

      /// <summary>
      /// A compact encoding for a boolean value that directly implies a value of true.
      /// </summary>
      BooleanTrue = 0x41,

      /// <summary>
      /// A compact encoding for a boolean value that directly implies a value of false.
      /// </summary>
      BooleanFalse = 0x42,

      /// <summary>
      /// Indicates that the next byte represents an unsigned byte value.
      /// </summary>
      UByte = 0x50,

      /// <summary>
      /// Indicates that the next two bytes represents an unsigned short value.
      /// </summary>
      UShort = 0x60,

      /// <summary>
      /// Indicates that the next four bytes represents an unsigned integer value.
      /// </summary>
      UInt = 0x70,

      /// <summary>
      /// Compact encoding where the next byte represents an unsigned integer value.
      /// </summary>
      SmallUInt = 0x52,

      /// <summary>
      /// Compact encoding for an unsigned integer whose value is zero.
      /// </summary>
      UInt0 = 0x43,

      /// <summary>
      /// Indicates that the next eight bytes represents an unsigned long value.
      /// </summary>
      ULong = 0x80,

      /// <summary>
      /// Compact encoding where the next byte represents an unsigned long value.
      /// </summary>
      SmallULong = 0x53,

      /// <summary>
      /// Compact encoding for an unsigned long whose value is zero.
      /// </summary>
      ULong0 = 0x44,

      /// <summary>
      /// Indicates that the next byte represents an signed byte value.
      /// </summary>
      Byte = 0x51,

      /// <summary>
      /// Indicates that the next two bytes represents an signed short value.
      /// </summary>
      Short = 0x61,

      /// <summary>
      /// Indicates that the next four bytes represents an signed integer value.
      /// </summary>
      Int = 0x71,

      /// <summary>
      /// Compact encoding where the next byte represents a signed integer value.
      /// </summary>
      SmallInt = 0x54,

      /// <summary>
      /// Indicates that the next eight bytes represents an signed long value.
      /// </summary>
      Long = 0x81,

      /// <summary>
      /// Compact encoding where the next byte represents a signed long value.
      /// </summary>
      SmallLong = 0x55,

      /// <summary>
      /// Indicates that the next four bytes represents an float value.
      /// </summary>
      Float = 0x72,

      /// <summary>
      /// Indicates that the next eight bytes represents an double value.
      /// </summary>
      Double = 0x82,

      /// <summary>
      /// Indicates that the next four bytes represents an AMQP Decimal32 value.
      /// </summary>
      Decimal32 = 0x74,

      /// <summary>
      /// Indicates that the next eight bytes represents an AMQP Decimal64 value.
      /// </summary>
      Decimal64 = 0x84,

      /// <summary>
      /// Indicates that the next sixteen bytes represents an AMQP Decimal128 value.
      /// </summary>
      Decimal128 = 0x94,

      /// <summary>
      /// Indicates that the next two bytes represents an character value.
      /// </summary>
      Char = 0x73,

      /// <summary>
      /// Indicates that the next four bytes represents an AMQP Timestamp value.
      /// </summary>
      Timestamp = 0x83,

      /// <summary>
      /// Indicates that the next sixteen bytes represents an UUID value.
      /// </summary>
      Uuid = 0x98,

      /// <summary>
      /// Indicates that a small binary encoding follows and the next byte will indicate
      /// the number of trailing bytes that comprise the binary value.
      /// </summary>
      VBin8 = 0xa0,

      /// <summary>
      /// Indicates that a binary encoding follows and the next four bytes compose an integer
      /// whose value is the number of trailing bytes that comprise the binary value.
      /// </summary>
      VBin32 = 0xb0,

      /// <summary>
      /// Indicates that a small UTF-8 string encoding follows and the next byte will indicate
      /// the number of trailing bytes that comprise the encoded string value.
      /// </summary>
      Str8 = 0xa1,

      /// <summary>
      /// Indicates that a UTF-8 string encoding follows and the next four bytes compose an integer
      /// whose value is the number of trailing bytes that comprise the encoded string value.
      /// </summary>
      Str32 = 0xb1,

      /// <summary>
      /// Indicates that a small AMQP Symbol encoding follows and the next byte will indicate
      /// the number of trailing bytes that comprise the Symbol value (in ASCII bytes).
      /// </summary>
      Sym8 = 0xa3,

      /// <summary>
      /// Indicates that an AMQP Symbol encoding follows and the next four bytes compose an integer
      /// whose value is the number of trailing bytes that comprise the Symbol value (in ASCII bytes).
      /// </summary>
      Sym32 = 0xb3,

      /// <summary>
      /// Indicates that a zero sized List type was encoded (no additional byte follow for this encoding)
      /// </summary>
      List0 = 0x45,

      /// <summary>
      /// Indicates that a small List encoding follows and the next two bytes will indicate the total list
      /// encoding size and the number of entries in the list respectively. Each element of the list is
      /// then encoded using their noraml AMQP encoded form.
      /// </summary>
      List8 = 0xc0,

      /// <summary>
      /// Indicates that a List encoding follows and the next eight bytes will indicate the total list
      /// encoding size and the number of entries in the list respectively as integer values. Each element
      /// of the list is then encoded using their noraml AMQP encoded form.
      /// </summary>
      List32 = 0xd0,

      /// <summary>
      /// Indicates that a small Map encoding follows and the next two bytes will indicate the total Map
      /// encoding size and the number of entries in the Map respectively (read as integer vlaues). .
      /// Each entry of the Map is then encoded using their noraml AMQP encoded form. The total Map size
      /// is one half the number of elements indicated by the number of entries value read.
      /// </summary>
      Map8 = 0xc1,

      /// <summary>
      /// Indicates that a Map encoding follows and the next eight bytes will indicate the total Map
      /// encoding size and the number of entries in the Map respectively (read as integer vlaues).
      /// Each entry of the Map is then encoded using their noraml AMQP encoded form. The total Map
      /// size is one half the number of elements indicated by the number of entries value read.
      /// </summary>
      Map32 = 0xd1,

      /// <summary>
      /// Indicates that a small array encoding follows.  The array is encoded using the next three
      /// bytes to represent the total encoded array size, the number of entries in the array and the
      /// encoding code of the type that comprieses the array in that order.
      /// </summary>
      Array8 = 0xe0,

      /// <summary>
      /// Indicates that a array encoding follows.  The array is encoded using the next nine
      /// bytes to represent the total encoded array size, the number of entries in the array and the
      /// encoding code of the type that comprieses the array in that order.
      /// </summary>
      Array32 = 0xf0

   }
}