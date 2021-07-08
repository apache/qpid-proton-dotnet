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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Encoders.Primitives
{
   public sealed class DeleteOnNoMessagesTypeEncoder : AbstractDescribedListTypeEncoder<DeleteOnNoMessages>
   {
      private static readonly int EncodingSize = 4;

      public override Symbol DescriptorSymbol => DeleteOnNoMessages.DescriptorSymbol;

      public override ulong DescriptorCode => DeleteOnNoMessages.DescriptorCode;

      protected override EncodingCodes GetListEncoding(DeleteOnNoMessages value)
      {
         return EncodingCodes.List0;
      }

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, DeleteOnNoMessages value)
      {
         buffer.EnsureWritable(EncodingSize);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)DeleteOnClose.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.List0));
      }

      protected override int GetElementCount(DeleteOnNoMessages value)
      {
         return 0;
      }

      protected override void WriteElement(DeleteOnNoMessages source, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // Nothing to do here, this type has no body
      }
   }
}