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
   /// A matcher that expects that a value will match one of an enumeration
   /// of possible values and when it does evaluation stops.
   /// </summary>
   public sealed class IsInMatcher : BaseMatcher<object>
   {
      private readonly IEnumerable<object> values;

      public IsInMatcher(params object[] values) : this(new List<object>(values))
      {
      }

      public IsInMatcher(IEnumerable<object> values)
      {
         this.values = values;
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("one of ");
         description.AppendValueList("{", ", ", "}", values);
      }

      public override bool Matches(object actual)
      {
         foreach (object value in values)
         {
            if (value == null && actual == null)
            {
               return true;
            }
            else if (value.Equals(actual))
            {
               return true;
            }
         }

         return false;
      }

      public static IMatcher In(IEnumerable<object> values)
      {
         return new IsInMatcher(values);
      }

      public static IMatcher OneOf(IEnumerable<object> values)
      {
         return new IsInMatcher(values);
      }
   }
}