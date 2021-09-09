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
using Apache.Qpid.Proton.Test.Driver.Matchers.Core;

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Defines simple assertion style APIs that a test or implementation
   /// can use to validate matching assumptions.
   /// </summary>
   public static class MatcherAssert
   {
      public static void AssertThat(object value, IMatcher matcher)
      {

      }

      public static void AssertThat(string reason, object value, IMatcher matcher)
      {
         if (!matcher.Matches(value))
         {
            IDescription description = new StringDescription();
            description.AppendText(reason)
                       .AppendNewLine()
                       .AppendText("Expected: ")
                       .AppendDescriptionOf(matcher)
                       .AppendNewLine()
                       .AppendText("     but: ");
            matcher.DescribeMismatch(value, description);

            throw new ArgumentException(description.ToString());
         }
      }

      public static void AssertThat(string reason, bool matched)
      {
         if (!matched)
         {
            throw new ArgumentException(reason);
         }
      }
   }
}