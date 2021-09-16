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
using System.Globalization;

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Base class for custom description implementations which provides some of
   /// the more universal implementation of description accumulation APIs.
   /// </summary>
   public abstract class BaseDescription : IDescription
   {
      /// <summary>
      /// Appends a given string value into the mechanism that the description
      /// implementation is using as its text store.
      /// </summary>
      /// <param name="text">The text to accumulate</param>
      /// <returns>This description instance.</returns>
      protected abstract IDescription Append(string text);

      public virtual IDescription AppendText(string text)
      {
         Append(text);
         return this;
      }

      public virtual IDescription AppendDescriptionOf(ISelfDescribing selfDescribing)
      {
         selfDescribing.DescribeTo(this);
         return this;
      }

      public virtual IDescription AppendList(string start, string separator, string end, IEnumerable<ISelfDescribing> values)
      {
         bool separate = false;

         Append(start);
         foreach (ISelfDescribing value in values)
         {
            if (separate)
            {
               Append(separator);
            }

            AppendDescriptionOf(value);
            separate = true;
         }

         Append(end);

         return this;
      }

      public virtual IDescription AppendNewLine()
      {
         Append(Environment.NewLine);
         return this;
      }

      public virtual IDescription AppendValue(object value)
      {
         if (value == null)
         {
            Append("null");
         }
         else if (value is Char)
         {
            Append("'");
            Append(((Char)value).ToString());
            Append("'");
         }
         else if (value is string)
         {
            Append("\"");
            Append(value.ToString());
            Append("\"");
         }
         else if (value is long)
         {
            Append(((long)value).ToString(CultureInfo.InvariantCulture) + "L");
         }
         else if (value is ulong)
         {
            Append(((ulong)value).ToString(CultureInfo.InvariantCulture) + "UL");
         }
         else if (value is uint)
         {
            Append(((uint)value).ToString(CultureInfo.InvariantCulture) + "U");
         }
         else if (value is float)
         {
            Append(((float)value).ToString(CultureInfo.InvariantCulture) + "f");
         }
         else if (value is double)
         {
            Append(((double)value).ToString(CultureInfo.InvariantCulture) + "d");
         }
         else if (value is decimal)
         {
            Append(((decimal)value).ToString(CultureInfo.InvariantCulture) + "m");
         }
         else if (value.GetType().IsArray)
         {
            AppendValueList("[", ", ", "]", EnumerateArrayOf((Array)value));
         }
         else
         {
            Append(value.ToString());
         }

         return this;
      }

      public virtual IDescription AppendValueList<T>(string start, string separator, string end, params T[] values)
      {
         AppendValueList(start, separator, end, EnumerateArrayOf(values));
         return this;
      }

      public virtual IDescription AppendValueList<T>(string start, string separator, string end, IEnumerable<T> values)
      {
         AppendList(start, separator, end, EnumerateAsSelfDescribing<T>(values));
         return this;
      }

      protected IEnumerable<ISelfDescribing> EnumerateAsSelfDescribing<T>(IEnumerable<T> values)
      {
         foreach (T value in values)
         {
            yield return new SelfDescribingProxy<T>(value);
         }
      }

      protected IEnumerable<object> EnumerateArrayOf(Array values)
      {
         for (int i = 0; i < values.Length; ++i)
         {
            yield return values.GetValue(i);
         }
      }

      private sealed class SelfDescribingProxy<T> : ISelfDescribing
      {
         private T value;

         public SelfDescribingProxy(T value)
         {
            this.value = value;
         }

         public void DescribeTo(IDescription description)
         {
            description.AppendValue(value);
         }
      }
   }
}