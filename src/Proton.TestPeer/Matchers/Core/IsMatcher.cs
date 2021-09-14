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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Core
{
   /// <summary>
   /// A type matcher that wraps another matcher but provides a more
   /// expressive API for some tests.
   /// </summary>
   public sealed class IsMatcher : BaseMatcher<object>
   {
      private readonly IMatcher matcher;

      public IsMatcher(IMatcher matcher)
      {
         this.matcher = matcher;
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("is ").AppendDescriptionOf(matcher);
      }

      public override bool Matches(object actual)
      {
         return matcher.Matches(actual);
      }

      public override void DescribeMismatch(object item, IDescription mismatchDescription)
      {
         matcher.DescribeMismatch(item, mismatchDescription);
      }

      /// <summary>
      /// Decorative matcher create method that provides an API for tests that want
      /// to be more expressive of their intent. The original behavior of the wrapped
      /// matcher is maintined.
      /// </summary>
      /// <param name="matcher">The actual matcher to decorate</param>
      /// <returns>A new matcher instance</returns>
      public static IMatcher Is(IMatcher matcher)
      {
         return new IsMatcher(matcher);
      }

      /// <summary>
      /// Shortcut method that tests that frequently use Is(EqualTo(x)).
      /// </summary>
      /// <param name="matcher">The actual matcher to decorate</param>
      /// <returns>A new matcher instance</returns>
      public static IMatcher Is(object value)
      {
         return new IsMatcher(IsEqualMatcher.EqualTo(value));
      }

      /// <summary>
      /// Provides a shortcut API for tests that frequently use Is(InstanceOf(X)).
      /// </summary>
      /// <param name="type">The actual type that the match should be</param>
      /// <returns>A new matcher instance</returns>
      public static IMatcher IsA(Type type)
      {
         return Is(IsInstanceOfMatcher.InstanceOf(type));
      }
   }
}