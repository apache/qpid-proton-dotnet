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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture]
   public class frameHandlerTests
   {
      [Test]
      public void TestDecodeMultipleEmptyFrames()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: 2x '8byte sized' empty AMQP frames
         byte[] emptyFrames = new byte[] { 0x00, 0x00, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00,
                                           0x00, 0x00, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00 };

         MemoryStream stream = new MemoryStream(emptyFrames);
         TestFrameHandler handler = new TestFrameHandler();
         FrameDecoder decoder = new FrameDecoder(handler);

         decoder.Ingest(new MemoryStream(AMQPHeader.Header.Buffer));

         Assert.AreEqual(1, handler.HeaderCount);

         do
         {
            Assert.DoesNotThrow(() => decoder.Ingest(stream));
         }
         while (stream.Position < stream.Length);

         Assert.AreEqual(2, handler.EmptyFrameCount);
      }

      [Test]
      public void TestInputOfFrameWithInvalidSizeBelowMinimumPossible()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: '7byte sized' AMQP frame header
         byte[] undersizedFrameHeader = new byte[] { (byte)0x00, 0x00, 0x00, 0x07, 0x02, 0x00, 0x00, 0x00 };

         MemoryStream stream = new MemoryStream(undersizedFrameHeader);
         TestFrameHandler handler = new TestFrameHandler();
         FrameDecoder decoder = new FrameDecoder(handler);

         decoder.Ingest(new MemoryStream(AMQPHeader.Header.Buffer));

         Assert.AreEqual(1, handler.HeaderCount);

         Assert.Throws<AssertionError>(() => decoder.Ingest(stream));
      }

      [Test]
      public void TestInputOfFrameWithInvalidDoffBelowMinimumPossible()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: '8byte sized' AMQP frame header with invalid doff of 1[*4 = 4bytes]
         byte[] underMinDoffFrameHeader = new byte[] { (byte)0x00, 0x00, 0x00, 0x08, 0x01, 0x00, 0x00, 0x00 };

         MemoryStream stream = new MemoryStream(underMinDoffFrameHeader);
         TestFrameHandler handler = new TestFrameHandler();
         FrameDecoder decoder = new FrameDecoder(handler);

         decoder.Ingest(new MemoryStream(AMQPHeader.Header.Buffer));

         Assert.AreEqual(1, handler.HeaderCount);

         Assert.Throws<AssertionError>(() => decoder.Ingest(stream));
      }

      [Test]
      public void TestInputOfFrameWithInvalidDoffAboveMaximumPossible()
      {
         // http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-transport-v1.0-os.html#doc-idp124752
         // Description: '8byte sized' AMQP frame header with invalid doff of 3[*4 = 12bytes]
         byte[] overFrameSizeDoffFrameHeader = new byte[] { (byte)0x00, 0x00, 0x00, 0x08, 0x03, 0x00, 0x00, 0x00 };

         MemoryStream stream = new MemoryStream(overFrameSizeDoffFrameHeader);
         TestFrameHandler handler = new TestFrameHandler();
         FrameDecoder decoder = new FrameDecoder(handler);

         decoder.Ingest(new MemoryStream(AMQPHeader.Header.Buffer));

         Assert.AreEqual(1, handler.HeaderCount);

         Assert.Throws<AssertionError>(() => decoder.Ingest(stream));
      }

      [Test]
      public void TestFrameSizeThatExceedsMaximumFrameSizeLimitTriggersError()
      {
         byte[] overFrameSizeLimitFrameHeader = new byte[] { (byte)0xA0, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };

         MemoryStream stream = new MemoryStream(overFrameSizeLimitFrameHeader);
         TestFrameHandler handler = new TestFrameHandler();
         handler.InboundMaxFrameSize = 16384;

         FrameDecoder decoder = new FrameDecoder(handler);

         decoder.Ingest(new MemoryStream(AMQPHeader.Header.Buffer));

         Assert.AreEqual(1, handler.HeaderCount);

         Assert.Throws<AssertionError>(() => decoder.Ingest(stream));
      }

      private class TestFrameHandler : IFrameHandler
      {
         private uint inboundMaxFrameSize = int.MaxValue;

         private IList<AMQPHeader> headers = new List<AMQPHeader>();
         private IList<PerformativeDescribedType> performatives = new List<PerformativeDescribedType>();
         private IList<SaslDescribedType> saslPerformatives = new List<SaslDescribedType>();

         private uint emptyFrameCount;

         public uint InboundMaxFrameSize
         {
            get => inboundMaxFrameSize;
            set => inboundMaxFrameSize = value;
         }

         public uint HeaderCount => (uint)headers.Count;

         public uint EmptyFrameCount => emptyFrameCount;

         public uint PerformativeCount => (uint)performatives.Count;

         public uint SaslPerformativeCount => (uint)saslPerformatives.Count;

         public void HandleHeader(AMQPHeader header)
         {
            headers.Add(header);
         }

         public void HandleHeartbeat(uint frameSize, ushort channel)
         {
            emptyFrameCount++;
         }

         public void HandlePerformative(uint frameSize, PerformativeDescribedType amqp, ushort channel, byte[] payload)
         {
            performatives.Add(amqp);
         }

         public void HandleSaslPerformative(uint frameSize, SaslDescribedType sasl, ushort channel, byte[] payload)
         {
            saslPerformatives.Add(sasl);
         }
      }
   }
}