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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   public enum ErrorConditionField
   {
      Condition,
      Description,
      Info
   }

   public sealed class ErrorCondition : ListDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:error:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x000000000000001dUL;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public ErrorCondition() : base(Enum.GetNames(typeof(ErrorConditionField)).Length)
      {
      }

      public ErrorCondition(object described) : base(Enum.GetNames(typeof(ErrorConditionField)).Length, (IList)described)
      {
      }

      public ErrorCondition(IList described) : base(Enum.GetNames(typeof(ErrorConditionField)).Length, described)
      {
      }

      public ErrorCondition(Symbol condition, String description) : base(Enum.GetNames(typeof(ErrorConditionField)).Length)
      {
         Condition = condition;
         Description = description;
      }

      public ErrorCondition(String condition, String description) : base(Enum.GetNames(typeof(ErrorConditionField)).Length)
      {
         Condition = new Symbol(condition);
         Description = description;
      }

      public ErrorCondition(Symbol condition, String description, IDictionary<Symbol, object> info) : base(Enum.GetNames(typeof(ErrorConditionField)).Length)
      {
         Condition = condition;
         Description = description;
         Info = (IDictionary)info;
      }

      public Symbol Condition
      {
         get => (Symbol)List[((int)ErrorConditionField.Condition)];
         set => List[((int)ErrorConditionField.Condition)] = value;
      }

      public string Description
      {
         get => (string)List[((int)ErrorConditionField.Description)];
         set => List[((int)ErrorConditionField.Description)] = value;
      }

      public IDictionary Info
      {
         get => (IDictionary)List[((int)ErrorConditionField.Info)];
         set => List[((int)ErrorConditionField.Info)] = value;
      }

      public override int GetHashCode()
      {
         return DESCRIPTOR_SYMBOL.GetHashCode();
      }

      public override bool Equals(object obj)
      {
         if (obj == this)
         {
            return true;
         }

         if (obj is not IDescribedType)
         {
            return false;
         }

         IDescribedType d = (IDescribedType)obj;
         if (!(DESCRIPTOR_CODE.Equals(d.Descriptor) || DESCRIPTOR_SYMBOL.Equals(d.Descriptor)))
         {
            return false;
         }

         if (d is ErrorCondition error)
         {
            bool equal = true;

            if (!EqualityComparer<Symbol>.Default.Equals(Condition, error.Condition))
            {
               equal = false;
            }

            if (equal && Description != error.Description)
            {
               equal = false;
            }

            if (equal && Info != null && error.Info != null && Info.Count == error.Info.Count)
            {
               foreach (KeyValuePair<Symbol, object> pair in Info)
               {
                  object value = error.Info[pair.Value];
                  if (!EqualityComparer<object>.Default.Equals(value, pair.Value))
                  {
                     equal = false;
                     break;
                  }
               }
            }
            else
            {
               equal = equal && (Info == null && error.Info == null);
            }

            return equal;
         }
         else
         {
            object described = Described;
            object described2 = d.Described;
            if (described == null)
            {
               return described2 == null;
            }
            else
            {
               return described.Equals(described2);
            }
         }
      }

      public override string ToString()
      {
         return "Error{" +
                "condition=" + Condition +
                ", description='" + Description + '\'' +
                ", info=" + Info +
                '}';
      }
   }
}