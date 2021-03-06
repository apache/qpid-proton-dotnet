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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Types.Transactions
{
   public sealed class Coordinator : ITerminus
   {
      public static readonly ulong DescriptorCode = 0x0000000000000030UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:coordinator:list");

      public Coordinator() : base() { }

      public Coordinator(Coordinator other) : this()
      {
         Capabilities = (Symbol[])other.Capabilities?.Clone();
      }

      public Symbol[] Capabilities { get; set; }

      public object Clone()
      {
         return new Coordinator(this);
      }

      public Coordinator Copy()
      {
         return new Coordinator(this);
      }

      ITerminus ITerminus.Copy()
      {
         return new Coordinator(this);
      }

      public override bool Equals(object obj)
      {
         return obj is Coordinator coordinator &&
                EqualityComparer<Symbol[]>.Default.Equals(Capabilities, coordinator.Capabilities);
      }

      public override int GetHashCode()
      {
         return HashCode.Combine(Capabilities);
      }

      public override string ToString()
      {
         return "Coordinator{" + "capabilities=" + Capabilities + '}';
      }
   }
}