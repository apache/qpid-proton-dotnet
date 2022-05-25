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

using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Performative into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public class DetachInjectAction : AbstractPerformativeInjectAction<Detach>
   {
      private readonly Detach detach = new();

      public DetachInjectAction(AMQPTestDriver driver) : base(driver)
      {
      }

      public override Detach Performative => detach;

      public DetachInjectAction WithHandle(uint handle)
      {
         detach.Handle = handle;
         return this;
      }

      public DetachInjectAction WithClosed(bool closed)
      {
         detach.Closed = closed;
         return this;
      }

      public DetachInjectAction WithErrorCondition(ErrorCondition error)
      {
         detach.Error = error;
         return this;
      }

      public DetachInjectAction WithErrorCondition(string condition, string description)
      {
         detach.Error = new ErrorCondition(new Symbol(condition), description);
         return this;
      }

      public DetachInjectAction WithErrorCondition(Symbol condition, string description)
      {
         detach.Error = new ErrorCondition(condition, description);
         return this;
      }

      public DetachInjectAction WithErrorCondition(string condition, string description, IDictionary<string, object> info)
      {
         detach.Error = new ErrorCondition(new Symbol(condition), description, TypeMapper.ToSymbolKeyedMap(info));
         return this;
      }

      public DetachInjectAction WithErrorCondition(Symbol condition, string description, IDictionary<Symbol, object> info)
      {
         detach.Error = new ErrorCondition(condition, description, info);
         return this;
      }

      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         // A test that is trying to send an unsolicited detach must provide a channel as we
         // won't attempt to make up one since we aren't sure what the intent here is.
         if (channel == null)
         {
            if (driver.Sessions.LastLocallyOpenedSession == null)
            {
               throw new AssertionError("Scripted Action cannot run without a configured channel: " +
                                        "No locally opened session exists to auto select a channel.");
            }

            channel = driver.Sessions.LastLocallyOpenedSession.LocalChannel;
         }

         ushort localChannel = (ushort)channel;
         SessionTracker session = driver.Sessions.SessionFromLocalChannel(localChannel);

         // A test might be trying to send Attach outside of session scope to check for error handling
         // of unexpected performatives so we just allow no session cases and send what we are told.
         if (session != null)
         {
            // Auto select last opened sender on last opened session.  Later an option could
            // be added to allow forcing the handle to be null for testing specification requirements.
            if (detach.Handle == null)
            {
               detach.Handle = session.LastOpenedLink.Handle;
            }

            session.HandleLocalDetach(detach);
         }
      }
   }
}