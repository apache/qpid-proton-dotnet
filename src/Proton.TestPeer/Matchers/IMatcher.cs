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
   /// Defines a matcher interface that provides means of defining well formed
   /// matching expectation over given values.
   /// </summary>
   public interface IMatcher : ISelfDescribing
   {
      /// <summary>
      /// Matches the target type defined by a concrete matcher implementation
      /// against the provided value to determine if they match.  The mechanics
      /// of the matching process are deferred to the matcher implementation.
      /// </summary>
      /// <param name="actual"></param>
      /// <returns></returns>
      bool Matches(object actual);

      /// <summary>
      /// Provides a method for matcher implementations to define descriptive but
      /// concise test that describes why a match failed.
      /// </summary>
      /// <param name="actual"></param>
      /// <param name="mismatchDescription"></param>
      void DescribeMismatch(object actual, IDescription mismatchDescription);

   }
}