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

namespace Apache.Qpid.Proton.Engine.Exceptions
{
   /// <summary>
   /// Support class used to easily create various ProtonException types.
   /// </summary>
   public static class ProtonExceptionSupport
   {
      /// <summary>
      /// Create an EngineFailedException if the given instance is not already of that
      /// type and return it.
      /// </summary>
      /// <param name="cause">The exception that indicates the failure reason.</param>
      /// <returns>An EngineFailedException</returns>
      public static EngineFailedException CreateFailedException(Exception cause)
      {
         return CreateFailedException(null, cause);
      }

      /// <summary>
      /// Create an EngineFailedException if the given instance is not already of that
      /// type and return it.
      /// </summary>
      /// <param name="message">The error message to assign the new failed exception</param>
      /// <param name="cause">The exception that indicates the failure reason.</param>
      /// <returns>An EngineFailedException</returns>
      public static EngineFailedException CreateFailedException(string message, Exception cause)
      {
         if (cause is EngineFailedException exception)
         {
            return exception;
         }
         else if (cause?.InnerException is EngineFailedException)
         {
            return (EngineFailedException)cause.InnerException;
         }
         else
         {
            if (string.IsNullOrEmpty(message))
            {
               message = cause?.Message ?? cause?.GetType().Name;
            }

            return new EngineFailedException(message, cause);
         }
      }
   }
}