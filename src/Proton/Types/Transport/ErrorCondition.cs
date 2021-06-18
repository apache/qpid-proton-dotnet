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
using System.Collections.ObjectModel;

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class ErrorCondition : ICloneable
   {
      public static readonly ulong DESCRIPTOR_CODE = 0x000000000000001dUL;
      public static readonly Symbol DESCRIPTOR_SYMBOL = Symbol.Lookup("amqp:error:list");

      public ErrorCondition(Symbol condition) : this(condition, null, null) { }

      public ErrorCondition(Symbol condition, string description) : this(condition, description, null) { }

      public ErrorCondition(Symbol condition, string description, IDictionary<Symbol, object> info)
      {
         this.Condition = condition;
         this.Description = description;
         this.Info = info != null ? new ReadOnlyDictionary<Symbol, object>(info) : null;
      }

      public ErrorCondition(ErrorCondition other)
      {
         this.Condition = other.Condition;
         this.Description = other.Description;
         this.Info = other.Info;
      }

      public Symbol Condition { get; }

      public string Description { get; }

      public IReadOnlyDictionary<Symbol, object> Info { get; }

      public object Clone()
      {
         return new ErrorCondition(this);
      }

      public new string ToString()
      {
         return "Error{" +
                "condition=" + Condition +
                ", description='" + Description + '\'' +
                ", info=" + Info +
                '}';
      }
   }
}