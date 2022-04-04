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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Encoders.Primitives
{
   /// <summary>
   /// Interface for an type encoders that handle primitive types
   /// </summary>
   public sealed class ArrayTypeEncoder : IPrimitiveTypeEncoder<Array>
   {
      public Type EncodesType => typeof(Array);

      public static bool IsArrayType => true;

      public void WriteArray(IProtonBuffer buffer, IEncoderState state, Array value)
      {
         ITypeEncoder typeEncoder = FindTypeEncoder(buffer, state, value);

         // If the is an array of arrays then we need to control the encoding code
         // and size indicators and hand off writing the entries to the raw write
         // method which will follow the nested path.  If not an array then we can
         // hand off the encoding to that specific type encoder which can then make
         // a determination of how to best encode the type for compactness.
         if (typeEncoder == this)
         {
            buffer.EnsureWritable(sizeof(byte) + value.Length);

            // Write the Array Type encoding code, we don't optimize here.
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));

            long startIndex = buffer.WriteOffset;

            // Reserve space for the size and write the count of list elements.
            buffer.EnsureWritable(sizeof(long));
            buffer.WriteInt(0);
            buffer.WriteInt(value.Length);

            // Write the arrays as a raw series of arrays accounting for nested arrays
            WriteRawArray(buffer, state, value);

            // Move back and write the size
            long writeSize = buffer.WriteOffset - startIndex - sizeof(int);

            if (writeSize > Int32.MaxValue)
            {
               throw new ArgumentException("Cannot encode given array, encoded size to large: " + writeSize);
            }

            buffer.SetInt(startIndex, (int)writeSize);
         }
         else
         {
            typeEncoder.WriteArray(buffer, state, value);
         }
      }

      public void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         // Write the Array Type encoding code, we don't optimize here.
         buffer.EnsureWritable(sizeof(byte) + values.Length);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));

         for (int i = 0; i < values.Length; ++i)
         {
            long startIndex = buffer.WriteOffset;

            if (values.GetValue(i) is not Array)
            {
               throw new ArgumentException("Expected array based elements but got object of type: " + values.GetValue(i));
            }

            Array subArray = values.GetValue(i) as Array;

            ITypeEncoder typeEncoder = FindTypeEncoder(buffer, state, subArray);

            // Reserve space for the size and write the count of list elements.
            buffer.EnsureWritable(sizeof(long));
            buffer.WriteInt(0);
            buffer.WriteInt(subArray.Length);

            typeEncoder.WriteRawArray(buffer, state, subArray);

            // Move back and write the size
            long writeSize = buffer.WriteOffset - startIndex - sizeof(int);

            if (writeSize > Int32.MaxValue)
            {
               throw new ArgumentOutOfRangeException("Cannot encode given array, encoded size to large: " + writeSize);
            }

            buffer.SetInt(startIndex, (int)writeSize);
         }
      }

      public void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         if (value is not Array)
         {
            throw new ArgumentException("Expected Array type but got: " + value.GetType().Name);
         }

         WriteArray(buffer, state, value as Array);
      }

      private ITypeEncoder FindTypeEncoder(IProtonBuffer buffer, IEncoderState state, Array array)
      {
         ITypeEncoder typeEncoder;

         if (array.Length == 0)
         {
            if (array.GetType().GetElementType() == typeof(object))
            {
               throw new ArgumentException("Cannot write a zero sized untyped array.");
            }
            else
            {
               typeEncoder = state.Encoder.LookupTypeEncoder(array.GetType().GetElementType());
            }
         }
         else
         {
            if (array.GetValue(0).GetType().IsArray)
            {
               typeEncoder = this;
            }
            else
            {
               if (array.GetValue(0).GetType() == typeof(object))
               {
                  throw new ArgumentException("Cannot write a zero sized untyped array.");
               }

               typeEncoder = state.Encoder.LookupTypeEncoder(array.GetValue(0).GetType());
            }
         }

         if (typeEncoder == null)
         {
            throw new ArgumentException("Cannot encode array of unknown type.");
         }

         return typeEncoder;
      }
   }
}