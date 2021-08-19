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
   /// <summary>
   /// A composite buffer contains zero, one or more proton buffer instances
   /// chained together to behave as if it were one single contiguous buffer
   /// which can be read or written to.
   /// </summary>
   public sealed class ProtonCompositeBuffer : IProtonBuffer
   {
      public long Capacity => throw new NotImplementedException();

      public bool IsReadable => throw new NotImplementedException();

      public long ReadableBytes => throw new NotImplementedException();

      public bool IsWritable => throw new NotImplementedException();

      public long WritableBytes => throw new NotImplementedException();

      public long ReadOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public long WriteOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public uint ComponentCount => throw new NotImplementedException();

      public uint ReadableComponentCount => throw new NotImplementedException();

      public uint WritableComponentCount => throw new NotImplementedException();

      public IProtonBuffer Fill(byte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Append(IProtonBuffer buffer)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Compact()
      {
         throw new NotImplementedException();
      }

      public int CompareTo(object obj)
      {
         throw new NotImplementedException();
      }

      public int CompareTo(IProtonBuffer other)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Copy()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Copy(long index, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(long srcPos, byte[] dest, long destPos, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(long srcPos, IProtonBuffer dest, long destPos, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer EnsureWritable(long amount)
      {
         throw new NotImplementedException();
      }

      public bool Equals(IProtonBuffer other)
      {
         throw new NotImplementedException();
      }

      public int ForEachReadableComponent(in int index, in Func<int, IReadableComponent, bool> processor)
      {
         throw new NotImplementedException();
      }

      public int ForEachWritableComponent(in int index, in Func<int, IWritableComponent, bool> processor)
      {
         throw new NotImplementedException();
      }

      public bool GetBoolean(long index)
      {
         throw new NotImplementedException();
      }

      public sbyte GetByte(long index)
      {
         throw new NotImplementedException();
      }

      public char GetChar(long index)
      {
         throw new NotImplementedException();
      }

      public double GetDouble(long index)
      {
         throw new NotImplementedException();
      }

      public float GetFloat(long index)
      {
         throw new NotImplementedException();
      }

      public int GetInt(long index)
      {
         throw new NotImplementedException();
      }

      public long GetLong(long index)
      {
         throw new NotImplementedException();
      }

      public short GetShort(long index)
      {
         throw new NotImplementedException();
      }

      public byte GetUnsignedByte(long index)
      {
         throw new NotImplementedException();
      }

      public uint GetUnsignedInt(long index)
      {
         throw new NotImplementedException();
      }

      public ulong GetUnsignedLong(long index)
      {
         throw new NotImplementedException();
      }

      public ushort GetUnsignedShort(long index)
      {
         throw new NotImplementedException();
      }

      public bool ReadBoolean()
      {
         throw new NotImplementedException();
      }

      public sbyte ReadByte()
      {
         throw new NotImplementedException();
      }

      public char ReadChar()
      {
         throw new NotImplementedException();
      }

      public double ReadDouble()
      {
         throw new NotImplementedException();
      }

      public float ReadFloat()
      {
         throw new NotImplementedException();
      }

      public int ReadInt()
      {
         throw new NotImplementedException();
      }

      public long ReadLong()
      {
         throw new NotImplementedException();
      }

      public short ReadShort()
      {
         throw new NotImplementedException();
      }

      public byte ReadUnsignedByte()
      {
         throw new NotImplementedException();
      }

      public uint ReadUnsignedInt()
      {
         throw new NotImplementedException();
      }

      public ulong ReadUnsignedLong()
      {
         throw new NotImplementedException();
      }

      public ushort ReadUnsignedShort()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Reset()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetBoolean(long index, bool value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetByte(long index, sbyte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetChar(long index, char value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetDouble(long index, double value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetFloat(long index, float value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetInt(long index, int value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetLong(long index, long value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetShort(long index, short value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedByte(long index, byte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedInt(long index, uint value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedLong(long index, ulong value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedShort(long index, ushort value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SkipBytes(long amount)
      {
         throw new NotImplementedException();
      }

      public string ToString(Encoding encoding)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteBoolean(bool value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteByte(sbyte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteBytes(byte[] source)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteBytes(byte[] source, long offset, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteBytes(IProtonBuffer source)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteDouble(double value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteFloat(float value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteInt(int value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteLong(long value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteShort(short value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedByte(byte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedInt(uint value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedLong(ulong value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedShort(ushort value)
      {
         throw new NotImplementedException();
      }
   }
}