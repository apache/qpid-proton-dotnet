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

using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Expectations;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Basic test script writing API
   /// </summary>
   public abstract class ScriptWriter
   {
      /// <summary>
      /// Provides the implementation with an entry point to give the script writer code
      /// an AMQPTestDriver instance to use in configuring test expectations and actions.
      /// </summary>
      public abstract AMQPTestDriver Driver { get; }

      #region AMQP Performative Expectations scripting APIs

      public AMQPHeaderExpectation ExpectAMQPHeader()
      {
         AMQPHeaderExpectation expecting = new AMQPHeaderExpectation(AMQPHeader.Header, Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      #endregion
   }
}