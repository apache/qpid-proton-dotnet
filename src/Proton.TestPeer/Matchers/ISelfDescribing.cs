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
   /// Marks an object as having the ability to describe itself for matcher
   /// error message or other output.
   /// </summary>
   public interface ISelfDescribing
   {
      /// <summary>
      /// Provides a description of this object which could be a subset of a
      /// larger description so care should be taken when creating the final
      /// wording of the description text.
      /// </summary>
      /// <param name="description">The description object that collects descriptive text</param>
      void DescribeTo(IDescription description);

   }
}