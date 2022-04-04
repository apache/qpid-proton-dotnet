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

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Description object used to accumulate useful description information for
   /// a matcher usually to describe the reason that a match failed.
   /// </summary>
   public interface IDescription
   {
      /// <summary>
      /// Do nothing description that will behave as if it accepts input
      /// but produces no actual descriptive output.
      /// </summary>
      static readonly IDescription None = new NullDescription();

      /// <summary>
      /// Appends the given text to this description
      /// </summary>
      /// <param name="text">The text to append</param>
      /// <returns>This description instance</returns>
      IDescription AppendText(string text);

      /// <summary>
      /// Appends the descrption of a self describing type into the overall
      /// description being accumulated by this description instance.
      /// </summary>
      /// <param name="selfDescribing">The self describing type to append</param>
      /// <returns>This description instance</returns>
      IDescription AppendDescriptionOf(ISelfDescribing selfDescribing);

      /// <summary>
      /// Appends an arbitrary value to this description, the value will be
      /// appended using the ToString method of the instance.
      /// </summary>
      /// <param name="value">The value to append into this description</param>
      /// <returns>This description instance</returns>
      IDescription AppendValue(object value);

      /// <summary>
      /// Appends a list of values into the description, the values will be prefixed
      /// with the start value and suffixed by the end value provided.
      /// </summary>
      /// <typeparam name="T">The type of values being appended</typeparam>
      /// <param name="start">The prefix text for the list description</param>
      /// <param name="separator">the separator for each list entry</param>
      /// <param name="end">The suffix text for the list description</param>
      /// <param name="values">The collection of actual values to append</param>
      /// <returns>This description instance</returns>
      IDescription AppendValueList<T>(string start, string separator, string end, params T[] values);

      /// <summary>
      /// Appends a list of values into the description, the values will be prefixed
      /// with the start value and suffixed by the end value provided.
      /// </summary>
      /// <typeparam name="T">The type of values being appended</typeparam>
      /// <param name="start">The prefix text for the list description</param>
      /// <param name="separator">the separator for each list entry</param>
      /// <param name="end">The suffix text for the list description</param>
      /// <param name="values">The collection of actual values to append</param>
      /// <returns>This description instance</returns>
      IDescription AppendValueList<T>(string start, string separator, string end, IEnumerable<T> values);

      /// <summary>
      /// Appends a list of self describing values into the description, the values will
      /// be prefixed with the start value and suffixed by the end value provided.
      /// </summary>
      /// <param name="start">The prefix text for the list description</param>
      /// <param name="separator">the separator for each list entry</param>
      /// <param name="end">The suffix text for the list description</param>
      /// <param name="values">The collection of actual values to append</param>
      /// <returns>This description instance</returns>
      IDescription AppendList(string start, string separator, string end, IEnumerable<ISelfDescribing> values);

      /// <summary>
      /// Appends an environment specific newline to the description text
      /// </summary>
      /// <returns>This description instance</returns>
      IDescription AppendNewLine();

   }

   internal sealed class NullDescription : IDescription
   {
      public IDescription AppendDescriptionOf(ISelfDescribing selfDescribing)
      {
         return this;
      }

      public IDescription AppendList(string start, string separator, string end, IEnumerable<ISelfDescribing> values)
      {
         return this;
      }

      public IDescription AppendNewLine()
      {
         return this;
      }

      public IDescription AppendText(string text)
      {
         return this;
      }

      public IDescription AppendValue(object value)
      {
         return this;
      }

      public IDescription AppendValueList<T>(string start, string separator, string end, params T[] values)
      {
         return this;
      }

      public IDescription AppendValueList<T>(string start, string separator, string end, IEnumerable<T> values)
      {
         return this;
      }

      public override string ToString()
      {
         return "";
      }
   }
}