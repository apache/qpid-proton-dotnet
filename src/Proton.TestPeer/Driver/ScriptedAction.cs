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

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Action type for a test script which produces some output or otherwise affects
   /// the state of the test driver in a proactive manner.
   /// </summary>
   public abstract class ScriptedAction : IScriptedElement
   {
      ScriptEntryType IScriptedElement.ScriptedType => ScriptEntryType.Action;

      /// <summary>
      /// Runs the scripted action on its associated test driver immediately
      /// regardless of any queued tasks or expected inputs.
      /// </summary>
      /// <returns>The scripted action</returns>
      public abstract ScriptedAction Now();

      /// <summary>
      /// Runs the scripted action on its associated test driver immediately
      /// following the given wait time regardless of any queued tasks or
      /// expected inputs.
      /// </summary>
      /// <param name="milliseconds"></param>
      /// <returns></returns>
      public abstract ScriptedAction Later(long milliseconds);

      /// <summary>
      /// Queues the scripted action for later run after any preceding scripted
      /// elements are performed.
      /// </summary>
      /// <returns>The scripted action</returns>
      public abstract ScriptedAction Queue();

      /// <summary>
      /// Triggers the action to be performed on the given test driver immediately.
      /// </summary>
      /// <param name="driver">The driver to perform the action on</param>
      /// <returns>The scripted action</returns>
      public abstract ScriptedAction Perform(AMQPTestDriver driver);

   }
}