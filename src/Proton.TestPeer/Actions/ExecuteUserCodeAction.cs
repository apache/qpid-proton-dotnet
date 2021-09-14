/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed With
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance With
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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Runs the given user code either now, later or after other test expectations have occurred.
   /// </summary>
   /// <remarks>
   /// The given action will be executed on the common thread pool to prevent any blocking
   /// operations from affecting the test driver event loop itself.
   /// </remarks>
   public class ExecuteUserCodeAction : ScriptedAction
   {
      private readonly AMQPTestDriver driver;
      private readonly Action action;

      private long? delay;

      public ExecuteUserCodeAction(AMQPTestDriver driver, Action action)
      {
         if (driver == null)
         {
            throw new AssertionError("Driver instance cannot be null");
         }

         if (action == null)
         {
            throw new AssertionError("User action instance cannot be null");
         }

         this.driver = driver;
         this.action = action;
      }

      /// <summary>
      /// Used only when queuing a scripted user code action which should delay once the
      /// queued action is finalled reached, this is a rare edge case test mechanism and
      /// could cause issues if other scripted actions depend on executing after this but
      /// are not suitably delayed themselves.
      /// </summary>
      /// <param name="delay">time in milliseconds to delay</param>
      /// <returns>This user code execution action instance</returns>/
      public ExecuteUserCodeAction AfterDelay(long delay)
      {
         this.delay = delay;
         return this;
      }

      public override ExecuteUserCodeAction Later(long delay)
      {
         driver.AfterDelay(delay, this);
         return this;
      }

      public override ExecuteUserCodeAction Now()
      {
         Task.Run(action);
         return this;
      }

      public override ExecuteUserCodeAction Perform(AMQPTestDriver driver)
      {
         if (delay != null)
         {
            driver.AfterDelay((long)delay, new ProxyDelayedScriptedAction(this));
         }
         else
         {
            return Now();
         }

         return this;
      }

      public override ExecuteUserCodeAction Queue()
      {
         driver.AddScriptedElement(this);
         return this;
      }
   }

   internal sealed class ProxyUserCodeAction : ScriptedAction
   {
      private readonly ExecuteUserCodeAction action;

      public ProxyUserCodeAction(ExecuteUserCodeAction action)
      {
         this.action = action;
      }

      public override ScriptedAction Later(long millis)
      {
         return this;
      }

      public override ScriptedAction Now()
      {
         return this;
      }

      public override ScriptedAction Perform(AMQPTestDriver driver)
      {
         action.Now();
         return this;
      }

      public override ScriptedAction Queue()
      {
         return this;
      }
   }
}