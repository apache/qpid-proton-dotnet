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
using Apache.Qpid.Proton.Test.Driver.Matchers.Core;

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Collection of static matcher factory methods useful for writing tests.
   /// </summary>
   public static class Matches
   {
      /// <summary>
      /// Creates a matcher instance that matches only if all of the supplied matchers
      /// match againt the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">The list of matchers that must all match</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AllOf(IEnumerable<IMatcher> matchers)
      {
         return AllOfMatcher.AllOf(matchers);
      }

      /// <summary>
      /// Creates a matcher instance that matches only if all of the supplied matchers
      /// match againt the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">The list of matchers that must all match</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AllOf(params IMatcher[] matchers)
      {
         return AllOfMatcher.AllOf(matchers);
      }

      /// <summary>
      /// Creates a matcher instance that matches if any of the supplied matchers
      /// succeeds in matching the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">A collection of matches to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AnyOf(IEnumerable<IMatcher> matchers)
      {
         return AnyOfMatcher.AnyOf(matchers);
      }

      /// <summary>
      /// Creates a matcher instance that matches if any of the supplied matchers
      /// succeeds in matching the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">A collection of matches to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AnyOf(params IMatcher[] matchers)
      {
         return AnyOfMatcher.AnyOf(matchers);
      }
   }
}