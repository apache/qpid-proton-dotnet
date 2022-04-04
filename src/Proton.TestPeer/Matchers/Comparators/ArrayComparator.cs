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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Comparators
{
   /// <summary>
   /// Compares two Array types for equality using the equality
   /// comparator to evaluate each pair of values in the array.
   /// </summary>
   public sealed class ArrayCompartor : ILinkedCompartor
   {
      private readonly PeerEqualityComparator comparators;
      private readonly EnumerableCompartor enumComparator;

      public ArrayCompartor(PeerEqualityComparator comparators, EnumerableCompartor enumComparator)
      {
         this.comparators = comparators;
         this.enumComparator = enumComparator;
      }

      public bool? Equals(object lhs, object rhs, IDescription mismatchDescription)
      {
         if (lhs is not Array lhsArray || rhs is not Array rhsArray)
         {
            return null;
         }

         int rank = lhsArray.Rank;

         if (rank != rhsArray.Rank)
         {
            mismatchDescription.AppendText("Expected an array with rank of ").AppendValue(rank)
                               .AppendText(" but found an array of rank ")
                               .AppendValue(rhsArray.Rank);
            return false;
         }

         for (int r = 1; r < rank; r++)
         {
            if (lhsArray.GetLength(r) != rhsArray.GetLength(r))
            {
               return false;
            }
         }

         return enumComparator.Equals(lhsArray, rhsArray, mismatchDescription);
      }
   }
}