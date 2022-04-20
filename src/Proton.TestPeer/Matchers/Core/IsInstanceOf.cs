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
   /// Tests for a value being an instance of a given type which can
   /// pass as long as the given object is assignable to the provided type.
   /// </summary>
   public sealed class IsInstanceOfMatcher : DiagnosingMatcher<object>
   {
      private readonly Type expectedType;

      public IsInstanceOfMatcher(Type expectedType)
      {
         this.expectedType = expectedType;
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("an instance of ").AppendText(expectedType.Name);
      }

      protected override bool Matches(object item, IDescription mismatch)
      {
         if (null == item)
         {
            mismatch.AppendText("null");
            return false;
         }

         if (!expectedType.IsAssignableFrom(item.GetType()))
         {
            mismatch.AppendValue(item).AppendText(" is a " + item.GetType().Name);
            return false;
         }

         return true;
      }

      /// <summary>
      /// Creates a matcher that matches when the examined object is an instance of the specified
      /// Type as determined by calling the Type IsAssignableFrom method on that type, passing the
      /// the examined object.
      /// </summary>
      /// <param name="type">The type that is expected</param>
      /// <returns>A new matcher that examins the type of a target object</returns>
      public static IMatcher InstanceOf(Type type)
      {
         return new IsInstanceOfMatcher(type);
      }

      /// <summary>
      /// Creates a matcher that matches when the examined object is an instance of the specified
      /// Type as determined by calling the Type IsAssignableFrom method on that type, passing the
      /// the examined object.
      /// </summary>
      /// <param name="type">The type that is expected</param>
      /// <returns>A new matcher that examins the type of a target object</returns>
      public static IMatcher Any(Type type)
      {
         return new IsInstanceOfMatcher(type);
      }
   }
}
