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

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// A base class used for matchers that require a non null value of a specific type
   /// before attempt any further matching. This matcher provides the basic setup to
   /// get as far as asserting that a value is of a given type then the implementation
   /// must finish the comparison.
   /// </summary>
   public abstract class TypeSafeDiagnosingMatcher<T> : BaseMatcher<T>
   {
      private readonly Type expectedType;

      protected TypeSafeDiagnosingMatcher()
      {
         this.expectedType = typeof(T);
      }

      /// <summary>
      /// Subclass must override to define the actual matching against a value of
      /// the type this matcher operates against.
      /// </summary>
      /// <param name="item">The value casted to the expected type</param>
      /// <returns>The result of the matching test, true if matched</returns>
      protected abstract bool MatchesSafely(T item, IDescription mismatchDescription);

      #region Sealed implementations from the base which leverage safe match APIs declared here

      public sealed override bool Matches(object item)
      {
         return item != null && expectedType.IsAssignableFrom(item.GetType()) && MatchesSafely((T)item, IDescription.None);
      }

      public sealed override void DescribeMismatch(object item, IDescription description)
      {
         if (item == null)
         {
            description.AppendText("was null");
         }
         else if (!expectedType.IsAssignableFrom(item.GetType()))
         {
            description.AppendText("was ")
                       .AppendText(item.GetType().Name)
                       .AppendText(" ")
                       .AppendValue(item);
         }
         else
         {
            MatchesSafely((T)item, description);
         }
      }

      #endregion
   }
}