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

using System.Collections.Generic;

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class MessageAnnotations : IBodySection<IDictionary<Symbol, object>>
   {
      public static readonly ulong DescriptorCode = 0x0000000000000072UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:message-annotations:map");

      public SectionType Type => SectionType.MessageAnnotations;

      public IDictionary<Symbol, object> Value { get; set; }

      public MessageAnnotations() : base()
      {
      }

      public MessageAnnotations(MessageAnnotations other) : this()
      {
         if (other.Value != null)
         {
            Value = new Dictionary<Symbol, object>(other.Value);
         }
      }

      public object Clone()
      {
         return new MessageAnnotations(this);
      }

      public override string ToString()
      {
         return "Footer{ " + Value + " }";
      }

      public override int GetHashCode()
      {
         const int prime = 31;
         int result = 1;
         result = prime * result + ((Value == null) ? 0 : Value.GetHashCode());
         return result;
      }

      public override bool Equals(object other)
      {
         if (other == null || !this.GetType().Equals(other.GetType()))
         {
            return false;
         }
         else
         {
            return Equals(other as MessageAnnotations);
         }
      }

      public bool Equals(MessageAnnotations other)
      {
         if (this == other)
         {
            return true;
         }
         else if (other == null)
         {
            return false;
         }
         else if (Value == null && other.Value == null)
         {
            return true;
         }
         else
         {
            return Value == null ? false : Value.Equals(other.Value);
         }
      }
   }
}