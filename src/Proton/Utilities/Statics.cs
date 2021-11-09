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

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// Static utility methods for validation and or interactions with
   /// various opaque object vales.
   /// </summary>
   public static class Statics
   {
      /// <summary>
      /// Checks if the given value is greater than zero and throws an exception if not.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static int CheckPositive(int value, string name)
      {
         if (value <= 0)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: > 0)");
         }
         return value;
      }

      /// <summary>
      /// Checks if the given value is greater than zero and throws an exception if not.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static long CheckPositive(long value, string name)
      {
         if (value <= 0L)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: > 0)");
         }
         return value;
      }

      /// <summary>
      /// Checks if the given value is less than zero and throws an exception if so.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static int CheckPositiveOrZero(int value, string name)
      {
         if (value < 0)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: >= 0)");
         }
         return value;
      }

      /// <summary>
      /// Checks if the given value is less than zero and throws an exception if so.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <param name="name">The name to use in the exception message for the given value</param>
      /// <returns>The value provided if it passes the check</returns>
      /// <exception cref="ArgumentOutOfRangeException">if the value fails the check</exception>
      public static long CheckPositiveOrZero(long value, string name)
      {
         if (value < 0L)
         {
            throw new ArgumentOutOfRangeException(name + " : " + value + " (expected: >= 0)");
         }
         return value;
      }

      /// <summary>
      /// Used to check for a method argument being null when the precondition
      /// states that it should not be, the method provides for the user to supply
      /// a nessage to use when throwing an Argument null exception if the value
      /// that was passed is indeed null.
      /// </summary>
      /// <param name="value">The value to check for null</param>
      /// <param name="errorMessage">The message to supply when throwing an error</param>
      /// <exception cref="ArgumentNullException">If the given value is null</exception>
      public static void RequireNonNull(object value, string errorMessage)
      {
         if (value == null)
         {
            throw new ArgumentNullException(errorMessage ?? "Value provided cannot be null");
         }
      }
   }
}