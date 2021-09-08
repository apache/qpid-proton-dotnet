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

using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Performative into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public class DetachLastCoordinatorInjectAction : DetachInjectAction
   {
      public DetachLastCoordinatorInjectAction(AMQPTestDriver driver) : base(driver)
      {
      }

      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         LinkTracker tracker = driver.Sessions.LastOpenedCoordinator;

         if (tracker == null)
         {
            throw new AssertionError("Cannot send coordinator detach as scripted, no active coordinator found.");
         }

         channel = tracker.Session.LocalChannel;

         if (!tracker.IsLocallyAttached)
         {
            AttachInjectAction attach = new AttachInjectAction(driver);

            attach.OnChannel((ushort)channel);
            attach.WithName(tracker.Name);
            attach.WithSource(tracker.RemoteSource);
            if (tracker.RemoteTarget != null)
            {
               attach.WithTarget(tracker.RemoteTarget);
            }
            else
            {
               attach.WithTarget(tracker.RemoteCoordinator);
            }

            if (tracker.IsSender)
            {
               attach.WithRole(Role.Sender);
               // Signal that a detach is incoming since an error was set
               // the action will not override an explicitly null source.
               if (Performative.Error != null)
               {
                  attach.WithNullSource();
               }
            }
            else
            {
               attach.WithRole(Role.Receiver);
               // Signal that a detach is incoming since an error was set
               // the action will not override an explicitly null target.
               if (Performative.Error != null)
               {
                  attach.WithNullTarget();
               }
            }

            attach.Perform(driver);
         }

         Performative.Handle = tracker.Handle;
      }
   }
}