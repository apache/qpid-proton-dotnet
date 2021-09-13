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

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   internal sealed class ProxyDelayedScriptedAction : ScriptedAction
   {
      private readonly ScriptedAction parent;

      public ProxyDelayedScriptedAction(ScriptedAction parent)
      {
         this.parent = parent;
      }

      public override ScriptedAction Perform(AMQPTestDriver driver)
      {
         return parent.Now();
      }

      #region Explicitly failing API methods

      public override ScriptedAction Later(long millis)
      {
         throw new NotImplementedException("Delayed action proxy cannot be used outside of scheduling");
      }

      public override ScriptedAction Now()
      {
         throw new NotImplementedException("Delayed action proxy cannot be used outside of scheduling");
      }

      public override ScriptedAction Queue()
      {
         throw new NotImplementedException("Delayed action proxy cannot be used outside of scheduling");
      }

      public override string ToString()
      {
         return parent.ToString();
      }

      #endregion
   }
}