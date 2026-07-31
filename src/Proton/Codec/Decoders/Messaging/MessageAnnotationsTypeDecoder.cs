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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Decoders.Messaging
{
   public sealed class MessageAnnotationsTypeDecoder : AbstractDescribedMapTypeDecoder<Symbol>
   {
      public override Symbol DescriptorSymbol => MessageAnnotations.DescriptorSymbol;

      public override ulong DescriptorCode => MessageAnnotations.DescriptorCode;

      public override Type DecodesType => typeof(MessageAnnotations);

      protected override MessageAnnotations CreateDescribed(IDictionary<Symbol, object> map)
      {
         return new MessageAnnotations(map);
      }

      protected override Symbol ReadKey(IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         return decoder.ReadSymbol(buffer, state);
      }

      protected override Symbol ReadKey(Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         return decoder.ReadSymbol(stream, state);
      }
   }
}