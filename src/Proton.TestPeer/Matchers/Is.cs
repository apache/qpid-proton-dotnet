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

using Apache.Qpid.Proton.Test.Driver.Matchers.Core;

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Collection of static matcher factory methods useful for writing tests.
   /// </summary>
   public static class Is
   {
      /// <summary>
      /// Returns a matcher instance that matches only if the expected value is equal
      /// to the actual value provided.
      /// </summary>
      /// <param name="expected">The value that should match the actual</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher EqualTo(object expected)
      {
         return IsEqualMatcher.EqualTo(expected);
      }

      /// <summary>
      /// Creates a new matcher that is the logical negation of the given matcher outcome.
      /// </summary>
      /// <param name="matcher">The matcher instance whose value is negated</param>
      /// <returns>A new matcher instance that will negate the given source</returns>
      public static IMatcher Not(IMatcher matcher)
      {
         return IsNotMatcher.Not(matcher);
      }

      /// <summary>
      /// Creates a new matcher that is the logical negation of the EqualTo outcome.
      /// </summary>
      /// <param name="matcher">The value that the tested value should not be equal to</param>
      /// <returns>A new matcher instance that will match if the source is not the expected</returns>
      public static IMatcher Not(object unexpected)
      {
         return IsNotMatcher.Not(unexpected);
      }

      /// <summary>
      /// Creates a new matcher instance that matches is the value matched upon is null.
      /// </summary>
      /// <returns>A null value checking matcher</returns>
      public static IMatcher NullValue()
      {
         return IsNullMatcher.NullValue();
      }

      /// <summary>
      /// Creates a new matcher instance that matches is the value matched upon is not null.
      /// </summary>
      /// <returns>A not null value checking matcher</returns>
      public static IMatcher NotNullValue()
      {
         return IsNullMatcher.NotNullValue();
      }
   }
}