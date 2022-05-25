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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Comparators
{
   /// <summary>
   /// Compares two string types for equality.
   /// </summary>
   public sealed class StringComparator : ILinkedComparator
   {
      private readonly PeerEqualityComparator comparators;

      public StringComparator(PeerEqualityComparator comparators)
      {
         this.comparators = comparators;
      }

      internal PeerEqualityComparator Comparators => comparators;

      public bool? Equals(object lhs, object rhs, IDescription mismatchDescription)
      {
         if (lhs is not string lhsString || rhs is not string rhsString)
         {
            return null;
         }

         if (!lhsString.Equals(rhsString))
         {
            mismatchDescription.AppendText("Expected a string value of ")
                               .AppendValue(lhsString)
                               .AppendText(" but found a string value of ")
                               .AppendValue(rhsString);
            return false;
         }

         return true;
      }
   }
}