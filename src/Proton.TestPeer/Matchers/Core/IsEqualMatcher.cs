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
using System.Collections;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Core
{
   /// <summary>
   /// A type matcher that examins a list of matcher and tests them against
   /// some input, if any single matcher fails to match the testing stops and
   /// an error description is created to describe that mismatch.
   /// </summary>
   public sealed class IsEqualMatcher : BaseMatcher<object>
   {
      private readonly object expectedValue;

      public IsEqualMatcher(object equalArg)
      {
         expectedValue = equalArg;
      }

      public override bool Matches(object actualValue)
      {
         return AreEqual(actualValue, expectedValue);
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendValue(expectedValue);
      }

      private static bool AreEqual(object actual, object expected)
      {
         if (ReferenceEquals(actual, expected))
         {
            return true;
         }

         if (ReferenceEquals(null, actual) || ReferenceEquals(null, expected))
         {
            return false;
         }

         if (expected is Array && actual is Array)
         {
            return ArraysMatch((Array)expected, (Array)actual);
         }
         else if(expected is IDictionary && actual is IDictionary)
         {
            return DictionariesMatch((IDictionary)expected, (IDictionary)actual);
         }
         else
         {
            return actual.Equals(expected);
         }
      }

      public static IMatcher EqualTo(object operand)
      {
         return new IsEqualMatcher(operand);
      }

      public static bool ArraysMatch(Array expected, Array actual)
      {
         if (expected.Length != actual.Length)
         {
            return false;
         }

         for (int i = 0; i < expected.Length; ++i)
         {
            object expectedN = expected.GetValue(i);
            object actualN = actual.GetValue(i);

            if (ReferenceEquals(actualN, expectedN))
            {
               return true;
            }

            if (ReferenceEquals(null, actualN) || ReferenceEquals(null, expectedN))
            {
               return false;
            }

            if (!actualN.Equals(expectedN))
            {
               return false;
            }
         }

         return true;
      }

      public static bool DictionariesMatch(IDictionary expected, IDictionary actual)
      {
         if (expected.Count != actual.Count)
         {
            return false;
         }

         return false;
      }
   }
}