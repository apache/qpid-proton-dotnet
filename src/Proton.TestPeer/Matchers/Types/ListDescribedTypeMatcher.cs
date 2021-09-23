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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types
{
   /// <summary>
   /// Type safe matcher for AMQP described types that describe a list of
   /// values with a well know structure.
   /// </summary>
   public abstract class ListDescribedTypeMatcher : TypeSafeMatcher<ListDescribedType>
   {
      private string mismatchTextAddition;

      private readonly int numFields;

      private readonly ulong descriptorCode;
      private readonly Symbol descriptorSymbol;

      protected readonly IDictionary<int, IMatcher> fieldMatchers = new Dictionary<int, IMatcher>();

      public ListDescribedTypeMatcher(int numFields, ulong code, Symbol symbol)
      {
         this.descriptorCode = code;
         this.descriptorSymbol = symbol;
         this.numFields = numFields;
      }

      public ListDescribedTypeMatcher AddFieldMatcher(int field, IMatcher matcher)
      {
         if (field > numFields)
         {
            throw new ArgumentOutOfRangeException("Field index supplied exceeds number of fields in type");
         }

         fieldMatchers[field] = matcher;

         return this;
      }

      /// <summary>
      /// Returns the actual Type value for the AMQP DescribedType being matched on.
      /// </summary>
      protected abstract Type DescribedTypeClassType { get; }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText(DescribedTypeClassType.Name + " which matches: ").AppendValue(fieldMatchers);
      }

      protected override void DescribeMismatchSafely(ListDescribedType item, IDescription mismatchDescription)
      {
         mismatchDescription.AppendText("\nActual form: ").AppendValue(item);

         mismatchDescription.AppendText("\nExpected descriptor: ")
                            .AppendValue(descriptorSymbol)
                            .AppendText(" / ")
                            .AppendValue(descriptorCode);

         if (mismatchTextAddition != null)
         {
            mismatchDescription.AppendText("\nAdditional info: ").AppendValue(mismatchTextAddition);
         }
      }

      protected override bool MatchesSafely(ListDescribedType item)
      {
         try
         {
            Object descriptor = item.Descriptor;
            if (!descriptorCode.Equals(descriptor) && !descriptorSymbol.Equals(descriptor))
            {
               mismatchTextAddition = "Descriptor mismatch";
               return false;
            }

            foreach(KeyValuePair<int, IMatcher> entry in fieldMatchers)
            {
                IMatcher matcher = entry.Value;
                MatcherAssert.AssertThat("Field " + entry.Key + " value should match", item[entry.Key], matcher);
            }
         }
         catch (Exception ae)
         {
            mismatchTextAddition = "AssertionFailure: " + ae.Message;
            return false;
         }

         return true;
      }
   }
}