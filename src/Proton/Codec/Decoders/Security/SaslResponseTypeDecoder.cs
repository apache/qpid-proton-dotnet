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
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Security;

namespace Apache.Qpid.Proton.Codec.Decoders.Security
{
   public sealed class SaslResponseTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int RequiredListEntries = 1;

      public override Symbol DescriptorSymbol => SaslResponse.DescriptorSymbol;

      public override ulong DescriptorCode => SaslResponse.DescriptorCode;

      public override Type DecodesType => typeof(SaslResponse);

      protected override int MinListElements => RequiredListEntries;

      protected override int MaxListElements => RequiredListEntries;

      protected override SaslResponse ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         SaslResponse result = new();

         result.Response = state.Decoder.ReadBinary(buffer, state);

         return result;
      }

      protected override SaslResponse ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         SaslResponse result = new();

         result.Response = state.Decoder.ReadBinary(stream, state);

         return result;
      }
   }
}