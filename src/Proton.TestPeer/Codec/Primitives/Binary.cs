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
using System.Text;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Primitives
{
   public sealed class Binary
   {
      private readonly byte[] buffer;

      public Binary()
      {
         this.buffer = null;
      }

      public Binary(byte[] data) : this(data, 0, data.Length)
      {
      }

      public Binary(byte[] data, int offset, int length)
      {
         this.buffer = new byte[length];
         System.Array.ConstrainedCopy(data, offset, buffer, 0, length);
      }

      public int Length => buffer == null ? 0 : buffer.Length;

      public bool HasArray => buffer != null;

      public byte[] Array => buffer;

      public override bool Equals(object obj)
      {
         if (obj is Binary binary)
         {
            if (Length != binary.Length)
            {
               return false;
            }

            for (int i = 0; i < Length; ++i)
            {
               if (buffer[i] != binary.buffer[i])
               {
                  return false;
               }
            }

            return true;
         }

         return false;
      }

      public override int GetHashCode()
      {
         return buffer.GetHashCode();
      }

      public override String ToString()
      {
         if (buffer == null)
         {
            return "";
         }

         StringBuilder str = new StringBuilder();

         for (int i = 0; i < Length; i++)
         {
            byte c = buffer[i];

            if (c > 31 && c < 127 && c != '\\')
            {
               str.Append((char)c);
            }
            else
            {
               str.Append(c.ToString("X2"));
            }
         }

         return str.ToString();
      }
   }
}