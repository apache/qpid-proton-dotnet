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
using System.Threading;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Header into a test script to
   /// drive the connect phase of the AMQP connection lifecycle.
   /// </summary>
   public sealed class ScriptCompleteAction : ScriptedAction
   {
      private readonly AMQPTestDriver driver;
      private readonly CountdownEvent complete = new CountdownEvent(1);
      private readonly ILogger<ScriptCompleteAction> logger;

      public ScriptCompleteAction(AMQPTestDriver driver)
      {
         this.driver = driver;
         this.logger = driver.LoggerFactory.CreateLogger<ScriptCompleteAction>();
      }

      public override ScriptCompleteAction Later(long delay)
      {
         driver.AfterDelay(delay, this);
         return this;
      }

      public override ScriptCompleteAction Now()
      {
         logger.LogTrace("{0} Script compeltion action triggered on Now call", driver.Name);
         complete.Signal();
         return this;
      }

      public override ScriptCompleteAction Perform(AMQPTestDriver driver)
      {
         logger.LogTrace("{0} Script compeltion action performing action via script driver", driver.Name);
         complete.Signal();
         return this;
      }

      public override ScriptCompleteAction Queue()
      {
         driver.AddScriptedElement(this);
         return this;
      }

      public void Await()
      {
         complete.Wait();
      }

      public void Await(long timeout)
      {
         if (!complete.Wait(TimeSpan.FromMilliseconds(timeout)))
         {
            throw new AssertionError("Timed out waiting for scripted expectations to be met", new TimeoutException());
         }
      }

      public void Await(TimeSpan timeout)
      {
         if (!complete.Wait(timeout))
         {
            throw new AssertionError("Timed out waiting for scripted expectations to be met", new TimeoutException());
         }
      }
   }
}