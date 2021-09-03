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
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// The AMQP Test driver internal frame processing a script handler class.
   /// </summary>
   public sealed class AMQPTestDriver
   {
      private readonly String driverName;
      private readonly FrameDecoder frameParser;
      private readonly FrameEncoder frameEncoder;

      private Open localOpen;
      private Open remoteOpen;

      // TODO private readonly DriverSessions sessions = new DriverSessions(this);

      private readonly Action<byte[]> frameConsumer;
      private readonly Action<Exception> assertionConsumer;
      private readonly Action<TaskScheduler> schedulerSupplier;

      private volatile Exception failureCause;

      private uint advertisedIdleTimeout = 0;

      private volatile uint emptyFrameCount;
      private volatile uint performativeCount;
      private volatile uint saslPerformativeCount;

      private uint inboundMaxFrameSize = UInt32.MaxValue;
      private uint outboundMaxFrameSize = UInt32.MaxValue;

      /// <summary>
      /// Holds the expectations for processing of data from the peer under test.
      /// Uses a thread safe queue to avoid contention on adding script entries
      /// and processing incoming data (although you should probably not do that).
      /// </summary>
      private readonly Queue<IScriptedElement> script = new Queue<IScriptedElement>();

      public string Name => driverName;

      public uint InboundMaxFrameSize
      {
         get => inboundMaxFrameSize;
         set => inboundMaxFrameSize = value;
      }

      public uint OutboundMaxFrameSize
      {
         get => outboundMaxFrameSize;
         set => outboundMaxFrameSize = value;
      }

      public uint AdvertisedIdleTimeout
      {
         get => advertisedIdleTimeout;
         set => advertisedIdleTimeout = value;
      }

      public uint EmptyFrameCount => emptyFrameCount;

      public uint PerformativeCount => performativeCount;

      public uint SaslPerformativeCount => saslPerformativeCount;

      public Open RemoteOpen => remoteOpen;

      public Open LocalOpen => localOpen;

      internal void AfterDelay(long delay, IScriptedAction action)
      {
         throw new NotImplementedException();
      }

      internal void AddScriptedElement(IScriptedElement element)
      {
         throw new NotImplementedException();
      }

      internal void SendAMQPFrame(ushort? channel, IDescribedType performative, byte[] payload)
      {
         throw new NotImplementedException();
      }

      #region Handlers for frame events

      internal void HandleHeader(AMQPHeader header)
      {
         // TODO
      }

      internal void HandleSaslPerformative(uint frameSize, SaslDescribedType sasl, ushort channel, byte[] payload)
      {
         // TODO
      }

      internal void HandlePerformative(uint frameSize, PerformativeDescribedType amqp, ushort channel, byte[] payload)
      {
         // TODO
      }

      internal void HandleHeartbeat(uint frameSize, ushort channel)
      {
         // TODO
      }

      #endregion
   }
}