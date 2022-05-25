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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Matchers.Core;

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Collection of static matcher factory methods useful for writing tests.
   /// </summary>
   public static class Matches
   {
      /// <summary>
      /// Creates a matcher instance that matches only if all of the supplied matchers
      /// match against the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">The list of matchers that must all match</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AllOf(IEnumerable<IMatcher> matchers)
      {
         return AllOfMatcher.AllOf(matchers);
      }

      /// <summary>
      /// Creates a matcher instance that matches only if all of the supplied matchers
      /// match against the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">The list of matchers that must all match</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AllOf(params IMatcher[] matchers)
      {
         return AllOfMatcher.AllOf(matchers);
      }

      /// <summary>
      /// Creates a matcher instance that matches if any of the supplied matchers
      /// succeeds in matching the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">A collection of matches to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AnyOf(IEnumerable<IMatcher> matchers)
      {
         return AnyOfMatcher.AnyOf(matchers);
      }

      /// <summary>
      /// Creates a matcher instance that matches if any of the supplied matchers
      /// succeeds in matching the actual value supplied in the match operation.
      /// </summary>
      /// <param name="matchers">A collection of matches to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher AnyOf(params IMatcher[] matchers)
      {
         return AnyOfMatcher.AnyOf(matchers);
      }

      /// <summary>
      /// Creates a matcher that matches when the examined object is an instance of the specified
      /// Type as determined by calling the Type IsAssignableFrom method on that type, passing the
      /// the examined object.
      /// </summary>
      /// <param name="type">The type that is expected</param>
      /// <returns>A new matcher that examines the type of a target object</returns>
      public static IMatcher Any(Type type)
      {
         return IsInstanceOfMatcher.Any(type);
      }

      /// <summary>
      /// Creates a matcher that matches when the examined object is an instance of the specified
      /// Type as determined by calling the Type IsAssignableFrom method on that type, passing the
      /// the examined object.
      /// </summary>
      /// <param name="type">The type that is expected</param>
      /// <returns>A new matcher that examines the type of a target object</returns>
      public static IMatcher InstanceOf(Type type)
      {
         return IsInstanceOfMatcher.InstanceOf(type);
      }

      /// <summary>
      /// Creates a matcher that matches when the examined object is an instance of the specified
      /// Type as determined by calling the Type IsAssignableFrom method on that type, passing the
      /// the examined object.
      /// </summary>
      /// <param name="type">The type that is expected</param>
      /// <returns>A new matcher that examines the type of a target object</returns>
      public static IMatcher IsA(Type type)
      {
         return IsMatcher.IsA(type);
      }

      /// <summary>
      /// Creates a matcher that matches when the examined object is equal to an expected value
      /// provided here. This methods is functionally equivalent to the Is.EqualTo(x) API but
      /// allows some tests to be more expressive.
      /// </summary>
      /// <param name="value">The value that is expected</param>
      /// <returns>A new matcher that compares the value of a target object</returns>
      public static IMatcher Is(object value)
      {
         return IsMatcher.Is(value);
      }

      /// <summary>
      /// Creates a matcher instance that matches the actual value matched against is
      /// contained within the enumeration of possible values provided.
      /// </summary>
      /// <param name="matchers">A collection of values to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher OneOf(IEnumerable<object> values)
      {
         return IsInMatcher.OneOf(values);
      }

      /// <summary>
      /// Creates a matcher instance that matches the actual value matched against is
      /// contained within the enumeration of possible values provided.
      /// </summary>
      /// <param name="matchers">A collection of matches to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher OneOf(params object[] values)
      {
         return IsInMatcher.OneOf(values);
      }

      /// <summary>
      /// Creates a matcher instance that matches the actual value matched against is
      /// contained within the enumeration of possible values provided.
      /// </summary>
      /// <param name="matchers">A collection of values to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher In(IEnumerable<object> values)
      {
         return IsInMatcher.In(values);
      }

      /// <summary>
      /// Creates a matcher instance that matches the actual value matched against is
      /// contained within the enumeration of possible values provided.
      /// </summary>
      /// <param name="matchers">A collection of matches to test against</param>
      /// <returns>A new matcher instance that validates the given criteria</returns>
      public static IMatcher In(params object[] values)
      {
         return IsInMatcher.In(values);
      }
   }
}