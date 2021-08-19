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
   public sealed class Source : ITerminus
   {
      public static readonly ulong DescriptorCode = 0x0000000000000028L;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:source:list");

      public Source() { }

      public Source(Source other)
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
         DistributionMode = other.DistributionMode;
         if (other.Filter != null)
         {
            Filter = new Dictionary<Symbol, object>(other.Filter);
         }
         DefaultOutcome = other.DefaultOutcome;
         Outcomes = (Symbol[]) other.Outcomes?.Clone();
         Capabilities = other.Capabilities;
      }

      public object Clone()
      {
         return new Source(this);
      }

      public Source Copy()
      {
         return new Source(this);
      }

      ITerminus ITerminus.Copy()
      {
         return new Source(this);
      }

      public string Address { get; set; }

      public TerminusDurability Durable { get; set; } = TerminusDurability.None;

      public TerminusExpiryPolicy ExpiryPolicy { get; set; } = TerminusExpiryPolicy.SessionEnd;

      public uint Timeout { get; set; } = 0;

      public bool Dynamic { get; set; } = false;

      public IDictionary<Symbol, object> DynamicNodeProperties { get; set; }

      public Symbol DistributionMode { get; set; }

      public IDictionary<Symbol, Object> Filter { get; set; }

      public IOutcome DefaultOutcome { get; set; }

      public Symbol[] Outcomes { get; set; }

      public Symbol[] Capabilities { get; set; }

      public new String ToString()
      {
         return "Source{" +
                "address='" + Address + '\'' +
                ", durable=" + Durable +
                ", expiryPolicy=" + ExpiryPolicy +
                ", timeout=" + Timeout +
                ", dynamic=" + Dynamic +
                ", dynamicNodeProperties=" + DynamicNodeProperties +
                ", distributionMode=" + DistributionMode +
                ", filter=" + Filter +
                ", defaultOutcome=" + DefaultOutcome +
                ", outcomes=" + Outcomes +
                ", capabilities=" + Capabilities +
                '}';
      }

      public override bool Equals(object obj)
      {
         return obj is Source source &&
                Address == source.Address &&
                Durable == source.Durable &&
                ExpiryPolicy == source.ExpiryPolicy &&
                Timeout == source.Timeout &&
                Dynamic == source.Dynamic &&
                EqualityComparer<IDictionary<Symbol, object>>.Default.Equals(DynamicNodeProperties, source.DynamicNodeProperties) &&
                EqualityComparer<Symbol>.Default.Equals(DistributionMode, source.DistributionMode) &&
                EqualityComparer<IDictionary<Symbol, object>>.Default.Equals(Filter, source.Filter) &&
                EqualityComparer<IOutcome>.Default.Equals(DefaultOutcome, source.DefaultOutcome) &&
                EqualityComparer<Symbol[]>.Default.Equals(Outcomes, source.Outcomes) &&
                EqualityComparer<Symbol[]>.Default.Equals(Capabilities, source.Capabilities);
      }

      public override int GetHashCode()
      {
         HashCode hash = new HashCode();
         hash.Add(Address);
         hash.Add(Durable);
         hash.Add(ExpiryPolicy);
         hash.Add(Timeout);
         hash.Add(Dynamic);
         hash.Add(DynamicNodeProperties);
         hash.Add(DistributionMode);
         hash.Add(Filter);
         hash.Add(DefaultOutcome);
         hash.Add(Outcomes);
         hash.Add(Capabilities);
         return hash.ToHashCode();
      }
   }
}