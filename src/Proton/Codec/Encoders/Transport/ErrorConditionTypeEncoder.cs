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
   public sealed class ErrorConditionTypeEncoder : AbstractDescribedListTypeEncoder<ErrorCondition>
   {
      public override Symbol DescriptorSymbol => ErrorCondition.DescriptorSymbol;

      public override ulong DescriptorCode => ErrorCondition.DescriptorCode;

      protected override EncodingCodes GetListEncoding(ErrorCondition value)
      {
         return EncodingCodes.List32;
      }

      protected override int GetElementCount(ErrorCondition value)
      {
         if (value.Info != null)
         {
            return 3;
         }
         else if (value.Description != null)
         {
            return 2;
         }
         else
         {
            return 1;
         }
      }

      protected override void WriteElement(ErrorCondition error, int index, IProtonBuffer buffer, IEncoderState state)
      {
         switch (index)
         {
            case 0:
                state.Encoder.WriteSymbol(buffer, state, error.Condition);
                break;
            case 1:
                state.Encoder.WriteString(buffer, state, error.Description);
                break;
            case 2:
                state.Encoder.WriteMap(buffer, state, error.Info);
                break;
            default:
               throw new ArgumentOutOfRangeException("Unknown ErrorCondition value index: " + index);
         }
      }
   }
}