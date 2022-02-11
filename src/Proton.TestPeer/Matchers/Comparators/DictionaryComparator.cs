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
   /// Compares two IDictionary types for equality meaing that each
   /// has the same number of elements and that every entry in the left
   /// is present in the right and that the values mapped to each entry
   /// are equivalent.
   /// </summary>
   public sealed class DictionaryCompartor : ILinkedCompartor
   {
      private readonly PeerEqualityComparator comparators;

      public DictionaryCompartor(PeerEqualityComparator comparators)
      {
         this.comparators = comparators;
      }

      public bool? Equals(object lhs, object rhs, IDescription mismatchDescription)
      {
         if (!(lhs is IDictionary lhsDictionary) || !(rhs is IDictionary rhsDictionary))
         {
            return null;
         }

         if (lhsDictionary.Count != rhsDictionary.Count)
         {
            mismatchDescription.AppendText("Expected a dictionary with ")
                               .AppendValue(lhsDictionary.Count)
                               .AppendText(" entries, but got a dictionary with ")
                               .AppendValue(rhsDictionary.Count)
                               .AppendText(" entries instead.");
            return false;
         }

         ArrayList rhsKeys = ToArrayList(rhsDictionary.Keys);
         foreach (object key in lhsDictionary.Keys)
         {
            rhsKeys.Remove(key);
         }

         if (rhsKeys.Count != 0)
         {
            mismatchDescription.AppendText("Expected a dictionary with matching keys ")
                               .AppendValue(lhsDictionary.Keys)
                               .AppendText(", but got a dictionary with ")
                               .AppendValue(rhsKeys.Count)
                               .AppendText("different entries instead, mismatched keys: ")
                               .AppendValue(rhsKeys);
            return false;
         }

         foreach (object key in lhsDictionary.Keys)
         {
            if (!comparators.AreEqual(lhsDictionary[key], rhsDictionary[key]))
            {
               mismatchDescription.AppendText("Mismatch in dictionary entry ")
                                  .AppendValue(key)
                                  .AppendText(", expected value of ")
                                  .AppendValue(lhsDictionary[key])
                                  .AppendText("but got value of ")
                                  .AppendValue(rhsDictionary[key]);
               return false;
            }
         }

         return true;
      }

      private static ArrayList ToArrayList(IEnumerable items)
      {
         if (items is ICollection ic)
         {
            return new ArrayList(ic);
         }

         var list = new ArrayList();
         foreach (object o in items)
         {
            list.Add(o);
         }

         return list;
      }
   }
}