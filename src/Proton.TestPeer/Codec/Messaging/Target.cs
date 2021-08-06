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
   public enum TargetField
   {
      Address,
      Durable,
      ExpiryPolicy,
      Timeout,
      Dynamic,
      DynamicNodeProperties,
      Capabilities
   }

   public sealed class Target : ListDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:target:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000029UL;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Target() : base(Enum.GetNames(typeof(TargetField)).Length)
      {
      }

      public Target(object described) : base(Enum.GetNames(typeof(TargetField)).Length, (IList)described)
      {
      }

      public Target(IList described) : base(Enum.GetNames(typeof(TargetField)).Length, described)
      {
      }

      public string Address
      {
         get => (string)List[((int)TargetField.Address)];
         set => List[((int)TargetField.Address)] = value;
      }

      public uint? Durable
      {
         get => (uint?)List[((int)TargetField.Durable)];
         set => List[((int)TargetField.Durable)] = value;
      }

      public Symbol ExpiryPolicy
      {
         get => (Symbol)List[((int)TargetField.ExpiryPolicy)];
         set => List[((int)TargetField.ExpiryPolicy)] = value;
      }

      public uint? Timeout
      {
         get => (uint?)List[((int)TargetField.Timeout)];
         set => List[((int)TargetField.Timeout)] = value;
      }

      public bool? Dynamic
      {
         get => (bool?)List[((int)TargetField.Dynamic)];
         set => List[((int)TargetField.Dynamic)] = value;
      }

      public IDictionary DynamicNodeProperties
      {
         get => (IDictionary)List[((int)TargetField.DynamicNodeProperties)];
         set => List[((int)TargetField.DynamicNodeProperties)] = value;
      }

      public Symbol[] Capabilities
      {
         get => (Symbol[])List[((int)TargetField.Capabilities)];
         set => List[((int)TargetField.Capabilities)] = value;
      }

      public override string ToString()
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
   }
}
