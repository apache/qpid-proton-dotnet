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
   public sealed class TargetTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinTargetListEntries = 0;
      private static readonly int MaxTargetListEntries = 7;

      public override Symbol DescriptorSymbol => Target.DescriptorSymbol;

      public override ulong DescriptorCode => Target.DescriptorCode;

      public override Type DecodesType => typeof(Target);

      protected override int MinListElements => MinTargetListEntries;

      protected override int MaxListElements => MaxTargetListEntries;

      protected override Target ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Target result = new();

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
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
            }
         }

         return result;
      }

      protected override Target ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Target result = new();

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
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
            }
         }

         return result;
      }
   }
}