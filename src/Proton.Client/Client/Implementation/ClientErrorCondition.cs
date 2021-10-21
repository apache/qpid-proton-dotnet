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
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Client error condition implementation which serves as a layer over the proton
   /// based version allowing for external implementations and mapping to / from
   /// Symbol types which the client tries not to expose.
   /// </summary>
   public sealed class ClientErrorCondition : IErrorCondition
   {
      private readonly Types.Transport.ErrorCondition error;

      /// <summary>
      /// Converts from an external IErrorCondition implementation into the internal type.
      /// </summary>
      /// <param name="condition">The condition to convert</param>
      /// <exception cref="ArgumentNullException"></exception>
      internal ClientErrorCondition(IErrorCondition condition)
      {
         if (condition == null)
         {
            throw new ArgumentNullException("The error condition value cannot be null");
         }

         error = new Types.Transport.ErrorCondition(
            Symbol.Lookup(condition.Condition), condition.Description, ClientConversionSupport.ToSymbolKeyedMap(condition.Info));
      }

      /// <summary>
      /// Wraps a Proton ErrorCondition in a client error condition instance to present to
      /// the user in a managed way.
      /// </summary>
      /// <param name="condition"></param>
      /// <exception cref="ArgumentNullException"></exception>
      internal ClientErrorCondition(Types.Transport.ErrorCondition condition)
      {
         if (condition == null)
         {
            throw new ArgumentNullException("The error condition value cannot be null");
         }

         error = condition;
      }

      /// <summary>
      /// Creates a new error condition instance with the given information.
      /// </summary>
      /// <param name="condition">The error condition symbolic name</param>
      /// <param name="description">The description to provide for the error</param>
      /// <param name="info">optional info map to provide with the error</param>
      /// <exception cref="ArgumentNullException"></exception>
      public ClientErrorCondition(string condition, string description, IEnumerable<KeyValuePair<string, object>> info = null)
      {
         if (condition == null)
         {
            throw new ArgumentNullException("The error condition value cannot be null");
         }

         error = new Types.Transport.ErrorCondition(
            Symbol.Lookup(condition), description, ClientConversionSupport.ToSymbolKeyedMap(info));
      }

      #region Access API for the IErrorCondition implementation

      public string Condition => error?.Condition?.ToString();

      public string Description => error?.Description;

      public IReadOnlyDictionary<string, object> Info => ClientConversionSupport.ToStringKeyedMap(error?.Info);

      /// <summary>
      /// Provides internal client access to the wrapped proton type
      /// </summary>
      internal Types.Transport.ErrorCondition ProtonErrorCondition => error;

      #endregion

      public static Types.Transport.ErrorCondition AsProtonErrorCondition(IErrorCondition condition)
      {
         if (condition == null)
         {
            return null;
         }
         else if (condition is ClientErrorCondition)
         {
            return ((ClientErrorCondition)condition).ProtonErrorCondition;
         }
         else
         {
            return new ClientErrorCondition(condition).ProtonErrorCondition;
         }
      }
   }
}