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
   public sealed class Integer8TypeDecoder : AbstractPrimitiveTypeDecoder
   {
      public override EncodingCodes EncodingCode => EncodingCodes.SmallInt;

      public override Type DecodesType() => typeof(int);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         return (int) buffer.ReadByte();
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         return (int) ProtonStreamReadUtils.ReadByte(stream);
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         buffer.SkipBytes(sizeof(byte));
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         ProtonStreamReadUtils.SkipBytes(stream, sizeof(byte));
      }
   }
}