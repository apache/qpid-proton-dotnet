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

using System.IO;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public static class StreamExtensions
   {
      public static bool IsReadable(this Stream stream)
      {
         return stream.CanRead ? stream.Position < stream.Length : false;
      }

      public static long ReadableBytes(this Stream stream)
      {
         return stream.CanRead ? stream.Length - stream.Position : 0L;
      }
   }

   public static class BinaryReaderExtensions
   {
      public static long ReadIndex(this BinaryReader reader)
      {
         return reader.BaseStream.Position;
      }

      public static long ReadIndex(this BinaryReader reader, long index)
      {
         return reader.BaseStream.Position = index;
      }

      public static bool IsReadable(this BinaryReader reader)
      {
         return reader.BaseStream.IsReadable();
      }

      public static long ReadableBytes(this BinaryReader reader)
      {
         return reader.BaseStream.ReadableBytes();
      }
   }
}