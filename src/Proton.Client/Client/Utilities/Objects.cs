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

namespace Apache.Qpid.Proton.Client.Utilities
{
   /// <summary>
   /// Static utility methods for validation and or interactions with
   /// various opaque object vales.
   /// </summary>
   public static class Objects
   {
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