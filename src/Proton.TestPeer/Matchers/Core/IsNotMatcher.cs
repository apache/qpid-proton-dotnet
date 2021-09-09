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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Core
{
   /// <summary>
   /// A type matcher that results in the logical negation of a given
   /// matcher's outcome.
   /// </summary>
   public sealed class IsNotMatcher : BaseMatcher<object>
   {
      private readonly IMatcher matcher;

      public IsNotMatcher(IMatcher matcher)
      {
         this.matcher = matcher;
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("not ").AppendDescriptionOf(matcher);
      }

      public override bool Matches(object actual)
      {
         return !matcher.Matches(actual);
      }

      /// <summary>
      /// Creates and returns a new matcher that wraps the given matcher and inverts
      /// the outcome of that matchers matches function.
      /// </summary>
      /// <param name="matcher">The matcher to wrap</param>
      /// <returns>A new outcome inverting matcher</returns>
      public static IMatcher Not(IMatcher matcher)
      {
         return new IsNotMatcher(matcher);
      }

      /// <summary>
      /// Creates and returns a new matcher that wraps the given expected value and
      /// inverts the outcome of that matchers matches function to create a not equals.
      /// </summary>
      /// <param name="matcher">The value to wrap</param>
      /// <returns>A new equals outcome inverting matcher</returns>
      public static IMatcher Not(object expected)
      {
         return Not(new IsEqualMatcher(expected));
      }
   }
}