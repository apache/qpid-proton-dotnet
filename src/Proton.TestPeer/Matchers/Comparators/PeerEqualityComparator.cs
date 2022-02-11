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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Comparators
{
   /// <summary>
   /// Used to do a deep equality comparison between .NET types that might not
   /// otherwise allow for equals comparison such as different Map types etc.
   /// </summary>
   public sealed class PeerEqualityComparator
   {
      /// <summary>
      /// The builtin list of comparators that will be tried
      /// </summary>
      private readonly IList<ILinkedCompartor> comparators = new List<ILinkedCompartor>();

      public PeerEqualityComparator()
      {
         EnumerableCompartor singletonEnumComparator = new EnumerableCompartor(this);

         comparators.Add(new ArrayCompartor(this, singletonEnumComparator));
         comparators.Add(new DictionaryCompartor(this));
         comparators.Add(new DictionaryEntryCompartor(this));
         comparators.Add(new KeyValuePairCompartor(this));
         comparators.Add(singletonEnumComparator);
         comparators.Add(new CharacterCompartor(this));
         comparators.Add(new SymbolCompartor(this));
         comparators.Add(new StringCompartor(this));
         comparators.Add(new NumberCompartor(this));
      }

      public bool AreEqual(object lhs, object rhs)
      {
         return AreEqual(lhs, rhs, new StringDescription());
      }

      internal bool AreEqual(object lhs, object rhs, IDescription mismatchDescription)
      {
         if (lhs == null && rhs == null)
         {
            return true;
         }

         if (lhs == null || rhs == null)
         {
            return false;
         }

         if (object.ReferenceEquals(lhs, rhs))
         {
            return true;
         }

         foreach (ILinkedCompartor comparer in comparators)
         {
            bool? result = comparer.Equals(lhs, rhs, mismatchDescription);
            if (result.HasValue)
            {
               return result.Value;
            }
         }

         // When all else fails try the built in equality check, maybe it will
         // be implemented for these types.
         return lhs.Equals(rhs);
      }
   }
}