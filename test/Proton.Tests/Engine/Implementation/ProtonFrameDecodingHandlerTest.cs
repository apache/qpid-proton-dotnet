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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonFrameDecodingHandlerTest : ProtonEngineTestSupport
   {
      private FrameRecordingTransportHandler testHandler;

      [SetUp]
      public void Reset()
      {
         testHandler = new FrameRecordingTransportHandler();
      }

      [Test]
      public void TestDecodeValidHeaderTriggersHeaderRead()
      {
         IEngine engine = CreateEngine();

         engine.Start();

         // Check for Header processing
         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);
      }

      [Test]
      public void TestReadValidHeaderInSingleByteChunks()
      {
         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'A' }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'M' }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'Q' }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'P' }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }));

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);
      }

      [Test]
      public void TestReadValidHeaderInSplitChunks()
      {
         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P' }));
         engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 0, 0 }));

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);
      }

      [Test]
      public void TestDecodeValidSaslHeaderTriggersHeaderRead()
      {
         IEngine engine = CreateEngine();

         engine.Start();

         // Check for Header processing
         engine.Pipeline.FireRead(AmqpHeader.GetSASLHeader().Buffer);

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetSASLHeader(), header.Body);
      }

      [Test]
      public void TestInvalidHeaderBytesTriggersError()
      {
         ProtonEngine engine = CreateEngine();

         engine.Start();

         try
         {
            engine.Ingest(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'S' }));
            Assert.Fail("Handler should throw error on invalid input");
         }
         catch (Exception)
         {
            // Expected
         }

         // Verify that the parser accepts no new input once in error state.
         try
         {
            engine.Pipeline.FireRead(AmqpHeader.GetSASLHeader().Buffer);
            Assert.Fail("Handler should throw error on additional input");
         }
         catch (Exception)
         {
            // Expected
         }
      }

      [Test]
      public void TestDecodeEmptyOpenEncodedFrame()
      {
         // Frame data for: Open
         //   Open{ containerId="", hostname='null', maxFrameSize=4294967295, channelMax=65535,
         //         idleTimeOut=null, outgoingLocales=null, incomingLocales=null, offeredCapabilities=null,
         //         desiredCapabilities=null, properties=null}
         byte[] emptyOpen = new byte[] { 0, 0, 0, 16, 2, 0, 0, 0, 0, 83, 16, 192, 3, 1, 161, 0 };

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);
         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(emptyOpen));

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);

         frame = testHandler.AmqpFramesRead[0];
         Assert.IsTrue(frame is IncomingAmqpEnvelope);
         IncomingAmqpEnvelope envelope = (IncomingAmqpEnvelope)frame;
         Open decoded = (Open)envelope.Body;

         Assert.IsTrue(decoded.HasContainerId());  // Defaults to empty string from proton-j
         Assert.IsFalse(decoded.HasHostname());
         Assert.IsFalse(decoded.HasMaxFrameSize());
         Assert.IsFalse(decoded.HasChannelMax());
         Assert.IsFalse(decoded.HasIdleTimeout());
         Assert.IsFalse(decoded.HasOutgoingLocales());
         Assert.IsFalse(decoded.HasIncomingLocales());
         Assert.IsFalse(decoded.HasOfferedCapabilities());
         Assert.IsFalse(decoded.HasDesiredCapabilities());
         Assert.IsFalse(decoded.HasProperties());
      }

      [Test]
      public void TestDecodeSimpleOpenEncodedFrame()
      {
         // Frame data for: Open
         //   Open{ containerId='container', hostname='localhost', maxFrameSize=16384, channelMax=65535,
         //         idleTimeOut=30000, outgoingLocales=null, incomingLocales=null, offeredCapabilities=null,
         //         desiredCapabilities=null, properties=null}
         byte[] basicOpen = new byte[] {0, 0, 0, 49, 2, 0, 0, 0, 0, 83, 16, 192, 36, 5, 161, 9, 99, 111,
                                             110, 116, 97, 105, 110, 101, 114, 161, 9, 108, 111, 99, 97, 108,
                                             104, 111, 115, 116, 112, 0, 0, 64, 0, 96, 255, 255, 112, 0, 0, 117, 48};

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);
         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(basicOpen));

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);

         frame = testHandler.AmqpFramesRead[0];
         Assert.IsTrue(frame is IncomingAmqpEnvelope);
         IncomingAmqpEnvelope envelope = (IncomingAmqpEnvelope)frame;
         Open decoded = (Open)envelope.Body;

         Assert.IsTrue(decoded.HasContainerId());
         Assert.AreEqual("container", decoded.ContainerId);
         Assert.IsTrue(decoded.HasHostname());
         Assert.AreEqual("localhost", decoded.Hostname);
         Assert.IsTrue(decoded.HasMaxFrameSize());
         Assert.AreEqual(16384, decoded.MaxFrameSize);
         Assert.IsTrue(decoded.HasChannelMax());
         Assert.IsTrue(decoded.HasIdleTimeout());
         Assert.AreEqual(30000, decoded.IdleTimeout);
         Assert.IsFalse(decoded.HasOutgoingLocales());
         Assert.IsFalse(decoded.HasIncomingLocales());
         Assert.IsFalse(decoded.HasOfferedCapabilities());
         Assert.IsFalse(decoded.HasDesiredCapabilities());
         Assert.IsFalse(decoded.HasProperties());
      }

      [Test]
      public void TestDecodePipelinedHeaderAndOpenEncodedFrame()
      {
         // Frame data for: Open
         //   Open{ containerId='container', hostname='localhost', maxFrameSize=16384, channelMax=65535,
         //         idleTimeOut=30000, outgoingLocales=null, incomingLocales=null, offeredCapabilities=null,
         //         desiredCapabilities=null, properties=null}
         byte[] basicOpen = new byte[] {(byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0, // HEADER
                                       0, 0, 0, 49, 2, 0, 0, 0, 0, 83, 16, 192, 36, 5, 161, 9, 99, 111,
                                       110, 116, 97, 105, 110, 101, 114, 161, 9, 108, 111, 99, 97, 108,
                                       104, 111, 115, 116, 112, 0, 0, 64, 0, 96, 255, 255, 112, 0, 0, 117, 48};

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(basicOpen));

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);

         frame = testHandler.AmqpFramesRead[0];
         Assert.IsTrue(frame is IncomingAmqpEnvelope);
         IncomingAmqpEnvelope envelope = (IncomingAmqpEnvelope)frame;
         Open decoded = (Open)envelope.Body;

         Assert.IsTrue(decoded.HasContainerId());
         Assert.AreEqual("container", decoded.ContainerId);
         Assert.IsTrue(decoded.HasHostname());
         Assert.AreEqual("localhost", decoded.Hostname);
         Assert.IsTrue(decoded.HasMaxFrameSize());
         Assert.AreEqual(16384, decoded.MaxFrameSize);
         Assert.IsTrue(decoded.HasChannelMax());
         Assert.IsTrue(decoded.HasIdleTimeout());
         Assert.AreEqual(30000, decoded.IdleTimeout);
         Assert.IsFalse(decoded.HasOutgoingLocales());
         Assert.IsFalse(decoded.HasIncomingLocales());
         Assert.IsFalse(decoded.HasOfferedCapabilities());
         Assert.IsFalse(decoded.HasDesiredCapabilities());
         Assert.IsFalse(decoded.HasProperties());
      }

      [Test]
      public void TestDecodeEmptyFrame()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: '8byte sized' empty AMQP frame
         byte[] emptyFrame = new byte[] { (byte)0x00, 0x00, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00 };

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);

         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(emptyFrame));

         frame = testHandler.AmqpFramesRead[0];
         Assert.IsTrue(frame is IncomingAmqpEnvelope);
         IncomingAmqpEnvelope envelope = (IncomingAmqpEnvelope)frame;
         Assert.IsNull(envelope.Body);
      }

      [Test]
      public void TestDecodeMultipleEmptyFrames()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: 2x '8byte sized' empty AMQP frames
         byte[] emptyFrames = new byte[] { (byte) 0x00, 0x00, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00,
                                           (byte) 0x00, 0x00, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00 };

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         object frame = testHandler.HeadersRead[0];
         Assert.IsTrue(frame is HeaderEnvelope);
         HeaderEnvelope header = (HeaderEnvelope)frame;
         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header.Body);

         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(emptyFrames));

         frame = testHandler.AmqpFramesRead[0];
         Assert.IsTrue(frame is IncomingAmqpEnvelope);
         IncomingAmqpEnvelope envelope = (IncomingAmqpEnvelope)frame;
         Assert.IsNull(envelope.Body);

         frame = testHandler.AmqpFramesRead[1];
         Assert.IsTrue(frame is IncomingAmqpEnvelope);
         envelope = (IncomingAmqpEnvelope)frame;
         Assert.IsNull(envelope.Body);
      }

      /*
       * Test that frames indicating they are under 8 bytes (the minimum size of the frame header) causes an error.
       */
      [Test]
      public void TestInputOfFrameWithInvalidSizeBelowMinimumPossible()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: '7byte sized' AMQP frame header
         byte[] undersizedFrameHeader = new byte[] { (byte)0x00, 0x00, 0x00, 0x07, 0x02, 0x00, 0x00, 0x00 };

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         try
         {
            engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(undersizedFrameHeader));
            Assert.Fail("Should indicate protocol has been violated.");
         }
         catch (ProtocolViolationException pve)
         {
            // Expected
            Assert.IsTrue(pve.Message.Contains("frame size 7 smaller than minimum"));
         }
      }

      /*
       * Test that frames indicating a DOFF under 8 bytes (the minimum size of the frame header) causes an error.
       */
      [Test]
      public void TestInputOfFrameWithInvalidDoffBelowMinimumPossible()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: '8byte sized' AMQP frame header with invalid doff of 1[*4 = 4bytes]
         byte[] underMinDoffFrameHeader = new byte[] { (byte)0x00, 0x00, 0x00, 0x08, 0x01, 0x00, 0x00, 0x00 };

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         try
         {
            engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(underMinDoffFrameHeader));
            Assert.Fail("Should indicate protocol has been violated.");
         }
         catch (ProtocolViolationException pve)
         {
            // Expected
            Assert.IsTrue(pve.Message.Contains("data offset 4 smaller than minimum"));
         }
      }

      /*
       * Test that frames indicating a DOFF larger than the frame size cause expected error.
       */
      [Test]
      public void TestInputOfFrameWithInvalidDoffAboveMaximumPossible()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: '8byte sized' AMQP frame header with invalid doff of 3[*4 = 12bytes]
         byte[] overFrameSizeDoffFrameHeader = new byte[] { (byte)0x00, 0x00, 0x00, 0x08, 0x03, 0x00, 0x00, 0x00 };

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         try
         {
            engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(overFrameSizeDoffFrameHeader));
            Assert.Fail("Should indicate protocol has been violated.");
         }
         catch (ProtocolViolationException pve)
         {
            // Expected
            Assert.IsTrue(pve.Message.Contains("data offset 12 larger than the frame size 8"));
         }
      }

      /*
       * Test that frame size above limit triggers error before attempting to decode the frame
       */
      [Test]
      public void TestFrameSizeThatExceedsMaximumFrameSizeLimitTriggersError()
      {
         byte[] overFrameSizeLimitFrameHeader = new byte[] { (byte)0xA0, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         try
         {
            engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(overFrameSizeLimitFrameHeader));
            Assert.Fail("Should indicate protocol has been violated.");
         }
         catch (ProtocolViolationException pve)
         {
            // Expected 2684354560 frame size is to big
            Assert.IsTrue(pve.Message.Contains("2684354560"));
            Assert.IsTrue(pve.Message.Contains("larger than maximum frame size"));
         }
      }

      [Test]
      public void TestReadSplitTransferFrames()
      {
         string frame1 = "00-00-01-1E-02-00-00-00-00-53-14-C0-09-08-43-43-40-40-42-41-40-40-00-53-75-B0-00-00-01-00-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-02-" +
                         "00-00-01-1E-02-00-00-00-00-53-14-C0-09-08-43-43-40-40-42-41-40-40-00-53-75-B0-00-00-01-00-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-03-" +
                         "00-00-01-1E-02-00-00-00-00-53-14-C0-09-08-43-43-40-40-42-41-40-40-00-53-75-B0-00-00-01-00-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-04-" +
                         "00-00-01-1E-02-00-00-00-00-53-14-C0-09-08-43-43-40-40-42-41-40-40-00-53-75-B0-00-00-01-00-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05";
         string frame2 = "05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-05-" +
                         "00-00-01-1E-02-00-00-00-00-53-14-C0-09-08-43-43-40-40-42-41-40-40-00-53-75-B0-00-00-01-00-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06-06";
         string frame3 = "00-00-01-1E-02-00-00-00-00-53-14-C0-09-08-43-43-40-40-42-41-40-40-00-53-75-B0-00-00-01-00-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07-07";

         ProtonEngine engine = CreateEngine();

         engine.Start();

         engine.Pipeline.FireRead(AmqpHeader.GetAMQPHeader().Buffer);

         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(StringToByteArray(frame1)));
         Assert.AreEqual(3, testHandler.AmqpFramesRead.Count);

         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(StringToByteArray(frame2)));
         Assert.AreEqual(5, testHandler.AmqpFramesRead.Count);

         engine.Pipeline.FireRead(ProtonByteBufferAllocator.Instance.Wrap(StringToByteArray(frame3)));
         Assert.AreEqual(6, testHandler.AmqpFramesRead.Count);
      }

      private ProtonEngine CreateEngine()
      {
         ProtonEngine engine = new ProtonEngine();

         engine.Pipeline.AddLast("read-sink", new FrameReadSinkTransportHandler());
         engine.Pipeline.AddLast("test", testHandler);
         engine.Pipeline.AddLast("frames", new ProtonFrameDecodingHandler());
         engine.Pipeline.AddLast("write-sink", new FrameWriteSinkTransportHandler());

         return engine;
      }

      public static byte[] StringToByteArray(String hex)
      {
         hex = hex.Replace("-", "");
         int NumberChars = hex.Length;
         byte[] bytes = new byte[NumberChars / 2];
         for (int i = 0; i < NumberChars; i += 2)
         {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
         }
         return bytes;
      }
   }
}