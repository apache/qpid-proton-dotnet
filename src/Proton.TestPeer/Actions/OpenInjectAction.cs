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

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Performative into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public class OpenInjectAction : AbstractPerformativeInjectAction<Open>
   {
      private readonly Open open = new Open();

      private bool explicitlyNullHandle;

      public OpenInjectAction(AMQPTestDriver driver) : base(driver)
      {
      }

      public override Open Performative => open;

      public OpenInjectAction WithContainerId(string containerId)
      {
         open.ContainerId = containerId;
         return this;
      }

      public OpenInjectAction WithHostname(string hostname)
      {
         open.Hostname = hostname;
         return this;
      }

      public OpenInjectAction WithMaxFrameSize(uint maxFrameSize)
      {
         open.MaxFrameSize = maxFrameSize;
         return this;
      }

      public OpenInjectAction WithChannelMax(ushort channelMax)
      {
         open.ChannelMax = channelMax;
         return this;
      }

      public OpenInjectAction WithIdleTimeOut(uint idleTimeout)
      {
         open.IdleTimeout = idleTimeout;
         return this;
      }

      public OpenInjectAction WithOutgoingLocales(params string[] outgoingLocales)
      {
         open.OutgoingLocales = TypeMapper.ToSymbolArray(outgoingLocales);
         return this;
      }

      public OpenInjectAction WithOutgoingLocales(params Symbol[] outgoingLocales)
      {
         open.OutgoingLocales = outgoingLocales;
         return this;
      }

      public OpenInjectAction WithIncomingLocales(params string[] incomingLocales)
      {
         open.IncomingLocales = TypeMapper.ToSymbolArray(incomingLocales);
         return this;
      }

      public OpenInjectAction WithIncomingLocales(params Symbol[] incomingLocales)
      {
         open.IncomingLocales = incomingLocales;
         return this;
      }

      public OpenInjectAction WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         open.OfferedCapabilities = TypeMapper.ToSymbolArray(offeredCapabilities);
         return this;
      }

      public OpenInjectAction WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         open.OfferedCapabilities = offeredCapabilities;
         return this;
      }

      public OpenInjectAction WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         open.DesiredCapabilities = TypeMapper.ToSymbolArray(desiredCapabilities);
         return this;
      }

      public OpenInjectAction WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         open.DesiredCapabilities = desiredCapabilities;
         return this;
      }

      public OpenInjectAction WithProperties(IDictionary<string, object> properties)
      {
         open.Properties = TypeMapper.ToSymbolKeyedMap(properties);
         return this;
      }

      public OpenInjectAction WithProperties(IDictionary<Symbol, object> properties)
      {
         open.Properties = properties;
         return this;
      }
      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         if (Performative.ContainerId == null)
         {
            Performative.ContainerId = "driver";
         }

         if (channel == null)
         {
            channel = 0;
         }
      }
   }
}