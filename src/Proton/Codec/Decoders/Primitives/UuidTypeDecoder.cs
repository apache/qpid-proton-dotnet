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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   public sealed class UuidTypeDecoder : AbstractPrimitiveTypeDecoder
   {
      private static readonly int BYTES = sizeof(long) * 2;

      public override EncodingCodes EncodingCode => EncodingCodes.Uuid;

      public override Type DecodesType() => typeof(Guid);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         byte[] guidBytes = new byte[BYTES];

         buffer.ReadBytes(guidBytes);

         return new Guid(guidBytes);
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         byte[] guidBytes = ProtonStreamReadUtils.ReadBytes(stream, BYTES);

         return new Guid(guidBytes);
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         buffer.SkipBytes(BYTES);
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         ProtonStreamReadUtils.SkipBytes(stream, BYTES);
      }

      public static Guid ReadUuid(IProtonBuffer buffer)
      {
         byte[] guidBytes = new byte[BYTES];
         buffer.ReadBytes(guidBytes);

         return new Guid(guidBytes);
      }

      public static Guid ReadUuid(Stream stream)
      {
         return new Guid(ProtonStreamReadUtils.ReadBytes(stream, BYTES));
      }
   }
}