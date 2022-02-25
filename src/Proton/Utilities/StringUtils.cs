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

using System.Collections.Generic;
using System.Text;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// A collection of utility methods used for String types such as truncation
   /// and pretty print methods that provide consistent formats across proton
   /// implementations.
   /// </summary>
   public static class StringUtils
   {
      private static readonly int QuotedStringLimit = 64;

      /// <summary>
      /// Given an array of string objects, convert them to a matching array of
      /// their Symbol equivalent values.
      /// </summary>
      /// <param name="stringArray"></param>
      /// <returns>A new Symbol array with values that correspond to the input stings</returns>
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

      /// <summary>
      /// Given an array of Symbol objects, convert them to a matching array of
      /// their string equivalent values.
      /// </summary>
      /// <param name="symbolArray"></param>
      /// <returns>A new string array with values that correspond to the input Symbols</returns>
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

      /// <summary>
      /// Converts the Binary to a quoted string using a default max length before truncation
      /// value and appends a truncation indication if the string required truncation.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <returns>The quoted string value of the given buffer</returns>
      public static string ToQuotedString(in IProtonBuffer buffer)
      {
         return ToQuotedString(buffer, QuotedStringLimit, true);
      }

      /// <summary>
      /// Converts the buffer to a quoted string using a default max length before truncation value.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <param name="appendIfTruncated">appends "...(truncated)" if not all of the payload is present in the string</param>
      /// <returns>The quoted string value of the given buffer</returns>
      public static string ToQuotedString(in IProtonBuffer buffer, in bool appendIfTruncated)
      {
         return ToQuotedString(buffer, QuotedStringLimit, appendIfTruncated);
      }

      /// <summary>
      /// Converts the buffer to a quoted string using a default max length before truncation value.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <param name="quotedStringLimit">the maximum length of stringified content (excluding the quotes,
      /// and truncated indicator)</param>
      /// <param name="appendIfTruncated">appends "...(truncated)" if not all of the payload is present in
      /// the string</param>
      /// <returns>The quoted string value of the given buffer</returns>
      public static string ToQuotedString(in IProtonBuffer buffer, in int quotedStringLimit, in bool appendIfTruncated)
      {
         if (buffer == null)
         {
            return "\"\"";
         }

         StringBuilder str = new StringBuilder();
         str.Append("\"");

         long byteToRead = buffer.ReadableBytes;
         long size = 0;
         bool truncated = false;

         for (long i = 0; i < byteToRead; ++i)
         {
            sbyte c = buffer.GetByte(i);

            if (c > 31 && c < 127 && c != '\\')
            {
               if (size + 1 <= quotedStringLimit)
               {
                  size += 1;
                  str.Append((char)c);
               }
               else
               {
                  truncated = true;
                  break;
               }
            }
            else
            {
               if (size + 4 <= quotedStringLimit)
               {
                  size += 4;
                  str.Append(string.Format("%{0:x2}", c));
               }
               else
               {
                  truncated = true;
                  break;
               }
            }
         }

         str.Append("\"");

         if (truncated && appendIfTruncated)
         {
            str.Append("...(truncated)");
         }

         return str.ToString();
      }
   }
}