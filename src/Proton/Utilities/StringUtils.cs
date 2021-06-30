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

using System.Text;
using Apache.Qpid.Proton.Buffer;

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

         int byteToRead = buffer.ReadableBytes;
         int size = 0;
         bool truncated = false;

         for (int i = 0; i < byteToRead; ++i)
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
                  str.Append(string.Format("\\x%02x", c));
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