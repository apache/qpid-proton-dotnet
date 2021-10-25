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
   // TODO
   /// <summary>
   /// Conversion utilities useful for multiple client operations.
   /// </summary>
   internal static class ClientConversionSupport
   {
      public static Symbol[] ToSymbolArray(string[] stringArray)
      {
         Symbol[] result = null;

         if (stringArray != null)
         {
            result = new Symbol[stringArray.Length];
            for (int i = 0; i < stringArray.Length; ++i)
            {
               result[i] = Symbol.Lookup(stringArray[i]);
            }
         }

         return result;
      }

      public static Symbol[] ToSymbolArray(DeliveryStateType[] stateTypeArray)
      {
         Symbol[] result = null;

         if (stateTypeArray != null)
         {
            result = new Symbol[stateTypeArray.Length];
            for (int i = 0; i < stateTypeArray.Length; ++i)
            {
               result[i] = stateTypeArray[i].ToSymbolicType();
            }
         }

         return result;
      }

      public static string[] ToStringArray(Symbol[] symbolArray)
      {
         string[] result = null;

         if (symbolArray != null)
         {
            result = new string[symbolArray.Length];
            for (int i = 0; i < symbolArray.Length; ++i)
            {
               result[i] = symbolArray[i].ToString();
            }
         }

         return result;
      }

      public static Dictionary<Symbol, object> ToSymbolKeyedMap<V>(IEnumerable<KeyValuePair<string, V>> stringsMap)
      {
         Dictionary<Symbol, object> result;

         if (stringsMap != null)
         {
            result = new Dictionary<Symbol, object>();
            foreach (KeyValuePair<string, V> entry in stringsMap)
            {
               result.Add(Symbol.Lookup(entry.Key), entry.Value);
            }
         }
         else
         {
            result = null;
         }

         return result;
      }

      public static Dictionary<string, Object> ToStringKeyedMap<V>(IEnumerable<KeyValuePair<Symbol, V>> symbolMap)
      {
         Dictionary<string, Object> result;

         if (symbolMap != null)
         {
            result = new Dictionary<string, object>();
            foreach (KeyValuePair<Symbol, V> entry in symbolMap)
            {
               result.Add(entry.Key.ToString(), entry.Value);
            }
         }
         else
         {
            result = null;
         }

         return result;
      }

      public static Symbol AsProtonType(this DistributionMode mode)
      {
         switch (mode)
         {
            case DistributionMode.Copy:
               return ClientConstants.COPY;
            case DistributionMode.Move:
               return ClientConstants.MOVE;
            case DistributionMode.None:
               return null;
         }

         throw new ArgumentException("Cannot convert unknown distribution mode: " + mode);
      }

      public static Types.Messaging.TerminusDurability AsProtonType(this DurabilityMode mode)
      {
         switch (mode)
         {
            case DurabilityMode.Configuration:
               return Types.Messaging.TerminusDurability.Configuration;
            case DurabilityMode.None:
               return Types.Messaging.TerminusDurability.None;
            case DurabilityMode.UnsettledState:
               return Types.Messaging.TerminusDurability.UnsettledState;
         }

         throw new ArgumentException("Cannot convert unknown durability mode: " + mode);
      }

      public static Types.Messaging.TerminusExpiryPolicy AsProtonType(this ExpiryPolicy policy)
      {
         switch (policy)
         {
            case ExpiryPolicy.ConnectionClose:
               return Types.Messaging.TerminusExpiryPolicy.ConnectionClose;
            case ExpiryPolicy.LinkClose:
               return Types.Messaging.TerminusExpiryPolicy.LinkDetach;
            case ExpiryPolicy.Never:
               return Types.Messaging.TerminusExpiryPolicy.Never;
            case ExpiryPolicy.SessionClose:
               return Types.Messaging.TerminusExpiryPolicy.SessionEnd;
         }

         throw new ArgumentException("Cannot convert unknown expiry policy: " + policy);
      }
   }
}