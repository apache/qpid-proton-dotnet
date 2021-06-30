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
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Types.Security
{
   public class SaslResponse : ISaslPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000043UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:sasl-response:list");

      public SaslResponse() { }

      public SaslResponse(IProtonBuffer response) => Response = response;

      /// <summary>
      /// The response buffer to use for this SASL Authentication step.
      /// </summary>
      public IProtonBuffer Response { get; set; }

      public SaslPerformativeType Type => SaslPerformativeType.Response;

      public void Invoke<T>(ISaslPerformativeHandler<T> handler, T context)
      {
         handler.HandleResponse(this, context);
      }

      public object Clone()
      {
         return new SaslResponse(Response);
      }

      public override string ToString()
      {
         return "SaslResponse{" +
                "response=" + (Response != null ? StringUtils.ToQuotedString(Response) : "<null>") + '}';
      }
   }
}