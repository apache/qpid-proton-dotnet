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
using System.Collections;
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Header into a test script to
   /// drive the connect phase of the AMQP connection lifecycle.
   /// </summary>
   public sealed class AttachInjectAction : AbstractPerformativeInjectAction<Attach>
   {
      private readonly Attach attach = new();

      private bool explicitlyNullName;
      private bool explicitlyNullHandle;
      private bool nullSourceRequired;
      private bool nullTargetRequired;

      public AttachInjectAction(AMQPTestDriver driver) : base(driver)
      {
      }

      public override Attach Performative => attach;

      public AttachInjectAction WithName(string name)
      {
         explicitlyNullName = name == null;
         attach.Name = name;
         return this;
      }

      public AttachInjectAction WithHandle(uint handle)
      {
         attach.Handle = handle;
         return this;
      }

      public AttachInjectAction WithNullHandle()
      {
         attach.Handle = null;
         explicitlyNullHandle = true;
         return this;
      }

      public AttachInjectAction WithRole(bool role)
      {
         attach.Role = RoleExtension.Lookup(role);
         return this;
      }

      public AttachInjectAction WithRole(Role role)
      {
         attach.Role = role;
         return this;
      }

      public AttachInjectAction OfSender()
      {
         attach.Role = Role.Sender;
         return this;
      }

      public AttachInjectAction OfReceiver()
      {
         attach.Role = Role.Receiver;
         return this;
      }

      public AttachInjectAction WithSndSettleMode(SenderSettleMode sndSettleMode)
      {
         attach.SenderSettleMode = sndSettleMode;
         return this;
      }

      public AttachInjectAction WithSndSettleMode(byte? sndSettleMode)
      {
         if (sndSettleMode.HasValue)
         {
            attach.SenderSettleMode = (SenderSettleMode?)sndSettleMode;
         }

         return this;
      }

      public AttachInjectAction WithSenderSettleModeMixed()
      {
         attach.SenderSettleMode = SenderSettleMode.Mixed;
         return this;
      }

      public AttachInjectAction WithSenderSettleModeSettled()
      {
         attach.SenderSettleMode = SenderSettleMode.Settled;
         return this;
      }

      public AttachInjectAction WithSenderSettleModeUnsettled()
      {
         attach.SenderSettleMode = SenderSettleMode.Unsettled;
         return this;
      }

      public AttachInjectAction WithRcvSettleMode(ReceiverSettleMode rcvSettleMode)
      {
         attach.ReceiverSettleMode = rcvSettleMode;
         return this;
      }

      public AttachInjectAction WithRcvSettleMode(byte? rcvSettleMode)
      {
         if (rcvSettleMode.HasValue)
         {
            attach.ReceiverSettleMode = (ReceiverSettleMode?)rcvSettleMode;
         }

         return this;
      }

      public AttachInjectAction WithReceiverSettlesFirst()
      {
         attach.ReceiverSettleMode = ReceiverSettleMode.First;
         return this;
      }

      public AttachInjectAction WithReceiverSettlesSecond()
      {
         attach.ReceiverSettleMode = ReceiverSettleMode.Second;
         return this;
      }

      public bool IsNullSourceRequired => nullSourceRequired;

      public AttachInjectAction WithNullSource()
      {
         nullSourceRequired = true;
         attach.Source = null;
         return this;
      }

      public SourceBuilder WithSource()
      {
         nullSourceRequired = false;
         return new SourceBuilder(this, GetOrCreateSource());
      }

      public AttachInjectAction WithSource(Source source)
      {
         nullSourceRequired = source == null;
         attach.Source = source;
         return this;
      }

      public bool IsNullTargetRequired => nullTargetRequired;

      public AttachInjectAction WithNullTarget()
      {
         nullTargetRequired = true;
         attach.Target = null;
         return this;
      }

      public TargetBuilder WithTarget()
      {
         nullSourceRequired = false;
         return new TargetBuilder(this, GetOrCreateTarget());
      }

      public CoordinatorBuilder WithCoordinator()
      {
         nullSourceRequired = false;
         return new CoordinatorBuilder(this, GetOrCreateCoordinator());
      }

      public AttachInjectAction WithTarget(Target target)
      {
         nullTargetRequired = target == null;
         attach.Target = target;
         return this;
      }

      public AttachInjectAction WithTarget(Coordinator coordinator)
      {
         nullTargetRequired = coordinator == null;
         attach.Target = coordinator;
         return this;
      }

      public AttachInjectAction WithUnsettled(IDictionary<Binary, IDeliveryState> unsettled)
      {
         if (unsettled != null)
         {
            IDictionary<Binary, IDescribedType> converted = new Dictionary<Binary, IDescribedType>();
            foreach (KeyValuePair<Binary, IDeliveryState> entry in unsettled)
            {
               converted.Add(entry.Key, entry.Value);
            }

            attach.Unsettled = (IDictionary)converted;
         }
         return this;
      }

      public AttachInjectAction WithIncompleteUnsettled(bool incomplete)
      {
         attach.IncompleteUnsettled = incomplete;
         return this;
      }

      public AttachInjectAction WithInitialDeliveryCount(uint initialDeliveryCount)
      {
         attach.InitialDeliveryCount = initialDeliveryCount;
         return this;
      }

      public AttachInjectAction WithMaxMessageSize(ulong? maxMessageSize)
      {
         attach.MaxMessageSize = maxMessageSize;
         return this;
      }

      public AttachInjectAction WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         attach.OfferedCapabilities = TypeMapper.ToSymbolArray(offeredCapabilities);
         return this;
      }

      public AttachInjectAction WithOfferedCapabilities(Symbol[] offeredCapabilities)
      {
         attach.OfferedCapabilities = offeredCapabilities;
         return this;
      }

      public AttachInjectAction WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         attach.DesiredCapabilities = TypeMapper.ToSymbolArray(desiredCapabilities);
         return this;
      }

      public AttachInjectAction WithDesiredCapabilities(Symbol[] desiredCapabilities)
      {
         attach.DesiredCapabilities = desiredCapabilities;
         return this;
      }

      public AttachInjectAction WithPropertiesMap(IDictionary<string, object> properties)
      {
         attach.Properties = TypeMapper.ToSymbolKeyedMap(properties);
         return this;
      }

      public AttachInjectAction WithProperties(IDictionary<Symbol, object> properties)
      {
         attach.Properties = properties;
         return this;
      }

      #region Action implementation

      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         ushort localChannel;

         // A test that is trying to send an unsolicited attach must provide a channel as we
         // won't attempt to make up one since we aren't sure what the intent here is.
         if (channel == null)
         {
            if (driver.Sessions.LastLocallyOpenedSession == null)
            {
               throw new ScriptConfigurationError("Scripted Action cannot run without a configured channel: " +
                                                  "No locally opened session exists to auto select a channel.");
            }

            localChannel = (ushort)(channel = driver.Sessions.LastLocallyOpenedSession.LocalChannel);
         }
         else
         {
            localChannel = (ushort)channel;
         }

         SessionTracker session = driver.Sessions.SessionFromLocalChannel(localChannel);

         // A test might be trying to send Attach outside of session scope to check for error handling
         // of unexpected performatives so we just allow no session cases and send what we are told.
         if (session != null)
         {
            if (attach.Name == null && !explicitlyNullName)
            {
               attach.Name = Guid.NewGuid().ToString();
            }

            if (attach.Handle == null && !explicitlyNullHandle)
            {
               attach.Handle = session.FindFreeLocalHandle();
            }

            // Do not signal the session that we created a link if it carries an invalid null handle
            // as that would trigger other exceptions, just pass it on as the test is likely trying
            // to validate something specific.
            if (attach.Handle != null)
            {
               session.HandleLocalAttach(attach);
            }
         }
         else
         {
            if (attach.Handle == null && !explicitlyNullHandle)
            {
               throw new ScriptConfigurationError("Attach must carry a handle or have an explicitly set null handle.");
            }
         }
      }

      private Source GetOrCreateSource()
      {
         if (attach.Source == null)
         {
            attach.Source = new Source();
         }
         return attach.Source;
      }

      private Target GetOrCreateTarget()
      {
         if (attach.Target == null)
         {
            attach.Target = new Target();
         }
         return (Target)attach.Target;
      }

      private Coordinator GetOrCreateCoordinator()
      {
         if (attach.Target == null)
         {
            attach.Target = new Coordinator();
         }
         return (Coordinator)attach.Target;
      }

      #endregion

      #region Terminus Builder types that aid in the configuration

      public class TerminusBuilder
      {
         protected readonly AttachInjectAction attach;

         internal TerminusBuilder(AttachInjectAction attach)
         {
            this.attach = attach;
         }

         public AttachInjectAction Also()
         {
            return attach;
         }

         public AttachInjectAction And()
         {
            return attach;
         }
      }

      public sealed class SourceBuilder : TerminusBuilder
      {
         private readonly Source source;

         internal SourceBuilder(AttachInjectAction attach, Source source) : base(attach)
         {
            this.source = source;
         }

         public SourceBuilder WithAddress(string address)
         {
            source.Address = address;
            return this;
         }

         public SourceBuilder WithDurability(TerminusDurability durability)
         {
            source.Durable = ((uint)durability);
            return this;
         }

         public SourceBuilder WithExpiryPolicy(TerminusExpiryPolicy expiryPolicy)
         {
            source.ExpiryPolicy = expiryPolicy.ToSymbol();
            return this;
         }

         public SourceBuilder WithTimeout(uint timeout)
         {
            source.Timeout = timeout;
            return this;
         }

         public SourceBuilder WithDynamic(bool dynamic)
         {
            source.Dynamic = dynamic;
            return this;
         }

         public SourceBuilder WithDynamicNodePropertiesMap(IDictionary<Symbol, object> properties)
         {
            source.DynamicNodeProperties = (IDictionary)properties;
            return this;
         }

         public SourceBuilder WithDynamicNodeProperties(IDictionary<string, object> properties)
         {
            source.DynamicNodeProperties = (IDictionary)TypeMapper.ToSymbolKeyedMap(properties);
            return this;
         }

         public SourceBuilder WithDistributionMode(string mode)
         {
            source.DistributionMode = new Symbol(mode);
            return this;
         }

         public SourceBuilder WithDistributionMode(Symbol mode)
         {
            source.DistributionMode = mode;
            return this;
         }

         public SourceBuilder WithFilter(IDictionary<Symbol, object> filters)
         {
            source.Filter = (System.Collections.IDictionary)filters;
            return this;
         }

         public SourceBuilder WithFilterMap(IDictionary<string, object> filters)
         {
            source.Filter = (System.Collections.IDictionary)TypeMapper.ToSymbolKeyedMap(filters);
            return this;
         }

         public SourceBuilder WithDefaultOutcome(IOutcome outcome)
         {
            source.DefaultOutcome = (IDescribedType)outcome;
            return this;
         }

         public SourceBuilder WithOutcomes(params Symbol[] outcomes)
         {
            source.Outcomes = outcomes;
            return this;
         }

         public SourceBuilder WithOutcomes(params string[] outcomes)
         {
            source.Outcomes = TypeMapper.ToSymbolArray(outcomes);
            return this;
         }

         public SourceBuilder WithCapabilities(params Symbol[] capabilities)
         {
            source.Capabilities = capabilities;
            return this;
         }

         public SourceBuilder WithCapabilities(params string[] capabilities)
         {
            source.Capabilities = TypeMapper.ToSymbolArray(capabilities);
            return this;
         }
      }

      internal void WithRcvSettleMode(ReceiverSettleMode? receiverSettleMode)
      {
         throw new NotImplementedException();
      }

      public sealed class TargetBuilder : TerminusBuilder
      {
         private readonly Target target;

         internal TargetBuilder(AttachInjectAction attach, Target target) : base(attach)
         {
            this.target = target;
         }

         public TargetBuilder WithAddress(string address)
         {
            target.Address = address;
            return this;
         }

         public TargetBuilder WithDurability(TerminusDurability durability)
         {
            target.Durable = ((uint)durability);
            return this;
         }

         public TargetBuilder WithExpiryPolicy(TerminusExpiryPolicy expiryPolicy)
         {
            target.ExpiryPolicy = expiryPolicy.ToSymbol();
            return this;
         }

         public TargetBuilder WithTimeout(uint timeout)
         {
            target.Timeout = timeout;
            return this;
         }

         public TargetBuilder WithDynamic(bool dynamic)
         {
            target.Dynamic = dynamic;
            return this;
         }

         public TargetBuilder WithDynamicNodePropertiesMap(IDictionary<Symbol, object> properties)
         {
            target.DynamicNodeProperties = (System.Collections.IDictionary)properties;
            return this;
         }

         public TargetBuilder WithDynamicNodeProperties(IDictionary<string, object> properties)
         {
            target.DynamicNodeProperties = (System.Collections.IDictionary)TypeMapper.ToSymbolKeyedMap(properties);
            return this;
         }

         public TargetBuilder WithCapabilities(params string[] capabilities)
         {
            target.Capabilities = TypeMapper.ToSymbolArray(capabilities);
            return this;
         }

         public TargetBuilder WithCapabilities(params Symbol[] capabilities)
         {
            target.Capabilities = capabilities;
            return this;
         }
      }

      public sealed class CoordinatorBuilder : TerminusBuilder
      {
         private readonly Coordinator coordinator;

         internal CoordinatorBuilder(AttachInjectAction attach, Coordinator coordinator) : base(attach)
         {
            this.coordinator = coordinator;
         }

         public CoordinatorBuilder WithCapabilities(params string[] capabilities)
         {
            coordinator.Capabilities = TypeMapper.ToSymbolArray(capabilities);
            return this;
         }

         public CoordinatorBuilder WithCapabilities(params Symbol[] capabilities)
         {
            coordinator.Capabilities = capabilities;
            return this;
         }
      }

      #endregion
   }
}