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
   /// Interface for all .NET type comparators that operate in a linked
   /// sequence of comparison that can recursive check nested type hierarchies.
   /// </summary>
   public interface ILinkedComparator
   {
      /// <summary>
      /// Compares the left hand side against the right hand side value and returns
      /// the result, if the comparison fails the mismatch description is filled in
      /// with details.  If the types of the expected and the actual aren't comparable
      /// by the given comparator then it should return null so the next one in the
      /// list can be tried.
      /// </summary>
      /// <param name="lhs">the left hand expected value</param>
      /// <param name="rhs">the right hand actual value</param>
      /// <param name="mismatchDescription">description of any mismatch</param>
      /// <returns>boolean result of the equality check if the types are comparable.</returns>
      bool? Equals(object lhs, object rhs, IDescription mismatchDescription);

   }
}