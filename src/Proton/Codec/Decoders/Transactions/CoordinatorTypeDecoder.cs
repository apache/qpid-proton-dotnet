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
   public sealed class CoordinatorTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinRequiredListEntries = 0;
      private static readonly int MaxRequiredListEntries = 1;

      public override Symbol DescriptorSymbol => Coordinator.DescriptorSymbol;

      public override ulong DescriptorCode => Coordinator.DescriptorCode;

      public override Type DecodesType => typeof(Coordinator);

      protected override int MinListElements => MinRequiredListEntries;

      protected override int MaxListElements => MaxRequiredListEntries;

      protected override Coordinator ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Coordinator result = new();

         if (count == 1)
         {
            result.Capabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
         }

         return result;
      }

      protected override Coordinator ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Coordinator result = new();

         if (count == 1)
         {
            result.Capabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
         }

         return result;
      }
   }
}