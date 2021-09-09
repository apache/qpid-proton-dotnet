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

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Simple Typed matcher that hands off matching to a match function
   /// implemented in a derived class. Matchers that derive from this base
   /// class are expected to provide diagnostic information about the cause
   /// of the mismatch as opposed to the less strict base matcher.
   /// </summary>
   public abstract class DiagnosingMatcher<T> : BaseMatcher<T>
   {
      public override void DescribeMismatch(object actual, IDescription mismatchDescription)
      {
         Matches(actual, mismatchDescription);
      }

      public override bool Matches(object actual)
      {
         return Matches(actual, IDescription.None);
      }

      protected abstract bool Matches(object item, IDescription mismatchDescription);

   }
}