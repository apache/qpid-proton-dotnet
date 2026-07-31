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
   public sealed class DeleteOnCloseTypeDecoder : AbstractDescribedListTypeDecoder
   {
      public override Symbol DescriptorSymbol => DeleteOnClose.DescriptorSymbol;

      public override ulong DescriptorCode => DeleteOnClose.DescriptorCode;

      public override Type DecodesType => typeof(DeleteOnClose);

      protected override int MinListElements => 0;

      protected override int MaxListElements => 0;

      protected sealed override DeleteOnClose ReadSingle(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder decoder)
      {
         decoder.SkipValue(buffer, state);

         return DeleteOnClose.Instance;
      }

      protected sealed override DeleteOnClose ReadSingle(Stream stream, IStreamDecoderState state, IListTypeDecoder decoder)
      {
         decoder.SkipValue(stream, state);

         return DeleteOnClose.Instance;
      }

      protected override object ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         throw new NotImplementedException("Should not be called for this AMQP type");
      }

      protected override object ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         throw new NotImplementedException("Should not be called for this AMQP type");
      }
   }
}