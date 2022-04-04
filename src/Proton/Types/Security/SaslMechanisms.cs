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

namespace Apache.Qpid.Proton.Types.Security
{
   public class SaslMechanisms : ISaslPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000040UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:sasl-mechanisms:list");

      private Symbol[] mechanisms;

      public SaslMechanisms() { }

      public SaslMechanisms(Symbol[] mechanisms) => this.mechanisms = mechanisms;

      /// <summary>
      /// The set of SASL mechanisms that the remote supports.
      /// </summary>
      public Symbol[] Mechanisms
      {
         get => mechanisms;
         set
         {
            mechanisms = value ?? throw new ArgumentNullException(nameof(mechanisms), "Server mechanisms are required and cannot be null");
         }
      }

      public SaslPerformativeType Type => SaslPerformativeType.Mechanisms;

      public void Invoke<T>(ISaslPerformativeHandler<T> handler, T context)
      {
         handler.HandleMechanisms(this, context);
      }

      public object Clone()
      {
         return new SaslMechanisms(Mechanisms);
      }

      public SaslMechanisms Copy()
      {
         return new SaslMechanisms(Mechanisms);
      }

      public override string ToString()
      {
         string[] mechanisms = null;

         if (Mechanisms != null)
         {
            mechanisms = Array.ConvertAll(Mechanisms, item => item.ToString());
         }

         return "SaslMechanisms{" +
                "mechanisms=" + (mechanisms == null ? "<null>" : string.Join(",", mechanisms)) + '}';
      }
   }
}