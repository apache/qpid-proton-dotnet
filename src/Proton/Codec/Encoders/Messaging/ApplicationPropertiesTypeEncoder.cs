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

using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Encoders.Messaging
{
   public sealed class ApplicationPropertiesTypeEncoder : AbstractDescribedMapTypeEncoder<string, object, ApplicationProperties>
   {
      public override Symbol DescriptorSymbol => ApplicationProperties.DescriptorSymbol;

      public override ulong DescriptorCode => ApplicationProperties.DescriptorCode;

      protected override int GetMapEntries(ApplicationProperties value)
      {
         return value?.Value?.Count ?? 0;
      }

      protected override bool HasMap(ApplicationProperties value)
      {
         return value?.Value != null;
      }

      protected override void WriteMapEntries(IProtonBuffer buffer, IEncoderState state, ApplicationProperties value)
      {
         foreach (KeyValuePair<string, object> entry in value.Value)
         {
            state.Encoder.WriteString(buffer, state, entry.Key);
            state.Encoder.WriteObject(buffer, state, entry.Value);
         }
      }
   }
}