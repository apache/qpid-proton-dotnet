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
using Apache.Qpid.Proton.Engine.Utils;

namespace Apache.Qpid.Proton.Types.Security
{
   public class SaslOutcome : SaslPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000044UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:sasl-outcome:list");

      public SaslOutcome() { }

      public SaslOutcome(SaslCode code, IProtonBuffer additionalData)
      {
         Code = code;
         AdditionalData = additionalData;
      }

      /// <summary>
      /// Additional data provided as part of the SASL authentication outcome.
      /// </summary>
      public IProtonBuffer AdditionalData { get; set; }

      /// <summary>
      /// The SASL Code that defines the authentication outcome.
      /// </summary>
      public SaslCode Code { get; set; }

      public SaslPerformativeType Type => SaslPerformativeType.Outcome;

      public void Invoke<T>(SaslPerformativeHandler<T> handler, T context)
      {
         handler.HandleOutcome(this, context);
      }

      public object Clone()
      {
         return new SaslOutcome(Code, AdditionalData);
      }

      public override string ToString()
      {
         return "SaslOutcome{" +
                "code=" + Code != null ? Code.ToString() : "<null>" + ", " +
                "additional-data=" + (AdditionalData != null ? StringUtils.ToQuotedString(AdditionalData) : "<null>") + '}';
      }
   }
}