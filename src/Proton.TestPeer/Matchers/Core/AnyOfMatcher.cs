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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Core
{
   /// <summary>
   /// A type matcher that examines a list of matcher and tests them against
   /// some input, if any single matcher succeeds in matching the target value
   /// then testing stops and the match is considered to have passed.
   /// </summary>
   public sealed class AnyOfMatcher : ShortcutCombination
   {
      public AnyOfMatcher(params IMatcher[] matchers) : base(matchers)
      {
      }

      public AnyOfMatcher(IEnumerable<IMatcher> matchers) : base(matchers)
      {
      }

      public override bool Matches(object actual)
      {
         return Matches(actual, true);
      }

      public override void DescribeTo(IDescription description)
      {
         DescribeTo(description, "or");
      }

      /// <summary>
      /// Returns a matcher that matches if any of the provided matchers match the
      /// actual value.
      /// </summary>
      /// <param name="matchers">The collection of matchers that can result in a match</param>
      /// <returns></returns>
      public static IMatcher AnyOf(IEnumerable<IMatcher> matchers)
      {
         return new AnyOfMatcher(matchers);
      }
   }
}