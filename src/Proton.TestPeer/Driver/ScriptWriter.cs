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
using System.Text;
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Expectations;
using Apache.Qpid.Proton.Test.Driver.Matchers;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Basic test script writing API
   /// </summary>
   public abstract class ScriptWriter
   {
      /// <summary>
      /// Provides the implementation with an entry point to give the script writer code
      /// an AMQPTestDriver instance to use in configuring test expectations and actions.
      /// </summary>
      public abstract AMQPTestDriver Driver { get; }

      #region AMQP Performative Expectations scripting APIs

      public AMQPHeaderExpectation ExpectAMQPHeader()
      {
         AMQPHeaderExpectation expecting = new(AMQPHeader.Header, Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public OpenExpectation ExpectOpen()
      {
         OpenExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public CloseExpectation ExpectClose()
      {
         CloseExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public BeginExpectation ExpectBegin()
      {
         BeginExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public EndExpectation ExpectEnd()
      {
         EndExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public AttachExpectation ExpectAttach()
      {
         AttachExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public DetachExpectation ExpectDetach()
      {
         DetachExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public FlowExpectation ExpectFlow()
      {
         FlowExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public TransferExpectation ExpectTransfer()
      {
         TransferExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public DispositionExpectation ExpectDisposition()
      {
         DispositionExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public EmptyFrameExpectation ExpectEmptyFrame()
      {
         EmptyFrameExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      #endregion

      #region Transaction based expectations APIS

      public AttachExpectation ExpectCoordinatorAttach()
      {
         AttachExpectation expecting = new(Driver);

         expecting.WithRole(Role.Sender);
         expecting.WithCoordinator(Matches.IsA(typeof(Coordinator)));
         expecting.WithSource(Is.NotNullValue());

         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public DeclareExpectation ExpectDeclare()
      {
         DeclareExpectation expecting = new(Driver);

         expecting.WithHandle(Is.NotNullValue());
         expecting.WithDeliveryId(Is.NotNullValue());
         expecting.WithDeliveryTag(Is.NotNullValue());
         expecting.WithMessageFormat(Matches.OneOf(null, 0));

         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public DischargeExpectation ExpectDischarge()
      {
         DischargeExpectation expecting = new(Driver);

         expecting.WithHandle(Is.NotNullValue());
         expecting.WithDeliveryId(Is.NotNullValue());
         expecting.WithDeliveryTag(Is.NotNullValue());
         expecting.WithMessageFormat(Matches.OneOf(null, 0));

         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      #endregion

      #region SASL Performative Script APIs

      public AMQPHeaderExpectation ExpectSASLHeader()
      {
         AMQPHeaderExpectation expecting = new(AMQPHeader.SASLHeader, Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public SaslMechanismsExpectation ExpectSaslMechanisms()
      {
         SaslMechanismsExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public SaslInitExpectation ExpectSaslInit()
      {
         SaslInitExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public SaslChallengeExpectation ExpectSaslChallenge()
      {
         SaslChallengeExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public SaslResponseExpectation ExpectSaslResponse()
      {
         SaslResponseExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      public SaslOutcomeExpectation ExpectSaslOutcome()
      {
         SaslOutcomeExpectation expecting = new(Driver);
         Driver.AddScriptedElement(expecting);
         return expecting;
      }

      #endregion

      #region Remote AMQP Performative injection APIs

      public AMQPHeaderInjectAction RemoteHeader(byte[] header)
      {
         return new AMQPHeaderInjectAction(Driver, new AMQPHeader(header));
      }

      public AMQPHeaderInjectAction RemoteHeader(AMQPHeader header)
      {
         return new AMQPHeaderInjectAction(Driver, header);
      }

      public OpenInjectAction RemoteOpen()
      {
         return new OpenInjectAction(Driver);
      }

      public CloseInjectAction RemoteClose()
      {
         return new CloseInjectAction(Driver);
      }

      public BeginInjectAction RemoteBegin()
      {
         return new BeginInjectAction(Driver);
      }

      public EndInjectAction RemoteEnd()
      {
         return new EndInjectAction(Driver);
      }

      public AttachInjectAction RemoteAttach()
      {
         return new AttachInjectAction(Driver);
      }

      public DetachInjectAction RemoteDetach()
      {
         return new DetachInjectAction(Driver);
      }

      public DetachInjectAction RemoteDetachLastCoordinatorLink()
      {
         return new DetachLastCoordinatorInjectAction(Driver);
      }

      public FlowInjectAction RemoteFlow()
      {
         return new FlowInjectAction(Driver);
      }

      public TransferInjectAction RemoteTransfer()
      {
         return new TransferInjectAction(Driver);
      }

      public DispositionInjectAction RemoteDisposition()
      {
         return new DispositionInjectAction(Driver);
      }

      public DeclareInjectAction RemoteDeclare()
      {
         return new DeclareInjectAction(Driver);
      }

      public DischargeInjectAction RemoteDischarge()
      {
         return new DischargeInjectAction(Driver);
      }

      public EmptyFrameInjectAction RemoteEmptyFrame()
      {
         return new EmptyFrameInjectAction(Driver);
      }

      public RawBytesInjectAction RemoteBytes()
      {
         return new RawBytesInjectAction(Driver);
      }

      #endregion

      #region SASL Performative script injection APIs

      public SaslInitInjectAction RemoteSaslInit()
      {
         return new SaslInitInjectAction(Driver);
      }

      public SaslMechanismsInjectAction RemoteSaslMechanisms()
      {
         return new SaslMechanismsInjectAction(Driver);
      }

      public SaslChallengeInjectAction RemoteSaslChallenge()
      {
         return new SaslChallengeInjectAction(Driver);
      }

      public SaslResponseInjectAction RemoteSaslResponse()
      {
         return new SaslResponseInjectAction(Driver);
      }

      public SaslOutcomeInjectAction RemoteSaslOutcome()
      {
         return new SaslOutcomeInjectAction(Driver);
      }

      #endregion

      #region SASL Authentication related test script APIS

      /// <summary>
      /// Creates all the scripted elements needed for a successful SASL Anonymous
      /// connection.
      /// </summary>
      /// <remarks>
      /// For this exchange the SASL header is expected which is responded to with the
      /// corresponding SASL header and an immediate SASL mechanisms frame that only
      /// advertises anonymous as the mechanism.It is expected that the remote will
      /// send a SASL init with the anonymous mechanism selected and the outcome is
      /// predefined as success.Once done the expectation is added for the AMQP
      /// header to arrive and a header response will be sent.
      /// </remarks>
      public void ExpectSASLAnonymousConnect()
      {
         ExpectSASLHeader().RespondWithSASLHeader();
         RemoteSaslMechanisms().WithMechanisms("ANONYMOUS").Queue();
         ExpectSaslInit().WithMechanism("ANONYMOUS");
         RemoteSaslOutcome().WithCode(SaslCode.Ok).Queue();
         ExpectAMQPHeader().RespondWithAMQPHeader();
      }

      /// <summary>
      /// Creates all the scripted elements needed for a successful SASL Plain connection.
      /// </summary>
      /// <remarks>
      /// For this exchange the SASL header is expected which is responded to with the
      /// corresponding SASL header and an immediate SASL mechanisms frame that only
      /// advertises plain as the mechanism.  It is expected that the remote will
      /// send a SASL init with the plain mechanism selected and the outcome is
      /// predefined as success.  Once done the expectation is added for the AMQP
      /// header to arrive and a header response will be sent.
      /// </remarks>
      public void ExpectSASLPlainConnect(string username, string password)
      {
         ExpectSASLHeader().RespondWithSASLHeader();
         RemoteSaslMechanisms().WithMechanisms("PLAIN").Queue();
         ExpectSaslInit().WithMechanism("PLAIN").WithInitialResponse(SaslPlainInitialResponse(username, password));
         RemoteSaslOutcome().WithCode(SaslCode.Ok).Queue();
         ExpectAMQPHeader().RespondWithAMQPHeader();
      }

      /// <summary>
      /// Creates all the scripted elements needed for a successful SASL XOAUTH2 connection.
      /// </summary>
      /// <remarks>
      /// For this exchange the SASL header is expected which is responded to with the
      /// corresponding SASL header and an immediate SASL mechanisms frame that only
      /// advertises XOAUTH2 as the mechanism.  It is expected that the remote will
      /// send a SASL init with the XOAUTH2 mechanism selected and the outcome is
      /// predefined as success.  Once done the expectation is added for the AMQP
      /// header to arrive and a header response will be sent.
      /// </remarks>
      public void ExpectSaslXOauth2Connect(string username, string password)
      {
         ExpectSASLHeader().RespondWithSASLHeader();
         RemoteSaslMechanisms().WithMechanisms("XOAUTH2").Queue();
         ExpectSaslInit().WithMechanism("XOAUTH2").WithInitialResponse(SaslXOauth2InitialResponse(username, password));
         RemoteSaslOutcome().WithCode(SaslCode.Ok).Queue();
         ExpectAMQPHeader().RespondWithAMQPHeader();
      }

      /// <summary>
      /// Creates all the scripted elements needed for a failed SASL Plain connection.
      /// </summary>
      /// <remarks>
      /// For this exchange the SASL header is expected which is responded to with the
      /// corresponding SASL header and an immediate SASL mechanisms frame that only
      /// advertises plain as the mechanism.  It is expected that the remote will
      /// send a SASL init with the plain mechanism selected and the outcome is
      /// predefined failing the exchange.
      /// </remarks>
      public void ExpectFailingSASLPlainConnect(byte saslCode)
      {
         ExpectFailingSASLPlainConnect(saslCode, "PLAIN");
      }

      /// <summary>
      /// Creates all the scripted elements needed for a failed SASL Plain connection.
      /// </summary>
      /// <remarks>
      /// For this exchange the SASL header is expected which is responded to with the
      /// corresponding SASL header and an immediate SASL mechanisms frame that only
      /// advertises plain as the mechanism.  It is expected that the remote will
      /// send a SASL init with the plain mechanism selected and the outcome is
      /// predefined failing the exchange.
      /// </remarks>
      public void ExpectFailingSASLPlainConnect(byte saslCode, params string[] offeredMechanisms)
      {
         if (!(new List<string>(offeredMechanisms).Contains("PLAIN")))
         {
            throw new ArgumentException("Expected offered mechanisms that contains the PLAIN mechanism");
         }

         ExpectSASLHeader().RespondWithSASLHeader();
         RemoteSaslMechanisms().WithMechanisms(offeredMechanisms).Queue();
         ExpectSaslInit().WithMechanism("PLAIN");

         if (saslCode > (byte)SaslCode.SysTemp)
         {
            throw new ArgumentOutOfRangeException(nameof(saslCode), "SASL Code should indicate a failure");
         }

         RemoteSaslOutcome().WithCode((SaslCode)saslCode).Queue();
      }

      /// <summary>
      /// Creates all the scripted elements needed for a successful SASL EXTERNAL connection.
      /// </summary>
      /// <remarks>
      /// For this exchange the SASL header is expected which is responded to with the
      /// corresponding SASL header and an immediate SASL mechanisms frame that only
      /// advertises EXTERNAL as the mechanism.  It is expected that the remote will
      /// send a SASL init with the EXTERNAL mechanism selected and the outcome is
      /// predefined as success.  Once done the expectation is added for the AMQP
      /// header to arrive and a header response will be sent.
      /// </remarks>
      public void ExpectSaslExternalConnect()
      {
         ExpectSASLHeader().RespondWithSASLHeader();
         RemoteSaslMechanisms().WithMechanisms("EXTERNAL").Queue();
         ExpectSaslInit().WithMechanism("EXTERNAL").WithInitialResponse(Array.Empty<byte>());
         RemoteSaslOutcome().WithCode(SaslCode.Ok).Queue();
         ExpectAMQPHeader().RespondWithAMQPHeader();
      }

      /// <summary>
      /// Creates all the scripted elements needed for a SASL exchange with the offered
      /// mechanisms but the client should fail if configured such that it cannot match
      /// any of those to its own available mechanisms.
      /// </summary>
      public void ExpectSaslMechanismNegotiationFailure(params string[] offeredMechanisms)
      {
         ExpectSASLHeader().RespondWithSASLHeader();
         RemoteSaslMechanisms().WithMechanisms(offeredMechanisms).Queue();
      }

      /// <summary>
      /// Creates all the scripted elements needed for a SASL exchange with the offered
      /// mechanisms with the expectation that the client will respond with the provided
      /// mechanism and then the server will fail the exchange with the auth failed code.
      /// </summary>
      public void ExpectSaslConnectThatAlwaysFailsAuthentication(string[] offeredMechanisms, string chosenMechanism)
      {
         ExpectSASLHeader().RespondWithSASLHeader();
         RemoteSaslMechanisms().WithMechanisms(offeredMechanisms).Queue();
         ExpectSaslInit().WithMechanism(chosenMechanism);
         RemoteSaslOutcome().WithCode(SaslCode.Auth).Queue();
      }

      #endregion

      #region Scripted responses for various scenarios that might be tested

      /// <summary>
      /// Creates a Begin response for the last session Begin that was received and fills in the Begin
      /// fields based on values from the remote.  The caller can further customize the Begin that is
      /// emitted by using the various with methods to assign values to the fields in the Begin.
      /// </summary>
      public BeginInjectAction RespondToLastBegin()
      {
         BeginInjectAction response = new(Driver);

         SessionTracker session = Driver.Sessions.LastRemotelyOpenedSession;
         if (session == null)
         {
            throw new InvalidOperationException("Cannot create response to Begin before one has been received.");
         }

         // Populate the response using data in the locally opened session, script can override this after return.
         response.WithRemoteChannel((ushort)session.RemoteChannel);

         return response;
      }

      /// <summary>
      /// Creates a Attach response for the last link Attach that was received and fills in the Attach
      /// fields based on values from the remote.  The caller can further customize the Attach that is
      /// emitted by using the various with methods to assign values to the fields in the Attach.
      /// </summary>
      public AttachInjectAction RespondToLastAttach()
      {
         AttachInjectAction response = new(Driver);

         SessionTracker session = Driver.Sessions.LastRemotelyOpenedSession;
         LinkTracker link = session.LastRemotelyOpenedLink;

         if (link == null)
         {
            throw new InvalidOperationException("Cannot create response to Attach before one has been received.");
         }

         if (link.IsLocallyAttached)
         {
            throw new InvalidOperationException("Cannot create response to Attach since a local Attach was already sent.");
         }

         // Populate the response using data in the locally opened link, script can override this after return.
         response.OnChannel((ushort)link.Session.LocalChannel);
         response.WithName(link.Name);
         response.WithRole(link.Role);
         response.WithSndSettleMode(link.RemoteSenderSettleMode);
         response.WithRcvSettleMode(link.RemoteReceiverSettleMode);

         if (link.RemoteSource != null)
         {
            response.WithSource(new Source(link.RemoteSource));
            if ((link.RemoteSource.Dynamic ?? false))
            {
               response.WithSource().WithAddress(Guid.NewGuid().ToString());
            }
         }
         if (link.RemoteTarget != null)
         {
            response.WithTarget(new Target(link.RemoteTarget));
            if ((link.RemoteTarget.Dynamic ?? false))
            {
               response.WithTarget().WithAddress(Guid.NewGuid().ToString());
            }
         }
         if (link.RemoteCoordinator != null)
         {
            response.WithTarget(new Coordinator(link.RemoteCoordinator));
         }

         if (response.Performative.InitialDeliveryCount == null)
         {
            if (link.IsSender)
            {
               response.WithInitialDeliveryCount(0);
            }
         }

         return response;
      }

      #endregion

      #region Allows for user code injection into the test script

      public ExecuteUserCodeAction Execute(Action action)
      {
         return new ExecuteUserCodeAction(Driver, action);
      }

      #endregion

      #region Immediate actions that are performed outside the test script

      public void Fire(AMQPHeader header)
      {
         Driver.SendHeader(header);
      }

      public void FireAMQP(IDescribedType performative)
      {
         Driver.SendAMQPFrame(0, performative, null);
      }

      public void FireSASL(IDescribedType performative)
      {
         Driver.SendSaslFrame(0, performative);
      }

      #endregion

      #region Utility Methods for tests

      public byte[] SaslPlainInitialResponse(string username, string password)
      {
         byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
         byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
         byte[] initialResponse = new byte[usernameBytes.Length + passwordBytes.Length + 2];

         Array.Copy(usernameBytes, 0, initialResponse, 1, usernameBytes.Length);
         Array.Copy(passwordBytes, 0, initialResponse, 2 + usernameBytes.Length, passwordBytes.Length);

         return initialResponse;
      }

      public byte[] SaslXOauth2InitialResponse(string username, string password)
      {
         byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
         byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
         byte[] initialResponse = new byte[usernameBytes.Length + passwordBytes.Length + 20];

         byte[] userPrefix = Encoding.ASCII.GetBytes("user=");
         byte[] passPrefix = Encoding.ASCII.GetBytes("auth=Bearer ");

         Array.Copy(userPrefix, 0, initialResponse, 0, 5);
         Array.Copy(usernameBytes, 0, initialResponse, 5, usernameBytes.Length);
         initialResponse[5 + usernameBytes.Length] = 1;
         Array.Copy(passPrefix, 0, initialResponse, 6 + usernameBytes.Length, 12);
         Array.Copy(passwordBytes, 0, initialResponse, 18 + usernameBytes.Length, passwordBytes.Length);
         initialResponse[initialResponse.Length - 2] = 1;
         initialResponse[initialResponse.Length - 1] = 1;

         return initialResponse;
      }

      #endregion
   }
}