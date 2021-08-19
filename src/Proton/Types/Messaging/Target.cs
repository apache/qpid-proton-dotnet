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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class Target : ITerminus
   {
      public static readonly ulong DescriptorCode = 0x0000000000000029UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:target:list");

      public Target() { }

      public Target(Target other)
      {
         Address = other.Address;
         Durable = other.Durable;
         ExpiryPolicy = other.ExpiryPolicy;
         Timeout = other.Timeout;
         Dynamic = other.Dynamic;
         if (other.DynamicNodeProperties != null)
         {
            DynamicNodeProperties = new Dictionary<Symbol, object>(other.DynamicNodeProperties);
         }
         Capabilities = other.Capabilities;
      }

      public object Clone()
      {
         return new Target(this);
      }

      public Target Copy()
      {
         return new Target(this);
      }

      ITerminus ITerminus.Copy()
      {
         return new Target(this);
      }

      public string Address { get; set; }

      public TerminusDurability Durable { get; set; } = TerminusDurability.None;

      public TerminusExpiryPolicy ExpiryPolicy { get; set; } = TerminusExpiryPolicy.SessionEnd;

      public uint Timeout { get; set; } = 0;

      public bool Dynamic { get; set; } = false;

      public IDictionary<Symbol, object> DynamicNodeProperties { get; set; }

      public Symbol[] Capabilities { get; set; }

      public new String ToString()
      {
         return "Target{" +
                "address='" + Address + '\'' +
                ", durable=" + Durable +
                ", expiryPolicy=" + ExpiryPolicy +
                ", timeout=" + Timeout +
                ", dynamic=" + Dynamic +
                ", dynamicNodeProperties=" + DynamicNodeProperties +
                ", capabilities=" + Capabilities +
                '}';
      }

      public override bool Equals(object obj)
      {
         return obj is Target target &&
                Address == target.Address &&
                Durable == target.Durable &&
                ExpiryPolicy == target.ExpiryPolicy &&
                Timeout == target.Timeout &&
                Dynamic == target.Dynamic &&
                EqualityComparer<IDictionary<Symbol, object>>.Default.Equals(DynamicNodeProperties, target.DynamicNodeProperties) &&
                EqualityComparer<Symbol[]>.Default.Equals(Capabilities, target.Capabilities);
      }

      public override int GetHashCode()
      {
         return HashCode.Combine(Address, Durable, ExpiryPolicy, Timeout, Dynamic, DynamicNodeProperties, Capabilities);
      }
   }
}