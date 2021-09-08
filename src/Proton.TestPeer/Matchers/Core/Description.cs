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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Core
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

      public abstract IDescription AppendList<T>(string start, string separator, string end, IEnumerable<T> values) where T : ISelfDescribing;
      public abstract IDescription AppendNewLine();
      public abstract IDescription AppendValue(object value);
      public abstract IDescription AppendValueList<T>(string start, string separator, string end, params T[] values);
      public abstract IDescription AppendValueList<T>(string start, string separator, string end, IEnumerable<T> values);

   }
}