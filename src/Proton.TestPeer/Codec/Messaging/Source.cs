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
using System.Collections;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Messaging
{
   public enum SourceField : uint
   {
      Address,
      Durable,
      ExpiryPolicy,
      Timeout,
      Dynamic,
      DynamicNodeProperties,
      DistributionMode,
      Filter,
      DefaultOutcome,
      Outcomes,
      Capabilities
   }

   public sealed class Source : ListDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new("amqp:source:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000028UL;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Source() : base(Enum.GetNames(typeof(SourceField)).Length)
      {
      }

      public Source(object described) : base(Enum.GetNames(typeof(SourceField)).Length, (IList)described)
      {
      }

      public Source(Source value) : base(Enum.GetNames(typeof(SourceField)).Length, value)
      {
      }

      public Source(IList described) : base(Enum.GetNames(typeof(SourceField)).Length, described)
      {
      }

      public string Address
      {
         get => (string)List[((int)SourceField.Address)];
         set => List[((int)SourceField.Address)] = value;
      }

      public uint? Durable
      {
         get => (uint?)List[((int)SourceField.Durable)];
         set => List[((int)SourceField.Durable)] = value;
      }

      public Symbol ExpiryPolicy
      {
         get => (Symbol)List[((int)SourceField.ExpiryPolicy)];
         set => List[((int)SourceField.ExpiryPolicy)] = value;
      }

      public uint? Timeout
      {
         get => (uint?)List[((int)SourceField.Timeout)];
         set => List[((int)SourceField.Timeout)] = value;
      }

      public bool? Dynamic
      {
         get => (bool?)List[((int)SourceField.Dynamic)];
         set => List[((int)SourceField.Dynamic)] = value;
      }

      public IDictionary DynamicNodeProperties
      {
         get => (IDictionary)List[((int)SourceField.DynamicNodeProperties)];
         set => List[((int)SourceField.DynamicNodeProperties)] = value;
      }

      public Symbol DistributionMode
      {
         get => (Symbol)List[((int)SourceField.DistributionMode)];
         set => List[((int)SourceField.DistributionMode)] = value;
      }

      public IDictionary Filter
      {
         get => (IDictionary)List[((int)SourceField.Filter)];
         set => List[((int)SourceField.Filter)] = value;
      }

      public IDescribedType DefaultOutcome
      {
         get => (IDescribedType)List[((int)SourceField.DefaultOutcome)];
         set => List[((int)SourceField.DefaultOutcome)] = value;
      }

      public Symbol[] Outcomes
      {
         get => (Symbol[])List[((int)SourceField.Outcomes)];
         set => List[((int)SourceField.Outcomes)] = value;
      }

      public Symbol[] Capabilities
      {
         get => (Symbol[])List[((int)SourceField.Capabilities)];
         set => List[((int)SourceField.Capabilities)] = value;
      }

      public override string ToString()
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
   }
}