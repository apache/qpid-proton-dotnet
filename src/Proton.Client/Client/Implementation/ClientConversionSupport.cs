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
using System.Linq;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

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

      public static Symbol[] ToSymbolArray(IEnumerable<DeliveryStateType> stateTypeArray)
      {
         Symbol[] result = null;

         if (stateTypeArray != null)
         {
            result = new Symbol[stateTypeArray.Count()];
            for (int i = 0; i < result.Length; ++i)
            {
               result[i] = stateTypeArray.ElementAt(i).AsProtonType();
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

      /// <summary>
      /// Converts an enumeration of string values into a set of Symbol values.
      /// </summary>
      /// <param name="strings">an enumeration of string values</param>
      /// <returns>a set of Symbol value that match the input strings</returns>
      public static ISet<Symbol> ToSymbolSet(in IEnumerable<string> strings)
      {
         ISet<Symbol> result;

         if (strings != null)
         {
            result = new HashSet<Symbol>();
            foreach (string value in strings)
            {
               result.Add(Symbol.Lookup(value));
            }
         }
         else
         {
            result = null;
         }

         return result;
      }

      /// <summary>
      /// Converts an enumeration of Symbol values into a set of string values.
      /// </summary>
      /// <param name="symbols">an enumeration of Symbol values</param>
      /// <returns>a set of string value that match the input Symbols</returns>
      public static ISet<string> ToStringSet(in IEnumerable<Symbol> symbols)
      {
         ISet<string> result;

         if (symbols != null)
         {
            result = new HashSet<string>();
            foreach (Symbol value in symbols)
            {
               result.Add(value.ToString());
            }
         }
         else
         {
            result = null;
         }

         return result;
      }

      public static Symbol AsProtonType(this DeliveryStateType mode)
      {
         switch (mode)
         {
            case DeliveryStateType.Accepted:
               return Accepted.DescriptorSymbol;
            case DeliveryStateType.Rejected:
               return Rejected.DescriptorSymbol;
            case DeliveryStateType.Modified:
               return Modified.DescriptorSymbol;
            case DeliveryStateType.Released:
               return Released.DescriptorSymbol;
         }

         throw new ArgumentException("Cannot convert unknown delivery state type: " + mode);
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