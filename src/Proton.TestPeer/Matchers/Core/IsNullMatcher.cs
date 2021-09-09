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
   /// A type matcher that checks if the target value is null if not the
   /// match fails.
   /// </summary>
   public sealed class IsNullMatcher : BaseMatcher<object>
   {
      public override void DescribeTo(IDescription description)
      {
         description.AppendText("null");
      }

      public override bool Matches(object actual)
      {
         return actual == null;
      }

      /// <summary>
      /// Creates and returns a matcher that checks that a given value is null
      /// </summary>
      /// <returns>A null checking matcher instance</returns>
      public static IMatcher NullValue()
      {
         return new IsNullMatcher();
      }

      /// <summary>
      /// Creates and returns a matcher that checks that a given value is not null
      /// </summary>
      /// <returns>A not null checking matcher instance</returns>
      public static IMatcher NotNullValue()
      {
         return IsNotMatcher.Not(NullValue());
      }
   }
}