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

using System;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type for a test script which produces some output or otherwise affects
   /// the state of the test driver in a proactive manner.
   /// </summary>
   public abstract class AbstractPerformativeInjectAction<T> : ScriptedAction where T : IDescribedType
   {
      private readonly AMQPTestDriver driver;

      protected ushort? channel;

      protected long delay = -1;

      public AbstractPerformativeInjectAction(AMQPTestDriver driver)
      {
         this.driver = driver;
      }

      public override AbstractPerformativeInjectAction<T> Later(long millis)
      {
         driver.AfterDelay(millis, this);
         return this;
      }

      public override AbstractPerformativeInjectAction<T> Now()
      {
         BeforeActionPerformed(driver);
         driver.SendAMQPFrame(channel ?? 0, Performative, Payload);

         return this;
      }

      public override AbstractPerformativeInjectAction<T> Queue()
      {
         driver.AddScriptedElement(this);
         return this;
      }

      public override AbstractPerformativeInjectAction<T> Perform(AMQPTestDriver driver)
      {
         if (delay > 0)
         {
            driver.AfterDelay(delay, new ProxyDelayedScriptedAction(this));
         }
         else
         {
            Now();
         }

         return this;
      }

      public ScriptedAction OnChannel(ushort channel)
      {
         this.channel = channel;
         return this;
      }

      internal ushort? OnChannel()
      {
         return channel;
      }

      /// <summary>
      /// Returns the channel this action was instructed to operate on or null if
      /// nothing configured which allows the action code to select an appropriate
      /// channel or use the default channel zero.
      /// </summary>
      protected ushort? ConfiguredChannel => channel;

      /// <summary>
      /// Returns the resulting object that this action injects into the driver
      /// when it executes its action.
      /// </summary>
      public abstract T Performative { get; }

      /// <summary>
      /// The binary payload that results from this action which should be fired
      /// by the test driver.
      /// </summary>
      public virtual byte[] Payload => null;

      /// <summary>
      /// Provides an event hook that the subclass can override to perform any necessary
      /// work before this action is performed by the test driver.
      /// </summary>
      /// <param name="driver"></param>
      protected virtual void BeforeActionPerformed(AMQPTestDriver driver)
      {
      }
   }
}