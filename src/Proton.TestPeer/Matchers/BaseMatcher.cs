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

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Base class for custom description implementations which provides some of
   /// the more universal implementation of description accumulation APIs.
   /// </summary>
   public abstract class BaseMatcher<T> : IMatcher
   {
      public virtual void DescribeMismatch(object actual, IDescription description)
      {
         description.AppendText("was ").AppendValue(actual);
      }

      public abstract bool Matches(object actual);

      /// <summary>
      /// Simple is not null check method that fills in description information about
      /// that condition of the actual value being null for the caller.
      /// </summary>
      /// <param name="actual">The actual value being tested</param>
      /// <param name="mismatch">The description instance to write to</param>
      /// <returns></returns>
      protected static bool IsNotNull(object actual, IDescription mismatch)
      {
         if (actual == null)
         {
            mismatch.AppendText("was null");
            return false;
         }

         return true;
      }

      public abstract void DescribeTo(IDescription description);

   }
}