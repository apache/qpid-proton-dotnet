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
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Types.Security
{
   public class SaslInit : ISaslPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000041UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:sasl-init:list");

      private Symbol mechanism;
      private IProtonBuffer initialResponse;
      private string hostname;

      public SaslInit() { }

      public SaslInit(Symbol mechanism, IProtonBuffer initialResponse, string hostname)
      {
         this.mechanism = mechanism;
         this.initialResponse = initialResponse;
         this.hostname = hostname;
      }

      /// <summary>
      /// The SASL mechanism that the remote has selected to use for the authentication
      /// </summary>
      public Symbol Mechanism
      {
         get => mechanism;
         set
         {
            mechanism = value ?? throw new ArgumentNullException(nameof(mechanism), "Mechanism cannot be null");
         }
      }

      /// <summary>
      /// The initial response value for use in the authentication exchange
      /// </summary>
      public IProtonBuffer InitialResponse
      {
         get => initialResponse;
         set => initialResponse = value;
      }

      /// <summary>
      /// The hostname value for use in the authentication exchange
      /// </summary>
      public string Hostname
      {
         get => hostname;
         set => hostname = value;
      }

      public SaslPerformativeType Type => SaslPerformativeType.Init;

      public void Invoke<T>(ISaslPerformativeHandler<T> handler, T context)
      {
         handler.HandleInit(this, context);
      }

      public object Clone()
      {
         return new SaslInit(Mechanism, InitialResponse, Hostname);
      }

      public SaslInit Copy()
      {
         return new SaslInit(Mechanism, InitialResponse, Hostname);
      }

      public override string ToString()
      {
         return "SaslInit{" +
                "mechanism" + Mechanism + ", " +
                "initialResponse=" + (InitialResponse != null ? StringUtils.ToQuotedString(InitialResponse) : "<null>") + ", " +
                "hostname=" + Hostname + '}';
      }
   }
}