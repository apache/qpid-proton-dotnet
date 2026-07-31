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
using Apache.Qpid.Proton.Types.Transactions;

namespace Apache.Qpid.Proton.Codec.Decoders.Transactions
{
   public sealed class DischargeTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinRequiredListEntries = 1;
      private static readonly int MaxRequiredListEntries = 2;

      public override Symbol DescriptorSymbol => Discharge.DescriptorSymbol;

      public override ulong DescriptorCode => Discharge.DescriptorCode;

      public override Type DecodesType => typeof(Discharge);

      protected override int MinListElements => MinRequiredListEntries;

      protected override int MaxListElements => MaxRequiredListEntries;

      protected override Discharge ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Discharge result = new();

         result.TxnId = state.Decoder.ReadBinary(buffer, state);

         if (count == 2)
         {
            result.Fail = state.Decoder.ReadBoolean(buffer, state) ?? false;
         }

         return result;
      }

      protected override Discharge ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Discharge result = new();

         result.TxnId = state.Decoder.ReadBinary(stream, state);

         if (count == 2)
         {
            result.Fail = state.Decoder.ReadBoolean(stream, state) ?? false;
         }

         return result;
      }
   }
}