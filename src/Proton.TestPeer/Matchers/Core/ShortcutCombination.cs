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
   /// A shortcut matcher for combinations of matching operations that succeeds
   /// or fails based on a selection from the concrete implementation which
   /// selects if success is based on a match or the failure to match any one
   /// matcher in a series of matchers.
   /// </summary>
   public abstract class ShortcutCombination : BaseMatcher<object>
   {
      protected readonly IEnumerable<IMatcher> matchers;

      public ShortcutCombination(params IMatcher[] matchers) : this(new List<IMatcher>(matchers))
      {
      }

      public ShortcutCombination(IEnumerable<IMatcher> matchers)
      {
         this.matchers = matchers;
      }

      public void DescribeTo(IDescription description, string @operator)
      {
         description.AppendList("(", " " + @operator + " ", ")", matchers);
      }

      protected bool Matches(object o, bool shortcut)
      {
         foreach (IMatcher matcher in matchers)
         {
            if (matcher.Matches(o) == shortcut)
            {
               return shortcut;
            }
         }

         return !shortcut;
      }
   }
}