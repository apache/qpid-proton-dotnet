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

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// AMQP Empty Frame injection action which can be added to a driver for write at a specific
   /// time or following on from some other action in the test script.
   /// </summary>
   public class EmptyFrameInjectAction : ScriptedAction
   {
      private readonly AMQPTestDriver driver;

      private ushort? channel;

      public EmptyFrameInjectAction(AMQPTestDriver driver)
      {
         this.driver = driver;
      }

      public override EmptyFrameInjectAction Later(long delay)
      {
         driver.AfterDelay(delay, this);
         return this;
      }

      public override EmptyFrameInjectAction Now()
      {
         return Perform(driver);
      }

      public override EmptyFrameInjectAction Perform(AMQPTestDriver driver)
      {
         driver.SendEmptyFrame((ushort)(this.channel == null ? 0 : this.channel));
         return this;
      }

      public override EmptyFrameInjectAction Queue()
      {
         driver.AddScriptedElement(this);
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
   }
}