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
using Apache.Qpid.Proton.Types.Security;

namespace Apache.Qpid.Proton.Codec.Encoders.Security
{
   public sealed class SaslInitTypeEncoder : AbstractDescribedListTypeEncoder<SaslInit>
   {
      public override Symbol DescriptorSymbol => SaslInit.DescriptorSymbol;

      public override ulong DescriptorCode => SaslInit.DescriptorCode;

      protected override int GetElementCount(SaslInit init)
      {
         if (init.Hostname != null)
         {
            return 3;
         }
         else if (init.InitialResponse != null)
         {
            return 2;
         }
         else
         {
            return 1;
         }
      }

      protected override int GetMinElementCount()
      {
         return 1;
      }

      protected override void WriteElement(SaslInit init, int index, IProtonBuffer buffer, IEncoderState state)
      {
         switch (index)
         {
            case 0:
               state.Encoder.WriteSymbol(buffer, state, init.Mechanism);
               break;
            case 1:
               state.Encoder.WriteBinary(buffer, state, init.InitialResponse);
               break;
            case 2:
               state.Encoder.WriteString(buffer, state, init.Hostname);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown SaslInit value index: " + index);
         }
      }
   }
}