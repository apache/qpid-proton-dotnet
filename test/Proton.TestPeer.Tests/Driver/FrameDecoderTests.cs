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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture, Timeout(20000)]
   public class FrameHandlerTests
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

         MemoryStream stream1 = new MemoryStream(StringToByteArray(frame1));
         MemoryStream stream2 = new MemoryStream(StringToByteArray(frame2));
         MemoryStream stream3 = new MemoryStream(StringToByteArray(frame3));

         TestFrameHandler handler = new TestFrameHandler();
         handler.InboundMaxFrameSize = 16384;

         FrameDecoder decoder = new FrameDecoder(handler);

         decoder.Ingest(new MemoryStream(AMQPHeader.Header.Buffer));

         decoder.Ingest(stream1);
         Assert.AreEqual(1, handler.PerformativeCount);
         Assert.IsTrue(stream1.Position < stream1.Length);
         decoder.Ingest(stream1);
         Assert.AreEqual(2, handler.PerformativeCount);
         Assert.IsTrue(stream1.Position < stream1.Length);
         decoder.Ingest(stream1);
         Assert.AreEqual(3, handler.PerformativeCount);
         Assert.IsTrue(stream1.Position < stream1.Length);
         decoder.Ingest(stream1);
         Assert.AreEqual(3, handler.PerformativeCount);
         Assert.IsTrue(stream1.Position == stream1.Length);

         decoder.Ingest(stream2);
         Assert.AreEqual(4, handler.PerformativeCount);
         Assert.IsTrue(stream2.Position < stream2.Length);
         decoder.Ingest(stream2);
         Assert.AreEqual(5, handler.PerformativeCount);
         Assert.IsTrue(stream2.Position == stream2.Length);

         decoder.Ingest(stream3);
         Assert.AreEqual(6, handler.PerformativeCount);
         Assert.IsTrue(stream3.Position == stream3.Length);
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

         public string Name => "TestFrameHandler";

         public ILoggerFactory LoggerFactory => new NullLoggerFactory();

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