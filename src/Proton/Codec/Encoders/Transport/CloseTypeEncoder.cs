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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Encoders.Transport
{
   public sealed class CloseTypeEncoder : AbstractDescribedListTypeEncoder<Close>
   {
      public override Symbol DescriptorSymbol => Close.DescriptorSymbol;

      public override ulong DescriptorCode => Close.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Close value)
      {
        return value.Error == null ? EncodingCodes.List8 : EncodingCodes.List32;
      }

      protected override int GetElementCount(Close value)
      {
        return value.Error == null ? 0 : 1;
      }

      protected override void WriteElement(Close close, int index, IProtonBuffer buffer, IEncoderState state)
      {
         switch (index)
         {
            case 0:
                state.Encoder.WriteObject(buffer, state, close.Error);
                break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Close value index: " + index);
         }
      }
   }
}