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
   public sealed class ProtonByteBuffer : IProtonBuffer
   {
      public int Capacity => throw new NotImplementedException();

      public int ReadOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public int WriteOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public uint ComponentCount => 1;

      public uint ReadableComponentCount => throw new NotImplementedException();

      public uint WritableComponentCount => throw new NotImplementedException();

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

      public IProtonBuffer Copy(int index, int length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(int srcPos, byte[] dest, int destPos, int length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(int srcPos, IProtonBuffer dest, int destPos, int length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer EnsureWritable(int amount)
      {
         throw new NotImplementedException();
      }

      public bool Equals(IProtonBuffer other)
      {
         throw new NotImplementedException();
      }

      public bool GetBool(int index)
      {
         throw new NotImplementedException();
      }

      public sbyte GetByte(int index)
      {
         throw new NotImplementedException();
      }

      public char GetChar(int index)
      {
         throw new NotImplementedException();
      }

      public double GetDouble(int index)
      {
         throw new NotImplementedException();
      }

      public float GetFloat(int index)
      {
         throw new NotImplementedException();
      }

      public int GetInt(int index)
      {
         throw new NotImplementedException();
      }

      public long GetLong(int index)
      {
         throw new NotImplementedException();
      }

      public short GetShort(int index)
      {
         throw new NotImplementedException();
      }

      public byte GetUnsignedByte(int index)
      {
         throw new NotImplementedException();
      }

      public uint GetUnsignedInt(int index)
      {
         throw new NotImplementedException();
      }

      public ulong GetUnsignedLong(int index)
      {
         throw new NotImplementedException();
      }

      public ushort GetUnsignedShort(int index)
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

      public IProtonBuffer SetBoolean(int index, bool value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetByte(int index, sbyte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetChar(int index, char value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetDouble(int index, double value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetFloat(int index, float value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetInt(int index, int value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetLong(int index, long value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetShort(int index, short value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedByte(int index, byte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedInt(int index, uint value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedLong(int index, ulong value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedShort(int index, ushort value)
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