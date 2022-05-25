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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Utilities
{
   public static class TypeMapper
   {
      private static readonly int DEFAULT_QUOTED_STRING_LIMIT = 64;

      public static Symbol[] ToSymbolArray(string[] stringArray)
      {
         Symbol[] result = null;

         if (stringArray != null)
         {
            result = new Symbol[stringArray.Length];
            for (int i = 0; i < stringArray.Length; ++i)
            {
               result[i] = new Symbol(stringArray[i]);
            }
         }

         return result;
      }

      public static IDictionary<Symbol, object> ToSymbolKeyedMap(IDictionary<string, object> stringsMap)
      {
         IDictionary<Symbol, object> result;

         if (stringsMap != null)
         {
            result = new Dictionary<Symbol, object>(stringsMap.Count);
            foreach (KeyValuePair<string, object> entry in stringsMap)
            {
               result.Add(new Symbol(entry.Key), entry.Value);
            }
         }
         else
         {
            result = null;
         }

         return result;
      }

      /// <summary>
      /// Converts the Binary to a quoted string using a default max length before truncation value and
      /// appends a truncation indication if the string required truncation.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <returns>The converted string</returns>
      public static string ToQuotedString(Binary buffer)
      {
         return ToQuotedString(buffer, DEFAULT_QUOTED_STRING_LIMIT, true);
      }

      /// <summary>
      /// Converts the byte[] to a quoted string using a default max length before truncation value and
      /// appends a truncation indication if the string required truncation.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <returns>The converted string</returns>
      public static string ToQuotedString(byte[] buffer)
      {
         return ToQuotedString(buffer, DEFAULT_QUOTED_STRING_LIMIT, true);
      }

      /// <summary>
      /// Converts the Binary to a quoted string using a default max length before truncation value and
      /// appends a truncation indication if the string required truncation.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <param name="appendIfTruncated">appends "...(truncated)" if not all of the payload is present in the string</param>
      /// <returns>The converted string</returns>
      public static string ToQuotedString(Binary buffer, bool appendIfTruncated)
      {
         return ToQuotedString(buffer, DEFAULT_QUOTED_STRING_LIMIT, appendIfTruncated);
      }

      /// <summary>
      /// Converts the byte array to a quoted string using a default max length before truncation value and
      /// appends a truncation indication if the string required truncation.
      /// </summary>
      /// <param name="buffer">The byte array to convert to a string</param>
      /// <param name="appendIfTruncated">appends "...(truncated)" if not all of the payload is present in the string</param>
      /// <returns>The converted string</returns>
      public static string ToQuotedString(byte[] buffer, bool appendIfTruncated)
      {
         return ToQuotedString(buffer, DEFAULT_QUOTED_STRING_LIMIT, appendIfTruncated);
      }

      /// <summary>
      /// Converts the Binary to a quoted string using a default max length before truncation value and
      /// appends a truncation indication if the string required truncation.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <param name="stringLength">the maximum length of stringified content (excluding the quotes, and truncated indicator)</param>
      /// <param name="appendIfTruncated">appends "...(truncated)" if not all of the payload is present in the string</param>
      /// <returns>The converted string</returns>
      public static string ToQuotedString(Binary buffer, int stringLength, bool appendIfTruncated)
      {
         if (buffer == null || buffer.Array == null)
         {
            return "\"\"";
         }

         return ToQuotedString(buffer.Array, stringLength, appendIfTruncated);
      }

      /// <summary>
      /// Converts the byte[] to a quoted string using a default max length before truncation value and
      /// appends a truncation indication if the string required truncation.
      /// </summary>
      /// <param name="buffer">The buffer to convert to a string</param>
      /// <param name="stringLength">the maximum length of stringified content (excluding the quotes, and truncated indicator)</param>
      /// <param name="appendIfTruncated">appends "...(truncated)" if not all of the payload is present in the string</param>
      /// <returns>The converted string</returns>
      public static string ToQuotedString(byte[] buffer, int stringLength, bool appendIfTruncated)
      {
         if (buffer == null)
         {
            return "\"\"";
         }

         StringBuilder str = new();
         str.Append('"');

         int byteToRead = buffer.Length;
         int size = 0;
         bool truncated = false;

         for (int i = 0; i < byteToRead; ++i)
         {
            byte c = buffer[i];

            if (c is > 31 and < 127 and not (byte)'\\')
            {
               if (size + 1 <= stringLength)
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
               if (size + 4 <= stringLength)
               {
                  size += 4;
                  str.Append(string.Format("{0:X}", c));
               }
               else
               {
                  truncated = true;
                  break;
               }
            }
         }

         str.Append('"');

         if (truncated && appendIfTruncated)
         {
            str.Append("...(truncated)");
         }

         return str.ToString();
      }
   }
}