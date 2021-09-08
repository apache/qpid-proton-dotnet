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

using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Expectation type that defines a want for some incoming activity
   /// that defines success or failure of a scripted test sequence.
   /// </summary>
   public abstract class ScriptedExpectation : IScriptedElement,
                                               IHeaderHandler<AMQPTestDriver>,
                                               IPerformativeHandler<AMQPTestDriver>,
                                               ISaslPerformativeHandler<AMQPTestDriver>
   {
      ScriptEntryType IScriptedElement.ScriptedType => ScriptEntryType.Expectation;

      /// <summary>
      /// Indicates if the scripted element is optional and the test should not fail
      /// if the element desired outcome is not met.
      /// </summary>
      public virtual bool IsOptional => false;

      /// <summary>
      /// Provides a scripted expectation the means of initiating some action
      /// following successful compeltion of the expectation.
      /// </summary>
      /// <returns>a scripted action to perform following the expectation being me</returns>
      public ScriptedAction PerformAfterwards() => null;

   }
}