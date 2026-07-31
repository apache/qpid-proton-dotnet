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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Decoders.Messaging
{
   public sealed class SourceTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinSourceListEntries = 0;
      private static readonly int MaxSourceListEntries = 11;

      public override Symbol DescriptorSymbol => Source.DescriptorSymbol;

      public override ulong DescriptorCode => Source.DescriptorCode;

      public override Type DecodesType => typeof(Source);

      protected override int MinListElements => MinSourceListEntries;

      protected override int MaxListElements => MaxSourceListEntries;

      protected override Source ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Source result = new();

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  result.Address = state.Decoder.ReadString(buffer, state);
                  break;
               case 1:
                  uint durability = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  result.Durable = TerminusDurabilityExtension.Lookup(durability);
                  break;
               case 2:
                  Symbol expiryPolicy = state.Decoder.ReadSymbol(buffer, state);
                  result.ExpiryPolicy = TerminusExpiryPolicyExtension.Lookup(expiryPolicy);
                  break;
               case 3:
                  result.Timeout = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 4:
                  result.Dynamic = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 5:
                  result.DynamicNodeProperties = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
               case 6:
                  result.DistributionMode = state.Decoder.ReadSymbol(buffer, state);
                  break;
               case 7:
                  result.Filter = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
               case 8:
                  result.DefaultOutcome = state.Decoder.ReadObject<IOutcome>(buffer, state);
                  break;
               case 9:
                  result.Outcomes = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 10:
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
            }
         }

         return result;
      }

      protected override Source ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Source result = new();

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  result.Address = state.Decoder.ReadString(stream, state);
                  break;
               case 1:
                  uint durability = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  result.Durable = TerminusDurabilityExtension.Lookup(durability);
                  break;
               case 2:
                  Symbol expiryPolicy = state.Decoder.ReadSymbol(stream, state);
                  result.ExpiryPolicy = TerminusExpiryPolicyExtension.Lookup(expiryPolicy);
                  break;
               case 3:
                  result.Timeout = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 4:
                  result.Dynamic = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 5:
                  result.DynamicNodeProperties = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
               case 6:
                  result.DistributionMode = state.Decoder.ReadSymbol(stream, state);
                  break;
               case 7:
                  result.Filter = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
               case 8:
                  result.DefaultOutcome = state.Decoder.ReadObject<IOutcome>(stream, state);
                  break;
               case 9:
                  result.Outcomes = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 10:
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
            }
         }

         return result;
      }
   }
}