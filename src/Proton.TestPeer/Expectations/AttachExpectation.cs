/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed With
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance With
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
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class AttachExpectation : AbstractExpectation<Attach>
   {
      private readonly AttachMatcher matcher = new AttachMatcher();

      private AttachInjectAction response;
      private bool rejecting;

      public AttachExpectation(AMQPTestDriver driver) : base(driver)
      {
         // Configure default expectations for a valid Attach
         WithName(Is.NotNullValue());
         WithHandle(Is.NotNullValue());
         WithRole(Is.NotNullValue());
      }

      public override AttachExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public AttachInjectAction Respond()
      {
         response = new AttachInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      public DetachInjectAction Reject(bool close, string condition, string description)
      {
         return Reject(close, new Symbol(condition), description);
      }

      public DetachInjectAction Reject(bool close, Symbol condition, string description)
      {
         rejecting = true;
         response = new AttachInjectAction(driver);
         driver.AddScriptedElement(response);

         DetachInjectAction action =
             new DetachInjectAction(driver).WithClosed(close).WithErrorCondition(condition, description);
         driver.AddScriptedElement(action);

         return action;
      }

      public AttachExpectation WithName(string name)
      {
         return WithName(Is.EqualTo(name));
      }

      public AttachExpectation WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public AttachExpectation WithRole(bool role)
      {
         return WithRole(Is.EqualTo(role));
      }

      public AttachExpectation WithRole(Role role)
      {
         return WithRole(Is.EqualTo(RoleExtension.ToBooleanEncoding(role)));
      }

      public AttachExpectation OfSender()
      {
         return WithRole(Is.EqualTo(false));
      }

      public AttachExpectation OfReceiver()
      {
         return WithRole(Is.EqualTo(true));
      }

      public AttachExpectation WithSndSettleMode(byte sndSettleMode)
      {
         return WithSndSettleMode(Is.EqualTo((SenderSettleMode)sndSettleMode));
      }

      public AttachExpectation WithSndSettleMode(byte? sndSettleMode)
      {
         if (sndSettleMode.HasValue)
         {
            return WithSndSettleMode(Is.EqualTo((SenderSettleMode)sndSettleMode));
         }
         else
         {
            return WithRcvSettleMode(Is.NullValue());
         }
      }

      public AttachExpectation WithSndSettleMode(SenderSettleMode sndSettleMode)
      {
         return WithSndSettleMode(Is.EqualTo(sndSettleMode));
      }

      public AttachExpectation WithSenderSettleModeMixed()
      {
         return WithSndSettleMode(Is.EqualTo(SenderSettleMode.Mixed));
      }

      public AttachExpectation WithSenderSettleModeSettled()
      {
         return WithSndSettleMode(Is.EqualTo(SenderSettleMode.Settked));
      }

      public AttachExpectation WithSenderSettleModeUnsettled()
      {
         return WithSndSettleMode(Is.EqualTo(SenderSettleMode.Unsettled));
      }

      public AttachExpectation WithRcvSettleMode(byte rcvSettleMode)
      {
         return WithRcvSettleMode(Is.EqualTo(rcvSettleMode));
      }

      public AttachExpectation WithRcvSettleMode(byte? rcvSettleMode)
      {
         if (rcvSettleMode.HasValue)
         {
            return WithRcvSettleMode(Is.EqualTo(rcvSettleMode));
         }
         else
         {
            return WithRcvSettleMode(Is.NullValue());
         }
      }

      public AttachExpectation WithRcvSettleMode(ReceiverSettleMode rcvSettleMode)
      {
         return WithRcvSettleMode(Is.EqualTo(rcvSettleMode));
      }

      public AttachExpectation WithReceiverSettlesFirst()
      {
         return WithRcvSettleMode(Is.EqualTo(ReceiverSettleMode.First));
      }

      public AttachExpectation WithReceiverSettlesSecond()
      {
         return WithRcvSettleMode(Is.EqualTo(ReceiverSettleMode.Second));
      }

      public AttachSourceMatcher WithSource()
      {
         AttachSourceMatcher matcher = new AttachSourceMatcher(this);
         WithSource(matcher);
         return matcher;
      }

      public AttachTargetMatcher WithTarget()
      {
         AttachTargetMatcher matcher = new AttachTargetMatcher(this);
         WithTarget(matcher);
         return matcher;
      }

      public AttachCoordinatorMatcher WithCoordinator()
      {
         AttachCoordinatorMatcher matcher = new AttachCoordinatorMatcher(this);
         WithCoordinator(matcher);
         return matcher;
      }

      public AttachExpectation WithSource(Source source)
      {
         if (source != null)
         {
            SourceMatcher sourceMatcher = new SourceMatcher(source);
            return WithSource(sourceMatcher);
         }
         else
         {
            return WithSource(Is.NullValue());
         }
      }

      public AttachExpectation WithTarget(Target target)
      {
         if (target != null)
         {
            TargetMatcher targetMatcher = new TargetMatcher(target);
            return WithTarget(targetMatcher);
         }
         else
         {
            return WithTarget(Is.NullValue());
         }
      }

      public AttachExpectation WithCoordinator(Coordinator coordinator)
      {
         if (coordinator != null)
         {
            CoordinatorMatcher coordinatorMatcher = new CoordinatorMatcher();
            return WithCoordinator(coordinatorMatcher);
         }
         else
         {
            return WithCoordinator(Is.NullValue());
         }
      }

      public AttachExpectation WithUnsettled(IDictionary<Binary, IDeliveryState> unsettled)
      {
         // TODO - Need to match on the driver types for DeliveryState
         return WithUnsettled(Is.EqualTo(unsettled));
      }

      public AttachExpectation WithIncompleteUnsettled(bool incomplete)
      {
         return WithIncompleteUnsettled(Is.EqualTo(incomplete));
      }

      public AttachExpectation WithInitialDeliveryCount(uint initialDeliveryCount)
      {
         return WithInitialDeliveryCount(Is.EqualTo(initialDeliveryCount));
      }

      public AttachExpectation WithMaxMessageSize(ulong maxMessageSize)
      {
         return WithMaxMessageSize(Is.EqualTo(maxMessageSize));
      }

      public AttachExpectation WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(offeredCapabilities));
      }

      public AttachExpectation WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(offeredCapabilities)));
      }

      public AttachExpectation WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(desiredCapabilities));
      }

      public AttachExpectation WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(desiredCapabilities)));
      }

      public AttachExpectation WithPropertiesMap(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      public AttachExpectation WithProperties(IDictionary<string, object> properties)
      {
         return WithProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      #region Matcher based With methods

      public AttachExpectation WithName(IMatcher m)
      {
         matcher.WithName(m);
         return this;
      }

      public AttachExpectation WithHandle(IMatcher m)
      {
         matcher.WithHandle(m);
         return this;
      }

      public AttachExpectation WithRole(IMatcher m)
      {
         matcher.WithRole(m);
         return this;
      }

      public AttachExpectation WithSndSettleMode(IMatcher m)
      {
         matcher.WithSndSettleMode(m);
         return this;
      }

      public AttachExpectation WithRcvSettleMode(IMatcher m)
      {
         matcher.WithRcvSettleMode(m);
         return this;
      }

      public AttachExpectation WithSource(IMatcher m)
      {
         matcher.WithSource(m);
         return this;
      }

      public AttachExpectation WithTarget(IMatcher m)
      {
         matcher.WithTarget(m);
         return this;
      }

      public AttachExpectation WithCoordinator(IMatcher m)
      {
         matcher.WithCoordinator(m);
         return this;
      }

      public AttachExpectation WithUnsettled(IMatcher m)
      {
         matcher.WithUnsettled(m);
         return this;
      }

      public AttachExpectation WithIncompleteUnsettled(IMatcher m)
      {
         matcher.WithIncompleteUnsettled(m);
         return this;
      }

      public AttachExpectation WithInitialDeliveryCount(IMatcher m)
      {
         matcher.WithInitialDeliveryCount(m);
         return this;
      }

      public AttachExpectation WithMaxMessageSize(IMatcher m)
      {
         matcher.WithMaxMessageSize(m);
         return this;
      }

      public AttachExpectation WithOfferedCapabilities(IMatcher m)
      {
         matcher.WithOfferedCapabilities(m);
         return this;
      }

      public AttachExpectation WithDesiredCapabilities(IMatcher m)
      {
         matcher.WithDesiredCapabilities(m);
         return this;
      }

      public AttachExpectation WithProperties(IMatcher m)
      {
         matcher.WithProperties(m);
         return this;
      }

      #endregion

      public override void HandleAttach(uint frameSize, Attach attach, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleAttach(frameSize, attach, payload, channel, context);

         SessionTracker session = driver.Sessions.SessionFromRemoteChannel(channel);

         if (session == null)
         {
            throw new AssertionError(string.Format(
                "Received Attach on channel [{0}] that has no matching Session for that remote channel. ", channel));
         }

         LinkTracker link = session.HandleRemoteAttach(attach);

         if (response != null)
         {
            // Input was validated now populate response with auto values where not configured
            // to say otherwise by the test.
            if (response.OnChannel() == null)
            {
               response.OnChannel((ushort)link.Session.LocalChannel);
            }

            // Populate the fields of the response with defaults if non set by the test script
            if (response.Performative.Handle == null)
            {
               response.WithHandle((uint)attach.Handle);
            }
            if (response.Performative.Name == null)
            {
               response.WithName(attach.Name);
            }
            if (response.Performative.Role == null)
            {
               response.WithRole(attach.Role?.ReverseOf() ?? Role.Sender);
            }
            if (response.Performative.SenderSettleMode == null)
            {
               response.WithSndSettleMode(attach.SenderSettleMode ?? SenderSettleMode.Mixed);
            }
            if (response.Performative.ReceiverSettleMode == null)
            {
               response.WithRcvSettleMode(attach.ReceiverSettleMode ?? ReceiverSettleMode.First);
            }
            if (response.Performative.Source == null && !response.IsNullSourceRequired)
            {
               response.WithSource(attach.Source);
               if (attach.Source != null && (attach.Source.Dynamic ?? false))
               {
                  attach.Source.Address = Guid.NewGuid().ToString();
               }
            }

            if (rejecting)
            {
               if ((attach.Role ?? Role.Sender) == Role.Sender)
               {
                  // Sender attach so response should have null target
                  response.WithNullTarget();
               }
               else
               {
                  // Receiver attach so response should have null source
                  response.WithNullSource();
               }
            }

            if (response.Performative.Target == null && !response.IsNullTargetRequired)
            {
               if (attach.Target != null)
               {
                  if (attach.Target is Target) {
                     Target target = (Target)attach.Target;
                     response.WithTarget(target);
                     if (target != null && (target.Dynamic ?? false))
                     {
                        target.Address = Guid.NewGuid().ToString();
                     }
                  }
                  else
                  {
                     Coordinator coordinator = (Coordinator)attach.Target;
                     response.WithTarget(coordinator);
                  }
               }
            }

            if (response.Performative.InitialDeliveryCount == null)
            {
               Role role = response.Performative.Role ?? Role.Receiver;
               if (role == Role.Sender)
               {
                  response.WithInitialDeliveryCount(0);
               }
            }

            // Other fields are left not set for now unless test script configured
         }
      }

      protected override IMatcher GetExpectationMatcher()
      {
         return matcher;
      }
   }

   public sealed class AttachSourceMatcher : SourceMatcher
   {
      private readonly AttachExpectation expectation;

      public AttachSourceMatcher(AttachExpectation expectation)
      {
         this.expectation = expectation;
      }

      public AttachExpectation Also()
      {
         return expectation;
      }

      public AttachExpectation And()
      {
         return expectation;
      }

      public override AttachSourceMatcher WithAddress(string name)
      {
         base.WithAddress(name);
         return this;
      }

      public override AttachSourceMatcher WithDurable(TerminusDurability durability)
      {
         base.WithDurable(durability);
         return this;
      }

      public override AttachSourceMatcher WithExpiryPolicy(TerminusExpiryPolicy expiry)
      {
         base.WithExpiryPolicy(expiry);
         return this;
      }

      public override AttachSourceMatcher WithTimeout(uint timeout)
      {
         base.WithTimeout(timeout);
         return this;
      }

      public override AttachSourceMatcher WithDynamic(bool dynamic)
      {
         base.WithDynamic(dynamic);
         return this;
      }

      public override AttachSourceMatcher WithDynamicNodeProperties(IDictionary<Symbol, object> properties)
      {
         base.WithDynamicNodeProperties(properties);
         return this;
      }

      public override AttachSourceMatcher WithDynamicNodeProperties(IDictionary<string, object> properties)
      {
         base.WithDynamicNodeProperties(properties);
         return this;
      }

      public override AttachSourceMatcher WithDistributionMode(string distributionMode)
      {
         base.WithDistributionMode(distributionMode);
         return this;
      }

      public override AttachSourceMatcher WithDistributionMode(Symbol distributionMode)
      {
         base.WithDistributionMode(distributionMode);
         return this;
      }

      public override AttachSourceMatcher WithFilter(IDictionary<string, object> filter)
      {
         base.WithFilter(filter);
         return this;
      }

      public override AttachSourceMatcher WithDefaultOutcome(IDeliveryState defaultOutcome)
      {
         base.WithDefaultOutcome(defaultOutcome);
         return this;
      }

      public override AttachSourceMatcher WithOutcomes(params string[] outcomes)
      {
         base.WithOutcomes(outcomes);
         return this;
      }

      public override AttachSourceMatcher WithOutcomes(params Symbol[] outcomes)
      {
         base.WithOutcomes(outcomes);
         return this;
      }

      public override AttachSourceMatcher WithCapabilities(params string[] capabilities)
      {
         base.WithCapabilities(capabilities);
         return this;
      }

      public override AttachSourceMatcher WithCapabilities(params Symbol[] capabilities)
      {
         base.WithCapabilities(capabilities);
         return this;
      }

      public override AttachSourceMatcher WithAddress(IMatcher m)
      {
         base.WithAddress(m);
         return this;
      }

      public override AttachSourceMatcher WithDurable(IMatcher m)
      {
         base.WithDurable(m);
         return this;
      }

      public override AttachSourceMatcher WithExpiryPolicy(IMatcher m)
      {
         base.WithExpiryPolicy(m);
         return this;
      }

      public override AttachSourceMatcher WithTimeout(IMatcher m)
      {
         base.WithTimeout(m);
         return this;
      }

      public override AttachSourceMatcher WithDefaultTimeout()
      {
         base.WithTimeout(Apache.Qpid.Proton.Test.Driver.Matchers.Matches.AnyOf(Is.NullValue(), Is.EqualTo(0)));
         return this;
      }

      public override AttachSourceMatcher WithDynamic(IMatcher m)
      {
         base.WithDynamic(m);
         return this;
      }

      public override AttachSourceMatcher WithDynamicNodeProperties(IMatcher m)
      {
         base.WithDynamicNodeProperties(m);
         return this;
      }

      public override AttachSourceMatcher WithDistributionMode(IMatcher m)
      {
         base.WithDistributionMode(m);
         return this;
      }

      public override AttachSourceMatcher WithFilter(IMatcher m)
      {
         base.WithFilter(m);
         return this;
      }

      public override AttachSourceMatcher WithDefaultOutcome(IMatcher m)
      {
         base.WithDefaultOutcome(m);
         return this;
      }

      public override AttachSourceMatcher WithOutcomes(IMatcher m)
      {
         base.WithOutcomes(m);
         return this;
      }

      public override AttachSourceMatcher WithCapabilities(IMatcher m)
      {
         base.WithCapabilities(m);
         return this;
      }
   }

   public sealed class AttachTargetMatcher : TargetMatcher
   {
      private readonly AttachExpectation expectation;

      public AttachTargetMatcher(AttachExpectation expectation)
      {
         this.expectation = expectation;
      }

      public AttachExpectation also()
      {
         return expectation;
      }

      public AttachExpectation and()
      {
         return expectation;
      }

      public override AttachTargetMatcher WithAddress(string name)
      {
         base.WithAddress(name);
         return this;
      }

      public override AttachTargetMatcher WithDurable(TerminusDurability durability)
      {
         base.WithDurable(durability);
         return this;
      }

      public override AttachTargetMatcher WithExpiryPolicy(TerminusExpiryPolicy expiry)
      {
         base.WithExpiryPolicy(expiry);
         return this;
      }

      public override AttachTargetMatcher WithTimeout(uint timeout)
      {
         base.WithTimeout(timeout);
         return this;
      }

      public override AttachTargetMatcher WithDefaultTimeout()
      {
         base.WithDefaultTimeout();
         return this;
      }

      public override AttachTargetMatcher WithDynamic(bool dynamic)
      {
         base.WithDynamic(dynamic);
         return this;
      }

      public override AttachTargetMatcher WithDynamicNodeProperties(IDictionary<string, object> properties)
      {
         base.WithDynamicNodeProperties(properties);
         return this;
      }

      public override AttachTargetMatcher WithCapabilities(params Symbol[] capabilities)
      {
         base.WithCapabilities(capabilities);
         return this;
      }

      public override AttachTargetMatcher WithCapabilities(params string[] capabilities)
      {
         base.WithCapabilities(capabilities);
         return this;
      }

      public override AttachTargetMatcher WithAddress(IMatcher m)
      {
         base.WithAddress(m);
         return this;
      }

      public override AttachTargetMatcher WithDurable(IMatcher m)
      {
         base.WithDurable(m);
         return this;
      }

      public override AttachTargetMatcher WithExpiryPolicy(IMatcher m)
      {
         base.WithExpiryPolicy(m);
         return this;
      }

      public override AttachTargetMatcher WithTimeout(IMatcher m)
      {
         base.WithTimeout(m);
         return this;
      }

      public override AttachTargetMatcher WithDynamic(IMatcher m)
      {
         base.WithDynamic(m);
         return this;
      }

      public override AttachTargetMatcher WithDynamicNodeProperties(IMatcher m)
      {
         base.WithDynamicNodeProperties(m);
         return this;
      }

      public override AttachTargetMatcher WithCapabilities(IMatcher m)
      {
         base.WithCapabilities(m);
         return this;
      }
   }

   public sealed class AttachCoordinatorMatcher : CoordinatorMatcher
   {
      private readonly AttachExpectation expectation;

      public AttachCoordinatorMatcher(AttachExpectation expectation)
      {
         this.expectation = expectation;
      }

      public AttachExpectation also()
      {
         return expectation;
      }

      public AttachExpectation and()
      {
         return expectation;
      }

      public override AttachCoordinatorMatcher WithCapabilities(params Symbol[] capabilities)
      {
         base.WithCapabilities(capabilities);
         return this;
      }

      public override AttachCoordinatorMatcher WithCapabilities(params string[] capabilities)
      {
         base.WithCapabilities(capabilities);
         return this;
      }
   }
}