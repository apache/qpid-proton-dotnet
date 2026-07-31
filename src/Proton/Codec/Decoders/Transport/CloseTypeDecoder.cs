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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Decoders.Transport
{
   public sealed class CloseTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinCloseListEntries = 0;
      private static readonly int MaxCloseListEntries = 1;

      public override Symbol DescriptorSymbol => Close.DescriptorSymbol;

      public override ulong DescriptorCode => Close.DescriptorCode;

      public override Type DecodesType => typeof(Close);

      protected override int MinListElements => MinCloseListEntries;

      protected override int MaxListElements => MaxCloseListEntries;

      protected override Close ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Close result = new();

          if (count == 1)
         {
            result.Error = state.Decoder.ReadObject<ErrorCondition>(buffer, state);
         }

         return result;
      }

      protected override Close ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Close result = new();

         if (count == 1)
         {
            result.Error = state.Decoder.ReadObject<ErrorCondition>(stream, state);
         }

         return result;
      }
   }
}
