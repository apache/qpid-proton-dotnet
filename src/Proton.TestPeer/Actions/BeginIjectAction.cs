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
   /// Action type used to inject the AMQP Begin into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public sealed class BeginInjectAction : AbstractPerformativeInjectAction<Begin>
   {
      private static readonly uint DEFAULT_WINDOW_SIZE = Int32.MaxValue;

      private readonly Begin begin = new Begin()
      {
         NextOutgoingId = 1,
         IncomingWindow = DEFAULT_WINDOW_SIZE,
         OutgoingWindow = DEFAULT_WINDOW_SIZE
      };

      public BeginInjectAction(AMQPTestDriver driver) : base(driver)
      {
      }

      public override Begin Performative => begin;

      public BeginInjectAction WithRemoteChannel(ushort remoteChannel)
      {
         begin.RemoteChannel = remoteChannel;
         return this;
      }

      public BeginInjectAction WithNextOutgoingId(uint nextOutgoingId)
      {
         begin.NextOutgoingId = nextOutgoingId;
         return this;
      }

      public BeginInjectAction WithIncomingWindow(uint incomingWindow)
      {
         begin.IncomingWindow = incomingWindow;
         return this;
      }

      public BeginInjectAction WithOutgoingWindow(uint outgoingWindow)
      {
         begin.OutgoingWindow = outgoingWindow;
         return this;
      }

      public BeginInjectAction WithHandleMax(uint handleMax)
      {
         begin.HandleMax = handleMax;
         return this;
      }

      public BeginInjectAction WithOfferedCapabilities(params String[] offeredCapabilities)
      {
         begin.OfferedCapabilities = TypeMapper.ToSymbolArray(offeredCapabilities);
         return this;
      }

      public BeginInjectAction WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         begin.OfferedCapabilities = offeredCapabilities;
         return this;
      }

      public BeginInjectAction WithDesiredCapabilities(params String[] desiredCapabilities)
      {
         begin.DesiredCapabilities = TypeMapper.ToSymbolArray(desiredCapabilities);
         return this;
      }

      public BeginInjectAction WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         begin.DesiredCapabilities = desiredCapabilities;
         return this;
      }

      public BeginInjectAction WithProperties(IDictionary<string, object> properties)
      {
         begin.Properties = TypeMapper.ToSymbolKeyedMap(properties);
         return this;
      }

      public BeginInjectAction WithProperties(IDictionary<Symbol, object> properties)
      {
         begin.Properties = properties;
         return this;
      }

      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         // We fill in a channel using the next available channel id if one isn't set, then
         // report the outbound begin to the session so it can track this new session.
         if (channel == null)
         {
            channel = driver.Sessions.FindFreeLocalChannel();
         }

         driver.Sessions.HandleLocalBegin(begin, (ushort)channel);
      }
   }
}