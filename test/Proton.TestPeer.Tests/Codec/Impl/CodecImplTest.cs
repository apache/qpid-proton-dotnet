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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   [TestFixture]
   public class CodecTest
   {
      private ICodec codec = CodecFactory.Create();

      [Test]
      public void TestEncodeAndDecodeString()
      {
         String input = "test";

         ICodec codec = CodecFactory.Create();
         codec.PutString(input);

         MemoryStream stream = new MemoryStream((int)codec.EncodedSize);
         long encodingSize = codec.Encode(stream);
         Assert.IsTrue(encodingSize > input.Length);

         codec.Clear();

         stream.Seek(0, SeekOrigin.Begin);
         long decodedSize = codec.Decode(stream);
         Assert.AreEqual(encodingSize, decodedSize);

         string output = codec.GetString();

         Assert.AreEqual(input, output);
      }

      [Test]
      public void TestEncodeAndDecodeUInt32()
      {
         uint input = uint.MaxValue;

         ICodec codec = CodecFactory.Create();
         codec.PutUnsignedInteger(input);

         MemoryStream stream = new MemoryStream((int)codec.EncodedSize);
         long encodingSize = codec.Encode(stream);
         Assert.IsTrue(encodingSize > sizeof(uint));

         codec.Clear();

         stream.Seek(0, SeekOrigin.Begin);
         long decodedSize = codec.Decode(stream);
         Assert.AreEqual(encodingSize, decodedSize);

         uint output = codec.GetUnsignedInteger();

         Assert.AreEqual(input, output);
      }

      [Test]
      public void TestEncodeAndDecodeSymbolArray()
      {
         Symbol[] input = new Symbol[] { new Symbol("one"), new Symbol("two") };

         ICodec codec = CodecFactory.Create();
         codec.PutArray(false, DataType.Symbol);
         codec.Enter();

         foreach (Symbol sym in input)
         {
            codec.PutSymbol(sym);
         }

         codec.Exit();

         MemoryStream stream = new MemoryStream((int)codec.EncodedSize);
         long encodingSize = codec.Encode(stream);
         Assert.IsTrue(encodingSize > 0);

         codec = CodecFactory.Create();

         stream.Seek(0, SeekOrigin.Begin);
         long decodedSize = codec.Decode(stream);
         Assert.AreEqual(encodingSize, decodedSize);

         Assert.AreEqual(input.Length, codec.GetArray());
         Assert.AreEqual(DataType.Symbol, codec.GetArrayType());

         Symbol[] output = (Symbol[])codec.GetPrimitiveArray();

         for (int i = 0; i < input.Length; ++i)
         {
            Assert.AreEqual(input[i], output[i]);
         }
      }

      [Test]
      public void TestDecodeOpen()
      {
         Open open = new Open();
         open.ContainerId = "test";
         open.Hostname = "localhost";

         long expectedRead;
         Stream encoded = EncodeProtonPerformative(open, out expectedRead);
         Assert.AreNotEqual(0, expectedRead);

         ICodec codec = CodecFactory.Create();

         Assert.AreEqual(expectedRead, codec.Decode(encoded));

         Open described = (Open)codec.GetDescribedType();
         Assert.IsNotNull(described);
         Assert.AreEqual(Open.DESCRIPTOR_SYMBOL, described.Descriptor);

         Assert.AreEqual(open.ContainerId, described.ContainerId);
         Assert.AreEqual(open.Hostname, described.Hostname);
      }

      [Test]
      public void TestDecodeOpenWithLocales()
      {
         Open open = new Open();
         open.ContainerId = "test";
         open.Hostname = "localhost";
         open.IncomingLocales = new Symbol[] { new Symbol("A"), new Symbol("B") };

         long expectedRead;
         Stream encoded = EncodeProtonPerformative(open, out expectedRead);
         Assert.AreNotEqual(0, expectedRead);

         ICodec codec = CodecFactory.Create();

         Assert.AreEqual(expectedRead, codec.Decode(encoded));

         Open described = (Open)codec.GetDescribedType();
         Assert.IsNotNull(described);
         Assert.AreEqual(Open.DESCRIPTOR_SYMBOL, described.Descriptor);

         Assert.AreEqual(open.ContainerId, described.ContainerId);
         Assert.AreEqual(open.Hostname, described.Hostname);
         Assert.IsNotNull(described.IncomingLocales);

         for (int i = 0; i < open.IncomingLocales.Length; ++i)
         {
            Assert.AreEqual(open.IncomingLocales[i], described.IncomingLocales[i]);
         }
      }

      [Test]
      public void TestEncodeOpen()
      {
         Open open = new Open();
         open.ContainerId = "test";
         open.Hostname = "localhost";

         ICodec codec = CodecFactory.Create();

         codec.PutDescribedType(open);
         MemoryStream stream = new MemoryStream((int)codec.EncodedSize);
         codec.Encode(stream);
         stream.Seek(0, SeekOrigin.Begin);

         IDescribedType decoded = DecodeProtonPerformative(stream);
         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded is Open);

         Open performative = (Open)decoded;
         Assert.AreEqual(open.ContainerId, performative.ContainerId);
         Assert.AreEqual(open.Hostname, performative.Hostname);
      }

      [Test]
      public void TestDecodeBegin()
      {
         Begin begin = new Begin();
         begin.HandleMax = 512;
         begin.RemoteChannel = 1;

         long expectedRead;
         Stream encoded = EncodeProtonPerformative(begin, out expectedRead);

         ICodec codec = CodecFactory.Create();

         Assert.AreEqual(expectedRead, codec.Decode(encoded));

         Begin described = (Begin)codec.GetDescribedType();
         Assert.IsNotNull(described);
         Assert.AreEqual(Begin.DESCRIPTOR_SYMBOL, described.Descriptor);

         Assert.AreEqual(1, described.RemoteChannel);
         Assert.AreEqual(512, described.HandleMax);
      }

      [Test]
      public void TestDecodeAttach()
      {
         Attach attach = new Attach();
         attach.Name = "test";
         attach.Handle = 1;
         attach.Role = Role.Sender;
         attach.SenderSettleMode = SenderSettleMode.Mixed;
         attach.ReceiverSettleMode = ReceiverSettleMode.First;
         attach.Source = new Source();
         attach.Target = new Target();

         long expectedRead;
         Stream encoded = EncodeProtonPerformative(attach, out expectedRead);

         ICodec codec = CodecFactory.Create();

         Assert.AreEqual(expectedRead, codec.Decode(encoded));

         Attach described = (Attach)codec.GetDescribedType();
         Assert.IsNotNull(described);
         Assert.AreEqual(Attach.DESCRIPTOR_SYMBOL, described.Descriptor);

         Assert.AreEqual(1, described.Handle);
         Assert.AreEqual("test", described.Name);
      }

      [Test]
      public void TestEncodeAttach()
      {
         Attach attach = new Attach();
         attach.Name = "test";
         attach.Handle = 1;
         attach.Role = Role.Sender;
         attach.SenderSettleMode = SenderSettleMode.Mixed;
         attach.ReceiverSettleMode = ReceiverSettleMode.First;
         attach.Source = new Source();
         attach.Target = new Target();

         ICodec codec = CodecFactory.Create();

         codec.PutDescribedType(attach);
         MemoryStream stream = new MemoryStream((int)codec.EncodedSize);
         codec.Encode(stream);
         stream.Seek(0, SeekOrigin.Begin);

         IDescribedType decoded = DecodeProtonPerformative(stream);
         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded is Attach);

         Attach performative = (Attach)decoded;

         Assert.AreEqual(1, performative.Handle);
         Assert.AreEqual("test", performative.Name);
      }

      [Test]
      public void TestDecodeOpenFromBytes()
      {
         // Encoded data for: Open
         //   Open{ containerId='container', hostname='localhost', maxFrameSize=16384, channelMax=65535,
         //         idleTimeOut=30000, outgoingLocales=null, incomingLocales=null, offeredCapabilities=null,
         //         desiredCapabilities=null, properties=null}
         byte[] basicOpen = new byte[] {0, 83, 16, 192, 36, 5, 161, 9, 99, 111,
                                        110, 116, 97, 105, 110, 101, 114, 161, 9, 108, 111, 99, 97, 108,
                                        104, 111, 115, 116, 112, 0, 0, 64, 0, 96, 255, 255, 112, 0, 0, 117, 48};

         MemoryStream stream = new MemoryStream(basicOpen);
         IDescribedType decoded = DecodeProtonPerformative(stream);
         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded is Open);

         Open performative = (Open)decoded;

         Assert.AreEqual("container", performative.ContainerId);
         Assert.AreEqual("localhost", performative.Hostname);
         Assert.AreEqual(16384u, performative.MaxFrameSize);
         Assert.AreEqual(30000u, performative.IdleTimeout);
         Assert.AreEqual(65535u, performative.ChannelMax);
      }

      [Test]
      public void TestComplexOpenPerformative()
      {
         // Frame data for: Open
         //   Open{ containerId='container', hostname='localhost', maxFrameSize=16384, channelMax=65535,
         //         idleTimeOut=36000, outgoingLocales=null, incomingLocales=null, offeredCapabilities=[SOMETHING],
         //         desiredCapabilities=[ANONYMOUS-RELAY, DELAYED-DELIVERY], properties={queue-prefix=queue://}}
         byte[] completeOpen = new byte[] {0, 83, 16, 192, 116, 10, 161, 9, 99, 111,
                                           110, 116, 97, 105, 110, 101, 114, 161, 9, 108, 111, 99, 97, 108, 104,
                                           111, 115, 116, 112, 0, 0, 64, 0, 96, 255, 255, 112, 0, 0, 140, 160,
                                           64, 64, 224, 12, 1, 163, 9, 83, 79, 77, 69, 84, 72, 73, 78, 71, 224,
                                           35, 2, 163, 15, 65, 78, 79, 78, 89, 77, 79, 85, 83, 45, 82, 69, 76,
                                           65, 89, 16, 68, 69, 76, 65, 89, 69, 68, 45, 68, 69, 76, 73, 86, 69,
                                           82, 89, 193, 25, 2, 163, 12, 113, 117, 101, 117, 101, 45, 112, 114,
                                           101, 102, 105, 120, 161, 8, 113, 117, 101, 117, 101, 58, 47, 47};

         MemoryStream stream = new MemoryStream(completeOpen);
         IDescribedType decoded = DecodeProtonPerformative(stream);
         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded is Open);

         Open performative = (Open)decoded;

         Assert.AreEqual("container", performative.ContainerId);
         Assert.AreEqual("localhost", performative.Hostname);
         Assert.AreEqual(16384u, performative.MaxFrameSize);
         Assert.AreEqual(36000u, performative.IdleTimeout);
         Assert.AreEqual(65535u, performative.ChannelMax);
         Assert.AreEqual(new Symbol[] { new Symbol("SOMETHING") }, performative.OfferedCapabilities);
      }

      [Test]
      public void TestEncodeAndDecodeHeader()
      {
         Header header = new Header();
         header.Durable = true;
         header.Ttl = 123;

         long expectedRead;
         Stream encoded = EncodeProtonPerformative(header, out expectedRead);
         Assert.AreNotEqual(0, expectedRead);

         ICodec codec = CodecFactory.Create();

         Assert.AreEqual(expectedRead, codec.Decode(encoded));

         Header described = (Header)codec.GetDescribedType();
         Assert.IsNotNull(described);
         Assert.AreEqual(Header.DESCRIPTOR_SYMBOL, described.Descriptor);

         Assert.AreEqual(header.Ttl, described.Ttl);
         Assert.AreEqual(header.Durable, described.Durable);
      }

      [Test]
      public void TestEncodeAndDecodeProperties()
      {
         Properties properties = new Properties();
         properties.AbsoluteExpiryTime = 123;
         properties.CreationTime = long.MaxValue;

         long expectedRead;
         Stream encoded = EncodeProtonPerformative(properties, out expectedRead);
         Assert.AreNotEqual(0, expectedRead);

         ICodec codec = CodecFactory.Create();

         Assert.AreEqual(expectedRead, codec.Decode(encoded));

         Properties described = (Properties)codec.GetDescribedType();
         Assert.IsNotNull(described);
         Assert.AreEqual(Properties.DESCRIPTOR_SYMBOL, described.Descriptor);

         Assert.AreEqual(properties.CreationTime, described.CreationTime);
         Assert.AreEqual(properties.AbsoluteExpiryTime, described.AbsoluteExpiryTime);
      }

      [Test]
      public void TestDecodeExternallyEncodedProperties()
      {
         // Frame data for: Properties
         byte[] encodedProperties = new byte[] {0, 83, 115, 208, 0, 0, 0, 131, 0, 0, 0, 13, 161,
                                               8, 73, 68, 58, 49, 50, 51, 52, 53, 160, 4, 117, 115,
                                               101, 114, 161, 14, 116, 104, 101, 45, 109, 97, 110, 97,
                                               103, 101, 109, 101, 110, 116, 161, 4, 97, 109, 113, 112,
                                               161, 11, 116, 104, 101, 45, 109, 105, 110, 105, 111, 110,
                                               115, 161, 3, 97, 98, 99, 163, 4, 103, 122, 105, 112, 161,
                                               16, 97, 112, 112, 108, 105, 99, 97, 116, 105, 111, 110,
                                               47, 106, 115, 111, 110, 131, 0, 0, 0, 0, 0, 0, 0, 123,
                                               131, 0, 0, 0, 0, 0, 0, 0, 1, 161, 11, 100, 105, 115, 103,
                                               114, 117, 110, 116, 108, 101, 100, 112, 0, 0, 32, 0, 161,
                                               9, 47, 100, 101, 118, 47, 110, 117, 108, 108, 0, 83, 119,
                                               161, 11, 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100};

         MemoryStream stream = new MemoryStream(encodedProperties);
         IDescribedType decoded = DecodeProtonPerformative(stream);
         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded is Properties);

         Properties properties = (Properties)decoded;

         Assert.IsTrue(properties.CreationTime.HasValue);
         Assert.AreEqual(123, properties.AbsoluteExpiryTime);
         Assert.AreEqual(1, properties.CreationTime);
      }

      private IDescribedType DecodeProtonPerformative(MemoryStream stream)
      {
         IDescribedType performative = null;

         try
         {
            codec.Decode(stream);
         }
         catch (Exception e)
         {
            throw new AssertionException("Decoder failed reading remote input:", e);
         }

         DataType dataType = codec.DataType;
         if (dataType != DataType.Described)
         {
            throw new ArgumentException(
                "Decoded type expected to be " + DataType.Described + " but was: " + dataType);
         }

         try
         {
            performative = codec.GetDescribedType();
         }
         finally
         {
            codec.Clear();
         }

         return performative;
      }

      private Stream EncodeProtonPerformative(IDescribedType performative, out long encodingSize)
      {
         MemoryStream stream = new MemoryStream();
         encodingSize = 0;

         if (performative != null)
         {
            try
            {
               codec.PutDescribedType(performative);
               encodingSize = codec.Encode(stream);
            }
            finally
            {
               codec.Clear();
            }
         }

         stream.Position = 0;  // Reset stream to beginning for decode.

         return encodingSize > 0 ? stream : null;
      }
   }
}