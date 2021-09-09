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
   /// before attempt any futher matching. This matcher provides the basic setup to
   /// get as far as asserting that a value is of a given type then the implementation
   /// must finish the comparison.
   /// </summary>
   public abstract class TypeSafeMatcher<T> : BaseMatcher<T>
   {
      private readonly Type expectedType;

      protected TypeSafeMatcher()
      {
         this.expectedType = typeof(T);
      }

      /// <summary>
      /// Subclass must override to define the actual matching against a value of
      /// the type this matcher operates against.
      /// </summary>
      /// <param name="item">The value casted to the expected type</param>
      /// <returns>The result of the metching test, true if matched</returns>
      protected abstract bool MatchesSafely(T item);

      /// <summary>
      /// Provides an entry point for the subclass to safely describe the mismatch
      /// with a value cast to the expected type after checks.
      /// </summary>
      /// <param name="item">The value that matched the type expectation but failed matching</param>
      /// <param name="mismatchDescription">The description object to write the messages to</param>
      protected virtual void DescribeMismatchSafely(T item, IDescription mismatchDescription)
      {
         base.DescribeMismatch(item, mismatchDescription);
      }

      #region Sealed implementations from the base which leverage safe match APIs declared here

      public sealed override bool Matches(object item)
      {
         return item != null && expectedType.IsAssignableFrom(item.GetType()) && MatchesSafely((T)item);
      }

      public sealed override void DescribeMismatch(object item, IDescription description)
      {
         if (item == null)
         {
            base.DescribeMismatch(null, description);
         }
         else if (!expectedType.IsAssignableFrom(item.GetType()))
         {
            description.AppendText("was a ")
                       .AppendText(item.GetType().Name)
                       .AppendText(" (")
                       .AppendValue(item)
                       .AppendText(")");
         }
         else
         {
            DescribeMismatchSafely((T)item, description);
         }
      }

      #endregion
   }
}