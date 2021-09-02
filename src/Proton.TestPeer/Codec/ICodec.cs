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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec
{
   public interface ICodec
   {
      void Free();

      void Clear();

      uint Count { get; }

      void Rewind();

      DataType Next { get; }

      DataType Prev { get; }

      bool Enter();

      bool Exit();

      DataType DataType { get; }

      long EncodedSize { get; }

      long Encode(Stream stream) => Encode(new BinaryWriter(stream));

      long Encode(BinaryWriter writer);

      long Decode(Stream stream) => Decode(new BinaryReader(stream));

      long Decode(BinaryReader reader);

      void PutList();

      void PutMap();

      void PutArray(bool described, DataType type);

      void PutDescribed();

      void PutNull();

      void PutBoolean(bool b);

      void PutUnsignedByte(byte ub);

      void PutByte(sbyte b);

      void PutUnsignedShort(ushort us);

      void PutShort(short s);

      void PutUnsignedInteger(uint ui);

      void PutInt(int i);

      void PutChar(char c);

      void PutUnsignedLong(ulong ul);

      void PutLong(long l);

      void PutTimestamp(DateTime t);

      void PutFloat(float f);

      void PutDouble(double d);

      void PutDecimal32(Decimal32 d);

      void PutDecimal64(Decimal64 d);

      void PutDecimal128(Decimal128 d);

      void PutUUID(Guid u);

      void PutBinary(Span<byte> bytes);

      void PutBinary(byte[] bytes);

      void PutString(string str);

      void PutSymbol(Symbol symbol);

      void PutObject(Object o);

      void PutPrimitiveMap(IDictionary map);

      void PutPrimitiveList(IList list);

      void PutDescribedType(IDescribedType dt);

      uint GetList();

      uint GetMap();

      uint GetArray();

      bool IsArrayDescribed { get; }

      DataType GetArrayType();

      bool IsDescribed { get; }

      bool IsNull { get; }

      bool GetBoolean();

      byte GetUnsignedByte();

      sbyte GetByte();

      ushort GetUnsignedShort();

      short GetShort();

      uint GetUnsignedInteger();

      int GetInt();

      int GetChar();

      ulong GetUnsignedLong();

      long GetLong();

      DateTime GetTimestamp();

      float GetFloat();

      double GetDouble();

      Decimal32 GetDecimal32();

      Decimal64 GetDecimal64();

      Decimal128 GetDecimal128();

      Guid GetUUID();

      Binary GetBinary();

      String GetString();

      Symbol GetSymbol();

      Object GetObject();

      IDictionary GetPrimitiveMap();

      IList GetPrimitiveList();

      Array GetPrimitiveArray();

      IDescribedType GetDescribedType();

      string Format();

   }
}