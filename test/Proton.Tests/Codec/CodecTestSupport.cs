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
using NUnit.Framework;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Codec.Decoders;
using Apache.Qpid.Proton.Codec.Encoders;

namespace Apache.Qpid.Proton.Codec
{
   public abstract class CodecTestSupport
   {
      protected static readonly int LargeSize = 1024;
      protected static readonly int SmallSize = 32;

      protected static readonly int LargeArraySize = 1024;
      protected static readonly int SmallArraySize = 32;

      protected IDecoderState decoderState;
      protected IEncoderState encoderState;
      protected IDecoder decoder;
      protected IEncoder encoder;

      protected IStreamDecoderState streamDecoderState;
      protected IStreamDecoder streamDecoder;

      protected Random random = new Random();
      protected int seed;

      [SetUp]
      public void setUp()
      {
         decoder = ProtonDecoderFactory.Create();
         decoderState = decoder.NewDecoderState();

         encoder = ProtonEncoderFactory.Create();
         encoderState = encoder.NewEncoderState();

         streamDecoder = ProtonStreamDecoderFactory.Create();
         streamDecoderState = streamDecoder.NewDecoderState();

         seed = Environment.TickCount;
         random = new Random(seed);
      }

      public static void AssertTypesEqual(Open open1, Open open2)
      {
         if (open1 == null && open2 == null)
         {
            return;
         }
         else if (open1 == null || open2 == null)
         {
            Assert.AreEqual(open1, open2);
         }

         Assert.AreEqual(open1.ChannelMax, open2.ChannelMax, "Channel max values not equal");
         Assert.AreEqual(open1.ContainerId, open2.ContainerId, "Container Id values not equal");
         Assert.AreEqual(open1.Hostname, open2.Hostname, "Hostname values not equal");
         Assert.AreEqual(open1.IdleTimeout, open2.IdleTimeout, "Idle timeout values not equal");
         Assert.AreEqual(open1.MaxFrameSize, open2.MaxFrameSize, "Max Frame Size values not equal");
         Assert.AreEqual(open1.Properties, open2.Properties, "Properties Map values not equal");
         Assert.AreEqual(open1.DesiredCapabilities, open2.DesiredCapabilities, "Desired Capabilities are not equal");
         Assert.AreEqual(open1.OfferedCapabilities, open2.OfferedCapabilities, "Offered Capabilities are not equal");
         Assert.AreEqual(open1.IncomingLocales, open2.IncomingLocales, "Incoming Locales are not equal");
         Assert.AreEqual(open1.OutgoingLocales, open2.OutgoingLocales, "Outgoing Locales are not equal");
      }

      public static void AssertTypesEqual(Close close1, Close close2)
      {
         if (close1 == null && close2 == null)
         {
            return;
         }
         else if (close1 == null || close2 == null)
         {
            Assert.AreEqual(close1, close2);
         }

         Assert.AreEqual(close1.Error, close2.Error);
      }

      public static void AssertTypesEqual(Begin begin1, Begin begin2)
      {
         if (begin1 == null && begin2 == null)
         {
            return;
         }
         else if (begin1 == null || begin2 == null)
         {
            Assert.AreEqual(begin1, begin2);
         }

         Assert.AreEqual(begin1.HasHandleMax(), begin2.HasHandleMax(), "Expected Begin with matching has handle max values");
         Assert.AreEqual(begin1.HandleMax, begin2.HandleMax, "Handle max values not equal");

         Assert.AreEqual(begin1.HasIncomingWindow(), begin2.HasIncomingWindow(), "Expected Begin with matching has Incoming window values");
         Assert.AreEqual(begin1.IncomingWindow, begin2.IncomingWindow, "Incoming Window values not equal");

         Assert.AreEqual(begin1.HasNextOutgoingId(), begin2.HasNextOutgoingId(), "Expected Begin with matching has Outgoing Id values");
         Assert.AreEqual(begin1.NextOutgoingId, begin2.NextOutgoingId, "Outgoing Id values not equal");

         Assert.AreEqual(begin1.HasOutgoingWindow(), begin2.HasOutgoingWindow(), "Expected Begin with matching has Outgoing window values");
         Assert.AreEqual(begin1.OutgoingWindow, begin2.OutgoingWindow, "Outgoing Window values not equal");

         Assert.AreEqual(begin1.HasRemoteChannel(), begin2.HasRemoteChannel(), "Expected Begin with matching has Remote Channel values");
         Assert.AreEqual(begin1.RemoteChannel, begin2.RemoteChannel, "Remote Channel values not equal");

         Assert.AreEqual(begin1.HasProperties(), begin2.HasProperties(), "Expected Attach with matching has properties values");
         Assert.AreEqual(begin1.Properties, begin2.Properties, "Properties Map values not equal");
         Assert.AreEqual(begin1.HasDesiredCapabilities(), begin2.HasDesiredCapabilities(), "Expected Attach with matching has desired capabilities values");
         Assert.AreEqual(begin1.DesiredCapabilities, begin2.DesiredCapabilities, "Desired Capabilities are not equal");
         Assert.AreEqual(begin2.HasOfferedCapabilities(), begin2.HasOfferedCapabilities(), "Expected Attach with matching has offered capabilities values");
         Assert.AreEqual(begin1.OfferedCapabilities, begin2.OfferedCapabilities, "Offered Capabilities are not equal");
      }

      public static void AssertTypesEqual(ErrorCondition condition1, ErrorCondition condition2)
      {
         if (condition1 == condition2)
         {
            return;
         }
         else if (condition1 == null || condition2 == null)
         {
            Assert.AreEqual(condition1, condition2);
         }

         Assert.AreEqual(condition1.Description, condition2.Description, "Error Descriptions should match");
         Assert.AreEqual(condition1.Condition, condition2.Condition, "Error Condition should match");
         Assert.AreEqual(condition1.Info, condition2.Info, "Error Info should match");
      }

      public static void AssertTypesEqual(Attach attach1, Attach attach2)
      {
         if (attach1 == attach2)
         {
            return;
         }
         else if (attach1 == null || attach2 == null)
         {
            Assert.AreEqual(attach1, attach2);
         }

         Assert.AreEqual(attach1.HasHandle(), attach2.HasHandle(), "Expected Attach with matching has handle values");
         Assert.AreEqual(attach1.Handle, attach2.Handle, "Handle values not equal");

         Assert.AreEqual(attach1.HasInitialDeliveryCount(), attach2.HasInitialDeliveryCount(), "Expected Attach with matching has initial delivery count values");
         Assert.AreEqual(attach1.InitialDeliveryCount, attach2.InitialDeliveryCount, "Initial delivery count values not equal");

         Assert.AreEqual(attach1.HasMaxMessageSize(), attach2.HasMaxMessageSize(), "Expected Attach with matching has max message size values");
         Assert.AreEqual(attach1.MaxMessageSize, attach2.MaxMessageSize, "Max MessageSize values not equal");

         Assert.AreEqual(attach1.HasName(), attach2.HasName(), "Expected Attach with matching has name values");
         Assert.AreEqual(attach1.Name, attach2.Name, "Link Name values not equal");

         Assert.AreEqual(attach1.HasSource(), attach2.HasSource(), "Expected Attach with matching has Source values");
         Assert.AreEqual(attach1.Source, attach2.Source);
         Assert.AreEqual(attach1.HasTarget(), attach2.HasTarget(), "Expected Attach with matching has Target values");
         AssertTypesEqual(attach1.Target, attach2.Target);

         Assert.AreEqual(attach1.HasUnsettled(), attach2.HasUnsettled(), "Expected Attach with matching has handle values");
         Assert.AreEqual(attach1.Unsettled, attach2.Unsettled);

         Assert.AreEqual(attach1.HasReceiverSettleMode(), attach2.HasReceiverSettleMode(), "Expected Attach with matching has receiver settle mode values");
         Assert.AreEqual(attach1.ReceiverSettleMode, attach2.ReceiverSettleMode, "Receiver settle mode values not equal");

         Assert.AreEqual(attach1.HasSenderSettleMode(), attach2.HasSenderSettleMode(), "Expected Attach with matching has sender settle mode values");
         Assert.AreEqual(attach1.SenderSettleMode, attach2.SenderSettleMode, "Sender settle mode values not equal");

         Assert.AreEqual(attach1.HasRole(), attach2.HasRole(), "Expected Attach with matching has Role values");
         Assert.AreEqual(attach1.Role, attach2.Role, "Role values not equal");

         Assert.AreEqual(attach1.HasIncompleteUnsettled(), attach2.HasIncompleteUnsettled(), "Expected Attach with matching has incomplete unsettled values");
         Assert.AreEqual(attach1.IncompleteUnsettled, attach2.IncompleteUnsettled, "Handle values not equal");

         Assert.AreEqual(attach1.HasProperties(), attach2.HasProperties(), "Expected Attach with matching has properties values");
         Assert.AreEqual(attach1.Properties, attach2.Properties, "Properties Map values not equal");
         Assert.AreEqual(attach1.HasDesiredCapabilities(), attach2.HasDesiredCapabilities(), "Expected Attach with matching has desired capabilities values");
         Assert.AreEqual(attach1.DesiredCapabilities, attach2.DesiredCapabilities, "Desired Capabilities are not equal");
         Assert.AreEqual(attach1.HasOfferedCapabilities(), attach2.HasOfferedCapabilities(), "Expected Attach with matching has offered capabilities values");
         Assert.AreEqual(attach1.OfferedCapabilities, attach2.OfferedCapabilities, "Offered Capabilities are not equal");
      }

      public static void AssertTypesEqual(ITerminus terminus1, ITerminus terminus2)
      {
         if (terminus1 == terminus2)
         {
            return;
         }
         else if (terminus1 == null || terminus2 == null)
         {
            Assert.AreEqual(terminus1, terminus2);
         }
         else if (terminus1.GetType().Equals(terminus2.GetType()))
         {
            Assert.Fail("Terminus types are not equal");
         }

         if (terminus1 is Source)
         {
            AssertTypesEqual((Source)terminus1, (Source)terminus2);
         }
         else if (terminus1 is Target)
         {
            AssertTypesEqual((Target)terminus1, (Target)terminus2);
         }
         else if (terminus1 is Coordinator)
         {
            AssertTypesEqual((Coordinator)terminus1, (Coordinator)terminus2);
         }
         else
         {
            Assert.Fail("Terminus types are of unknown origin.");
         }
      }

      public static void AssertTypesEqual(Target target1, Target target2)
      {
         if (target1 == target2)
         {
            return;
         }
         else if (target1 == null || target2 == null)
         {
            Assert.AreEqual(target1, target2);
         }

         Assert.AreEqual(target1.Address, target2.Address, "Address values not equal");
         Assert.AreEqual(target1.Durable, target2.Durable, "TerminusDurability values not equal");
         Assert.AreEqual(target1.ExpiryPolicy, target2.ExpiryPolicy, "TerminusExpiryPolicy values not equal");
         Assert.AreEqual(target1.Timeout, target2.Timeout, "Timeout values not equal");
         Assert.AreEqual(target1.Dynamic, target2.Dynamic, "Dynamic values not equal");
         Assert.AreEqual(target1.DynamicNodeProperties, target2.DynamicNodeProperties, "Dynamic Node Properties values not equal");
         Assert.AreEqual(target1.Capabilities, target2.Capabilities, "Capabilities values not equal");
      }

      public static void AssertTypesEqual(Coordinator coordinator1, Coordinator coordinator2)
      {
         if (coordinator1 == coordinator2)
         {
            return;
         }
         else if (coordinator1 == null || coordinator2 == null)
         {
            Assert.AreEqual(coordinator1, coordinator2);
         }

         Assert.AreEqual(coordinator1.Capabilities, coordinator2.Capabilities, "Capabilities values not equal");
      }

      public static void AssertTypesEqual(Source source1, Source source2)
      {
         if (source1 == source2)
         {
            return;
         }
         else if (source1 == null || source2 == null)
         {
            Assert.AreEqual(source1, source2);
         }

         Assert.AreEqual(source1.Address, source2.Address, "Address values not equal");
         Assert.AreEqual(source1.Durable, source2.Durable, "TerminusDurability values not equal");
         Assert.AreEqual(source1.ExpiryPolicy, source2.ExpiryPolicy, "TerminusExpiryPolicy values not equal");
         Assert.AreEqual(source1.Timeout, source2.Timeout, "Timeout values not equal");
         Assert.AreEqual(source1.Dynamic, source2.Dynamic, "Dynamic values not equal");
         Assert.AreEqual(source1.DynamicNodeProperties, source2.DynamicNodeProperties, "Dynamic Node Properties values not equal");
         Assert.AreEqual(source1.DistributionMode, source2.DistributionMode, "Distribution Mode values not equal");
         Assert.AreEqual(source1.DefaultOutcome, source2.DefaultOutcome, "Filter values not equal");
         Assert.AreEqual(source1.Filter, source2.Filter, "Default outcome values not equal");
         Assert.AreEqual(source1.Outcomes, source2.Outcomes, "Outcomes values not equal");
         Assert.AreEqual(source1.Capabilities, source2.Capabilities, "Capabilities values not equal");
      }

      public static void assertTypesEqual(IDictionary<IProtonBuffer, IDeliveryState> unsettled1,
                                          IDictionary<IProtonBuffer, IDeliveryState> unsettled2)
      {
         if (unsettled1 == null && unsettled2 == null)
         {
            return;
         }
         else if (unsettled1 == null || unsettled2 == null)
         {
            Assert.AreEqual(unsettled1, unsettled2);
         }

         Assert.AreEqual(unsettled1.Count, unsettled2.Count, "Unsettled Map size values are not the same");

         IEnumerator<KeyValuePair<IProtonBuffer, IDeliveryState>> entries1 = unsettled1.GetEnumerator();
         IEnumerator<KeyValuePair<IProtonBuffer, IDeliveryState>> entries2 = unsettled2.GetEnumerator();

         while (entries1.MoveNext() && entries2.MoveNext())
         {
            KeyValuePair<IProtonBuffer, IDeliveryState> entry1 = entries1.Current;
            KeyValuePair<IProtonBuffer, IDeliveryState> entry2 = entries2.Current;

            IProtonBuffer key1 = entry1.Key;
            IProtonBuffer key2 = entry2.Key;

            Assert.AreEqual(key1, key2, "Unsettled map keys don't match.");
            Assert.AreEqual(entry1.Value.GetType(), entry2.Value.GetType(), "Delivery states do not match");
         }
      }
   }
}