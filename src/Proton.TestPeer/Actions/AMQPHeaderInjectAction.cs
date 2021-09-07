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

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Header into a test script to
   /// drive the connect phase of the AMQP connection lifecycle.
   /// </summary>
   public sealed class AMQPHeaderInjectAction : IScriptedAction
   {
      private readonly AMQPTestDriver driver;
      private readonly AMQPHeader header;

      private ushort? channel;

      public AMQPHeaderInjectAction(AMQPTestDriver driver, AMQPHeader header)
      {
         this.driver = driver;
         this.header = header;
      }

      public IScriptedAction Later(long delay)
      {
         driver.AfterDelay(delay, this);
         return this;
      }

      public IScriptedAction Now()
      {
         Perform(driver);
         return this;
      }

      public IScriptedAction Perform(AMQPTestDriver driver)
      {
         driver.SendHeader(header);
         return this;
      }

      public IScriptedAction Queue()
      {
         driver.AddScriptedElement(this);
         return this;
      }
   }
}