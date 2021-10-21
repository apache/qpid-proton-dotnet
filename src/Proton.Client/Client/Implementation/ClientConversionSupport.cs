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

      public static Dictionary<Symbol, object> ToSymbolKeyedMap(IEnumerable<KeyValuePair<string, object>> stringsMap)
      {
         Dictionary<Symbol, object> result;

         if (stringsMap != null)
         {
            result = new Dictionary<Symbol, object>();
            foreach (KeyValuePair<string, object> entry in stringsMap)
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

      public static Dictionary<string, Object> ToStringKeyedMap(IEnumerable<KeyValuePair<Symbol, object>> symbolMap)
      {
         Dictionary<string, Object> result;

         if (symbolMap != null)
         {
            result = new Dictionary<string, object>();
            foreach (KeyValuePair<Symbol, object> entry in symbolMap)
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
   }
}