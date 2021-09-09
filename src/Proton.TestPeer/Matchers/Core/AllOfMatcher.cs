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
   /// A type matcher that examins a list of matcher and tests them against
   /// some input, if any single matcher fails to match the testing stops and
   /// an error description is created to describe that mismatch.
   /// </summary>
   public sealed class AllOfMatcher : DiagnosingMatcher<object>
   {
      private readonly IEnumerable<IMatcher> matchers;

      public AllOfMatcher(params IMatcher[] matchers) : this(new List<IMatcher>(matchers))
      {
      }

      public AllOfMatcher(IEnumerable<IMatcher> matchers)
      {
         this.matchers = matchers;
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendList("(", " " + "and" + " ", ")", matchers);
      }

      protected override bool Matches(object actual, IDescription mismatchDescription)
      {
         foreach (IMatcher matcher in matchers)
         {
            if (!matcher.Matches(actual))
            {
               mismatchDescription.AppendDescriptionOf(matcher).AppendText(" ");
               matcher.DescribeMismatch(actual, mismatchDescription);
               return false;
            }
         }

         return true;
      }

      /// <summary>
      /// Returns a matcher that matches if and only if all provided matchers match the
      /// actual value.  If any one matcher fails the operation stops and a failure is
      /// generated.
      /// </summary>
      /// <param name="matchers"></param>
      /// <returns></returns>
      public static IMatcher AllOf(IEnumerable<IMatcher> matchers)
      {
         return new AllOfMatcher(matchers);
      }
   }
}