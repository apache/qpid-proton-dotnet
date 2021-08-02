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
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class AmqpHeaderTest
   {
      [Test]
      public void TestDefaultCreate()
      {
         AmqpHeader header = new AmqpHeader();

         Assert.AreEqual(AmqpHeader.GetAMQPHeader(), header);
         Assert.IsFalse(header.IsSaslHeader());
         Assert.AreEqual(0, header.ProtocolId);
         Assert.AreEqual(1, header.Major);
         Assert.AreEqual(0, header.Minor);
         Assert.AreEqual(0, header.Revision);
         Assert.IsTrue(header.HasValidPrefix());
      }

      [Test]
      public void TestToArray()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });
         AmqpHeader header = new AmqpHeader(buffer);
         byte[] array = header.ToArray();

         buffer.ReadOffset = 0;

         Assert.AreEqual(buffer, ProtonByteBufferAllocator.Instance.Wrap(array));
      }

      [Test]
      public void TestToByteBuffer()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });
         AmqpHeader header = new AmqpHeader(buffer);
         IProtonBuffer byteBuffer = header.Buffer;

         buffer.ReadOffset = 0;

         Assert.AreEqual(buffer, byteBuffer);
      }

      [Test]
      public void TestCreateFromBufferWithoutValidation()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 4, 1, 0, 0 });
         AmqpHeader invalid = new AmqpHeader(buffer, false);

         Assert.AreEqual(4, invalid.GetByteAt(4));
         Assert.AreEqual(4, invalid.ProtocolId);
      }

      [Test]
      public void TestCreateFromBufferWithoutValidationFailsWithToLargeInAdd()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 4, 1, 0, 0, 0 });
         Assert.Throws<IndexOutOfRangeException>(() => new AmqpHeader(buffer, false));
      }

      [Test]
      public void TestGetBuffer()
      {
         AmqpHeader header = new AmqpHeader();

         Assert.IsNotNull(header.Buffer);
         IProtonBuffer buffer = header.Buffer;

         buffer[0] = (byte)'B';

         Assert.AreEqual('A', header.GetByteAt(0));
      }

      [Test]
      public void TestHashCode()
      {
         AmqpHeader defaultCtor = new AmqpHeader();
         AmqpHeader byteCtor = new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });
         AmqpHeader byteCtorSasl = new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 3, 1, 0, 0 });

         Assert.AreEqual(defaultCtor.GetHashCode(), byteCtor.GetHashCode());
         Assert.AreEqual(defaultCtor.GetHashCode(), AmqpHeader.GetAMQPHeader().GetHashCode());
         Assert.AreEqual(byteCtor.GetHashCode(), AmqpHeader.GetAMQPHeader().GetHashCode());
         Assert.AreEqual(byteCtorSasl.GetHashCode(), AmqpHeader.GetSASLHeader().GetHashCode());
         Assert.AreNotEqual(byteCtor.GetHashCode(), AmqpHeader.GetSASLHeader().GetHashCode());
         Assert.AreNotEqual(defaultCtor.GetHashCode(), AmqpHeader.GetSASLHeader().GetHashCode());
         Assert.AreEqual(byteCtorSasl.GetHashCode(), AmqpHeader.GetSASLHeader().GetHashCode());
      }

      [Test]
      public void TestIsTypeMethods()
      {
         AmqpHeader defaultCtor = new AmqpHeader();
         AmqpHeader byteCtor = new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });
         AmqpHeader byteCtorSasl = new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 3, 1, 0, 0 });

         Assert.IsFalse(defaultCtor.IsSaslHeader());
         Assert.IsFalse(byteCtor.IsSaslHeader());
         Assert.IsTrue(byteCtorSasl.IsSaslHeader());
         Assert.IsFalse(AmqpHeader.GetAMQPHeader().IsSaslHeader());
         Assert.IsTrue(AmqpHeader.GetSASLHeader().IsSaslHeader());
      }

      [Test]
      public void TestEquals()
      {
         AmqpHeader defaultCtor = new AmqpHeader();
         AmqpHeader byteCtor = new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });
         AmqpHeader byteCtorSasl = new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 3, 1, 0, 0 });

         Assert.AreEqual(defaultCtor, defaultCtor);
         Assert.AreEqual(defaultCtor, byteCtor);
         Assert.AreEqual(byteCtor, byteCtor);
         Assert.AreEqual(defaultCtor, AmqpHeader.GetAMQPHeader());
         Assert.AreEqual(byteCtor, AmqpHeader.GetAMQPHeader());
         Assert.AreEqual(byteCtorSasl, AmqpHeader.GetSASLHeader());
         Assert.AreNotEqual(byteCtor, AmqpHeader.GetSASLHeader());
         Assert.AreNotEqual(defaultCtor, AmqpHeader.GetSASLHeader());
         Assert.AreEqual(byteCtorSasl, AmqpHeader.GetSASLHeader());

         Assert.IsFalse(AmqpHeader.GetSASLHeader().Equals(null));
         Assert.IsFalse(AmqpHeader.GetSASLHeader().Equals(true));
      }

      [Test]
      public void TestToStringOnDefault()
      {
         AmqpHeader header = new AmqpHeader();
         Assert.IsTrue(header.ToString().StartsWith("AMQP"));
      }

      [Test]
      public void TestValidateByteWithValidHeaderBytes()
      {
         IProtonBuffer buffer = AmqpHeader.GetAMQPHeader().Buffer;

         for (int i = 0; i < AmqpHeader.HeaderSizeBytes; ++i)
         {
            AmqpHeader.ValidateByte(i, buffer[i]);
         }
      }

      [Test]
      public void TestValidateByteWithInvalidHeaderBytes()
      {
         byte[] bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

         for (int i = 0; i < AmqpHeader.HeaderSizeBytes; ++i)
         {
            try
            {
               AmqpHeader.ValidateByte(i, bytes[i]);
               Assert.Fail("Should throw ArgumentException as bytes are invalid");
            }
            catch (ArgumentException)
            {
               // Expected
            }
         }
      }

      [Test]
      public void TestCreateWithNullBuffer()
      {
         Assert.Throws<NullReferenceException>(() => new AmqpHeader((IProtonBuffer)null));
      }

      [Test]
      public void TestCreateWithNullByte()
      {
         Assert.Throws<NullReferenceException>(() => new AmqpHeader((byte[])null));
      }

      [Test]
      public void TestCreateWithEmptyBuffer()
      {
         Assert.Throws<ArgumentException>(() => new AmqpHeader(ProtonByteBufferAllocator.Instance.Allocate()));
      }

      [Test]
      public void TestCreateWithOversizedBuffer()
      {
         Assert.Throws<ArgumentException>(() => new AmqpHeader(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }));
      }

      [Test]
      public void TestCreateWithInvalidHeaderPrefix()
      {
         Assert.Throws<ArgumentException>(() => new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', 0, 0, 1, 0, 0 }));
      }

      [Test]
      public void TestCreateWithInvalidHeaderProtocol()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 4, 1, 0, 0 }));
      }

      [Test]
      public void TestCreateWithInvalidHeaderMajor()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 2, 0, 0 }));
      }

      [Test]
      public void TestCreateWithInvalidHeaderMinor()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 1, 0 }));
      }

      [Test]
      public void TestCreateWithInvalidHeaderRevion()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => new AmqpHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 1 }));
      }

      [Test]
      public void TestValidateHeaderByte0WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(0, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByte1WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(1, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByte2WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(2, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByte3WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(3, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByte4WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(4, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByte5WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(5, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByte6WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(6, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByte7WithInvalidValue()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(7, (byte)85));
      }

      [Test]
      public void TestValidateHeaderByteIndexOutOfBounds()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => AmqpHeader.ValidateByte(9, (byte)85));
      }

      [Test]
      public void TestInvokeOnAMQPHeader()
      {
         string captured = null;

         AmqpHeader.GetAMQPHeader().Invoke<String>(
            (x, context) => captured = context, (x, y) => throw new NotSupportedException(), "test");

         Assert.AreEqual("test", captured);
      }

      [Test]
      public void TestInvokeOnSASLHeader()
      {
         string captured = null;

         AmqpHeader.GetSASLHeader().Invoke<String>(
            (x, y) => throw new NotSupportedException(), (x, context) => captured = context, "test");

         Assert.AreEqual("test", captured);
      }
   }
}