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

         foreach(Symbol sym in input)
         {
            codec.PutSymbol(sym);
         }

         codec.Exit();

         MemoryStream stream = new MemoryStream((int)codec.EncodedSize);
         long encodingSize = codec.Encode(stream);
         Assert.IsTrue(encodingSize > 0);

         codec = CodecFactory.Create();

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

         Assert.AreEqual(expectedRead, codec.Decode(new BinaryReader(encoded)));

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

         Assert.AreEqual(expectedRead, codec.Decode(new BinaryReader(encoded)));

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

         IDescribedType decoded = DecodeProtonPerformative(stream);
         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded is Attach);

         Attach performative = (Attach)decoded;

         Assert.AreEqual(1, performative.Handle);
         Assert.AreEqual("test", performative.Name);
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
               encodingSize = codec.Encode(new BinaryWriter(stream));
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