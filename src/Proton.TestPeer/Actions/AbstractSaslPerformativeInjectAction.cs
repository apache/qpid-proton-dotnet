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

using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type for a test script which produces some output or otherwise affects
   /// the state of the test driver in a proactive manner.  This action produces SASL
   /// performatives used when driving a scripted SASL authentication interaction.
   /// </summary>
   public abstract class AbstractSaslPerformativeInjectAction<T> : ScriptedAction where T : IDescribedType
   {
      private readonly AMQPTestDriver driver;

      private ushort? channel;

      public AbstractSaslPerformativeInjectAction(AMQPTestDriver driver)
      {
         this.driver = driver;
      }

      public override AbstractSaslPerformativeInjectAction<T> Later(long delay)
      {
         driver.AfterDelay(delay, this);
         return this;
      }

      public override AbstractSaslPerformativeInjectAction<T> Now()
      {
         Perform(driver);
         return this;
      }

      public override AbstractSaslPerformativeInjectAction<T> Perform(AMQPTestDriver driver)
      {
         driver.SendSaslFrame(OnChannel() ?? 0, Performative);
         return this;
      }

      public override AbstractSaslPerformativeInjectAction<T> Queue()
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

      public abstract T Performative { get; }
   }
}