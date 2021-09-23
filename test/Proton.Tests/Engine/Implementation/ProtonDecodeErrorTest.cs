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
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonDecodeErrorTest : ProtonEngineTestSupport
   {
      [Test]
      public void TestEmptyContainerIdInOpenProvokesDecodeError()
      {
         // Provide the bytes for Open, but omit the mandatory container-id to provoke a decode error.
         byte[] bytes = new byte[] { 0x00, 0x00, 0x00, 0x0F, // Frame size = 15 bytes.
                                     0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
                                     0x00, 0x53, 0x10, (byte) 0xC0, // Described-type, ulong type, open descriptor, list0.
                                     0x03, 0x01, 0x40 }; // size (3), count (1), container-id (null).

         DoInvalidOpenProvokesDecodeErrorTestImpl(bytes, "The container-id field cannot be omitted from the Open");
      }

      [Test]
      public void TestEmptyOpenProvokesDecodeError()
      {
         // Provide the bytes for Open, but omit the mandatory container-id to provoke a decode error.
         byte[] bytes = new byte[] { 0x00, 0x00, 0x00, 0x0C, // Frame size = 12 bytes.
                                     0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
                                     0x00, 0x53, 0x10, 0x45};// Described-type, ulong type, open descriptor, list0.

         DoInvalidOpenProvokesDecodeErrorTestImpl(bytes, "The container-id field cannot be omitted from the Open");
      }

      private void DoInvalidOpenProvokesDecodeErrorTestImpl(byte[] bytes, String errorDescription)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();

         engine.Start().Open();

         peer.WaitForScriptToCompleteIgnoreErrors();

         peer.ExpectClose().WithError(AmqpError.DECODE_ERROR.ToString(), errorDescription);
         peer.RemoteBytes().WithBytes(bytes).Now();

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is FrameDecodingException);
         Assert.AreEqual(errorDescription, failure.Message);
      }

      [Test]
      public void TestEmptyBeginProvokesDecodeError()
      {
         // Provide the bytes for Begin, but omit any fields to provoke a decode error.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0C, // Frame size = 12 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x11, 0x45};// Described-type, ulong type, Begin descriptor, list0.

         DoInvalidBeginProvokesDecodeErrorTestImpl(bytes, "The next-outgoing-id field cannot be omitted from the Begin");
      }

      [Test]
      public void TestTruncatedBeginProvokesDecodeError1()
      {
         // Provide the bytes for Begin, but only give a null (i-e not-present) for the remote-channel.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0F, // Frame size = 15 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x11, (byte) 0xC0, // Described-type, ulong type, Begin descriptor, list8.
            0x03, 0x01, 0x40 }; // size (3), count (1), remote-channel (null).

         DoInvalidBeginProvokesDecodeErrorTestImpl(bytes, "The next-outgoing-id field cannot be omitted from the Begin");
      }

      [Test]
      public void TestTruncatedBeginProvokesDecodeError2()
      {
         // Provide the bytes for Begin, but only give a [not-present remote-channel +] next-outgoing-id and incoming-window. Provokes a decode error as there must be 4 fields.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x11, // Frame size = 17 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x11, (byte) 0xC0, // Described-type, ulong type, Begin descriptor, list8.
            0x05, 0x03, 0x40, 0x43, 0x43 }; // size (5), count (3), remote-channel (null), next-outgoing-id (uint0), incoming-window (uint0).

         DoInvalidBeginProvokesDecodeErrorTestImpl(bytes, "The outgoing-window field cannot be omitted from the Begin");
      }

      [Test]
      public void TestTruncatedBeginProvokesDecodeError3()
      {
         // Provide the bytes for Begin, but only give a [not-present remote-channel +] next-outgoing-id and incoming-window, and send not-present/null for outgoing-window. Provokes a decode error as must be present.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x12, // Frame size = 18 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x11, (byte) 0xC0, // Described-type, ulong type, Begin descriptor, list8.
            0x06, 0x04, 0x40, 0x43, 0x43, 0x40 }; // size (5), count (4), remote-channel (null), next-outgoing-id (uint0), incoming-window (uint0), outgoing-window (null).

         DoInvalidBeginProvokesDecodeErrorTestImpl(bytes, "The outgoing-window field cannot be omitted from the Begin");
      }

      private void DoInvalidBeginProvokesDecodeErrorTestImpl(byte[] bytes, String errorDescription)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         engine.Start().Open();

         peer.WaitForScriptToCompleteIgnoreErrors();

         peer.ExpectClose().WithError(AmqpError.DECODE_ERROR.ToString(), errorDescription);
         peer.RemoteBytes().WithBytes(bytes).Now();

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is FrameDecodingException);
         Assert.AreEqual(errorDescription, failure.Message);
      }

      [Test]
      public void TestEmptyFlowProvokesDecodeError()
      {
         // Provide the bytes for Flow, but omit any fields to provoke a decode error.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0C, // Frame size = 12 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x13, 0x45};// Described-type, ulong type, Flow descriptor, list0.

         DoInvalidFlowProvokesDecodeErrorTestImpl(bytes, "The incoming-window field cannot be omitted from the Flow");
      }

      [Test]
      public void TestTruncatedFlowProvokesDecodeError1()
      {
         // Provide the bytes for Flow, but only give a 0 for the next-incoming-id. Provokes a decode error as there must be 4 fields.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0F, // Frame size = 15 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x13, (byte) 0xC0, // Described-type, ulong type, Flow descriptor, list8.
            0x03, 0x01, 0x43 }; // size (3), count (1), next-incoming-id (uint0).

         DoInvalidFlowProvokesDecodeErrorTestImpl(bytes, "The incoming-window field cannot be omitted from the Flow");
      }

      [Test]
      public void TestTruncatedFlowProvokesDecodeError2()
      {
         // Provide the bytes for Flow, but only give a next-incoming-id and incoming-window and next-outgoing-id. Provokes a decode error as there must be 4 fields.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x11, // Frame size = 17 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x13, (byte) 0xC0, // Described-type, ulong type, Flow descriptor, list8.
            0x05, 0x03, 0x43, 0x43, 0x43 }; // size (5), count (3), next-incoming-id (0), incoming-window (uint0), next-outgoing-id (uint0).

         DoInvalidFlowProvokesDecodeErrorTestImpl(bytes, "The outgoing-window field cannot be omitted from the Flow");
      }

      [Test]
      public void TestTruncatedFlowProvokesDecodeError3()
      {
         // Provide the bytes for Flow, but only give a next-incoming-id and incoming-window and next-outgoing-id, and send not-present/null for outgoing-window. Provokes a decode error as must be present.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x12, // Frame size = 18 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x13, (byte) 0xC0, // Described-type, ulong type, Flow descriptor, list8.
            0x06, 0x04, 0x43, 0x43, 0x43, 0x40 }; // size (5), count (4), next-incoming-id (0), incoming-window (uint0), next-outgoing-id (uint0), outgoing-window (null).

         DoInvalidFlowProvokesDecodeErrorTestImpl(bytes, "The outgoing-window field cannot be omitted from the Flow");
      }

      private void DoInvalidFlowProvokesDecodeErrorTestImpl(byte[] bytes, String errorDescription)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteBytes().WithBytes(bytes).Queue();  // Queue the frame for write after expected setup
         peer.ExpectClose().WithError(AmqpError.DECODE_ERROR.ToString(), errorDescription);

         IConnection connection = engine.Start().Open();
         connection.Session().Open();

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is FrameDecodingException);
         Assert.AreEqual(errorDescription, failure.Message);
      }

      [Test]
      [Ignore("Test fails due to missing handle in Transfer")]
      public void TestEmptyTransferProvokesDecodeError()
      {
         // Provide the bytes for Transfer, but omit any fields to provoke a decode error.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0C, // Frame size = 12 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x14, 0x45};// Described-type, ulong type, Transfer descriptor, list0.

         DoInvalidTransferProvokesDecodeErrorTestImpl(bytes, "The handle field cannot be omitted");
      }

      [Test]
      public void TestTruncatedTransferProvokesDecodeError()
      {
         // Provide the bytes for Transfer, but only give a null for the not-present handle. Provokes a decode error as there must be a handle.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0F, // Frame size = 15 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x14, (byte) 0xC0, // Described-type, ulong type, Transfer descriptor, list8.
            0x03, 0x01, 0x40 }; // size (3), count (1), handle (null / not-present).

         DoInvalidTransferProvokesDecodeErrorTestImpl(bytes, "The handle field cannot be omitted from the Transfer");
      }

      [Test]
      [Ignore("Test fails due to wrong type error")]
      public void TestTransferWithWrongHandleTypeCodeProvokesDecodeError()
      {
         // Provide the bytes for Transfer, but give the wrong type code for a not-really-present handle. Provokes a decode error.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0F, // Frame size = 15 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x14, (byte) 0xC0, // Described-type, ulong type, Transfer descriptor, list8.
            0x03, 0x01, (byte) 0xA3 }; // size (3), count (1), handle (invalid sym8 type constructor given, not really present).

         DoInvalidTransferProvokesDecodeErrorTestImpl(bytes, "Expected Unsigned Integer type but found encoding: SYM8:0xa3");
      }

      private void DoInvalidTransferProvokesDecodeErrorTestImpl(byte[] bytes, String errorDescription)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteBytes().WithBytes(bytes).Queue();  // Queue the frame for write after expected setup
         peer.ExpectClose().WithError(AmqpError.DECODE_ERROR.ToString(), errorDescription);

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open();

         receiver.AddCredit(1);

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is FrameDecodingException);
         Assert.AreEqual(errorDescription, failure.Message);
      }

      [Test]
      [Ignore("Fails due to issues in test peer state tracking")]
      public void TestEmptyDispositionProvokesDecodeError()
      {
         // Provide the bytes for Disposition, but omit any fields to provoke a decode error.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x0C, // Frame size = 12 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x15, 0x45};// Described-type, ulong type, Disposition descriptor, list0.

         DoInvalidDispositionProvokesDecodeErrorTestImpl(bytes, "The role field cannot be omitted from the Disposition");
      }

      [Test]
      [Ignore("Fails due to issues in test peer state tracking")]
      public void TestTruncatedDispositionProvokesDecodeError()
      {
         // Provide the bytes for Disposition, but only give a null/not-present for the 'first' field. Provokes a decode error as there must be a role and 'first'.
         byte[] bytes = new byte[] {
            0x00, 0x00, 0x00, 0x10, // Frame size = 16 bytes.
            0x02, 0x00, 0x00, 0x00, // DOFF, TYPE, 2x CHANNEL
            0x00, 0x53, 0x15, (byte) 0xC0, // Described-type, ulong type, Disposition descriptor, list8.
            0x04, 0x02, 0x41, 0x40 }; // size (4), count (2), role (receiver - the peers perspective), first ( null / not-present)

         DoInvalidDispositionProvokesDecodeErrorTestImpl(bytes, "The first field cannot be omitted from the Disposition");
      }

      private void DoInvalidDispositionProvokesDecodeErrorTestImpl(byte[] bytes, String errorDescription)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(10).Queue();
         peer.ExpectTransfer();
         peer.RemoteBytes().WithBytes(bytes).Queue();  // Queue the frame for write after expected setup
         peer.ExpectClose().WithError(AmqpError.DECODE_ERROR.ToString(), errorDescription);

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test").Open();

         IOutgoingDelivery delivery = sender.Next;
         delivery.WriteBytes(payload.Copy());

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is FrameDecodingException);
         Assert.AreEqual(errorDescription, failure.Message);
      }
   }
}