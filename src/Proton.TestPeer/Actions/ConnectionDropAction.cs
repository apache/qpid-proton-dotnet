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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to forcibly close a connection during a test.
   /// </summary>
   public sealed class ConnectionDropAction : ScriptedAction
   {
      private readonly ProtonTestPeer peer;

      private long delay;

      public ConnectionDropAction(ProtonTestPeer peer) : base()
      {
         this.peer = peer;
      }

      /// <summary>
      /// Used to set a delay of execution for a queued instance of this action, this
      /// delay is only applied if the action is queued to the test driver using the
      /// Queue API.
      /// </summary>
      public ConnectionDropAction AfterDelay(long delay)
      {
         this.delay = delay;
         return this;
      }

      public override ScriptedAction Later(long delay)
      {
         peer.Driver.AfterDelay(delay, this);
         return this;
      }

      public override ScriptedAction Now()
      {
         //LOG.info("Connection Drop Action closing test peer as scripted");
         peer.Dispose();
         return this;
      }

      public override ScriptedAction Perform(AMQPTestDriver driver)
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

      public override ScriptedAction Queue()
      {
         peer.Driver.AddScriptedElement(this);
         return this;
      }
   }
}