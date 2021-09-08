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
   /// Action type used to inject raw streams of binary data into a test.
   /// </summary>
   public sealed class RawBytesInjectAction : ScriptedAction
   {
      private readonly byte[] bytes;
      private readonly AMQPTestDriver driver;

      public RawBytesInjectAction(AMQPTestDriver driver, byte[] bytes)
      {
         this.driver = driver;
         this.bytes = bytes;
      }

      public override RawBytesInjectAction Later(long delay)
      {
         driver.AfterDelay(delay, this);
         return this;
      }

      public override RawBytesInjectAction Now()
      {
         return Perform(driver);
      }

      public override RawBytesInjectAction Perform(AMQPTestDriver driver)
      {
         driver.SendBytes(bytes);
         return this;
      }

      public override RawBytesInjectAction Queue()
      {
         driver.AddScriptedElement(this);
         return this;
      }
   }
}