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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Comparators
{
   /// <summary>
   /// Compares two IEnumerable types for equality using the equality
   /// comparator to evaluate each pair of values in the enumeration.
   /// </summary>
   public sealed class EnumerableCompartor : ILinkedCompartor
   {
      private readonly PeerEqualityComparator comparators;

      public EnumerableCompartor(PeerEqualityComparator comparators)
      {
         this.comparators = comparators;
      }

      public bool? Equals(object lhs, object rhs, IDescription mismatchDescription)
      {
         // Must both be enumerable to qualify for testing otherwise the top level
         // comparator needs to handle this difference.
         if (lhs is not IEnumerable lhsEnumerable || rhs is not IEnumerable rhsEnumerable)
         {
            return null;
         }

         IEnumerator expectedEnum = lhsEnumerable.GetEnumerator();
         using (expectedEnum as IDisposable)
         {
            IEnumerator actualEnum = rhsEnumerable.GetEnumerator();
            using (actualEnum as IDisposable)
            {
               for (int count = 0; ; count++)
               {
                  bool expectedHasData = expectedEnum.MoveNext();
                  bool actualHasData = actualEnum.MoveNext();

                  if (!expectedHasData && !actualHasData)
                  {
                     return true;
                  }

                  if (expectedHasData != actualHasData ||
                      !comparators.AreEqual(expectedEnum.Current, actualEnum.Current, mismatchDescription))
                  {
                     mismatchDescription.AppendText("Enumerable comparison failed at position [")
                                        .AppendValue(count).AppendText("]");
                     if (expectedHasData)
                     {
                        if (actualHasData)
                        {
                           mismatchDescription.AppendText("Expected ").AppendValue(expectedEnum.Current)
                                              .AppendText(" but found ").AppendValue(actualEnum.Current);
                        }
                        else
                        {
                           mismatchDescription.AppendText("Expected ").AppendValue(expectedEnum.Current)
                                              .AppendText(" but reached end of actual enumerable");
                        }
                     }
                     else if (actualHasData)
                     {
                        mismatchDescription.AppendText("Expected no more entries")
                                           .AppendText(" but found ").AppendValue(actualEnum.Current);
                     }

                     return false;
                  }
               }
            }
         }
      }
   }
}