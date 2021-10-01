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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec;
using Apache.Qpid.Proton.Test.Driver.Codec.Impl;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP performative into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public class TransferInjectAction : AbstractPerformativeInjectAction<Transfer>
   {
      private readonly Transfer transfer = new Transfer();
      private readonly DeliveryStateBuilder stateBuilder;

      private byte[] payload;

      private Header header;
      private DeliveryAnnotations deliveryAnnotations;
      private MessageAnnotations messageAnnotations;
      private Properties properties;
      private ApplicationProperties applicationProperties;
      private IDescribedType body;
      private Footer footer;

      private bool explicitlyNullDeliveryTag;

      public TransferInjectAction(AMQPTestDriver driver) : base(driver)
      {
         this.stateBuilder = new DeliveryStateBuilder(this);
      }

      public override Transfer Performative => transfer;

      public override byte[] Payload => payload ?? EncodePayload();

      public virtual TransferInjectAction WithHandle(uint handle)
      {
         transfer.Handle = handle;
         return this;
      }

      public virtual TransferInjectAction WithDeliveryId(uint deliveryId)
      {
         transfer.DeliveryId = deliveryId;
         return this;
      }

      public virtual TransferInjectAction WithDeliveryTag(byte[] deliveryTag)
      {
         explicitlyNullDeliveryTag = deliveryTag == null;
         transfer.DeliveryTag = new Binary(deliveryTag);
         return this;
      }

      public virtual TransferInjectAction WithDeliveryTag(Binary deliveryTag)
      {
         explicitlyNullDeliveryTag = deliveryTag == null;
         transfer.DeliveryTag = deliveryTag;
         return this;
      }

      public virtual TransferInjectAction WithNullDeliveryTag()
      {
         explicitlyNullDeliveryTag = true;
         transfer.DeliveryTag = null;
         return this;
      }

      public virtual TransferInjectAction WithMessageFormat(uint messageFormat)
      {
         transfer.MessageFormat = messageFormat;
         return this;
      }

      public virtual TransferInjectAction WithSettled(bool settled)
      {
         transfer.Settled = settled;
         return this;
      }

      public virtual TransferInjectAction WithMore(bool more)
      {
         transfer.More = more;
         return this;
      }

      public virtual TransferInjectAction WithRcvSettleMode(ReceiverSettleMode rcvSettleMode)
      {
         transfer.ReceiverSettleMode = rcvSettleMode;
         return this;
      }

      public virtual TransferInjectAction WithState(IDeliveryState state)
      {
         transfer.State = state;
         return this;
      }

      public DeliveryStateBuilder WithState()
      {
         return stateBuilder;
      }

      public virtual TransferInjectAction WithResume(bool resume)
      {
         transfer.Resume = resume;
         return this;
      }

      public virtual TransferInjectAction WithAborted(bool aborted)
      {
         transfer.Aborted = aborted;
         return this;
      }

      public virtual TransferInjectAction WithBatchable(bool batchable)
      {
         transfer.Batchable = batchable;
         return this;
      }

      public virtual TransferInjectAction WithPayload(byte[] payload)
      {
         this.payload = payload;
         return this;
      }

      public HeaderBuilder WithHeader()
      {
         return new HeaderBuilder(this);
      }

      public DeliveryAnnotationsBuilder WithDeliveryAnnotations()
      {
         return new DeliveryAnnotationsBuilder(this);
      }

      public MessageAnnotationsBuilder WithMessageAnnotations()
      {
         return new MessageAnnotationsBuilder(this);
      }

      public PropertiesBuilder WithProperties()
      {
         return new PropertiesBuilder(this);
      }

      public ApplicationPropertiesBuilder WithApplicationProperties()
      {
         return new ApplicationPropertiesBuilder(this);
      }

      public BodySectionBuilder WithBody()
      {
         return new BodySectionBuilder(this);
      }

      public FooterBuilder WithFooter()
      {
         return new FooterBuilder(this);
      }

      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         // We fill in a channel using the next available channel id if one isn't set, then
         // report the outbound begin to the session so it can track this new session.
         if (channel == null)
         {
            channel = driver.Sessions.LastLocallyOpenedSession.LocalChannel;
         }

         // Auto select last opened receiver on last opened session.  Later an option could
         // be added to allow forcing the handle to be null for testing specification requirements.
         if (transfer.Handle == null)
         {
            transfer.Handle = driver.Sessions.LastLocallyOpenedSession.LastOpenedSender.Handle;
         }

         SessionTracker session = driver.Sessions.SessionFromLocalChannel((ushort)channel);

         if (transfer.DeliveryTag == null && !explicitlyNullDeliveryTag)
         {
            transfer.DeliveryTag = GenerateUniqueDeliveryTag();
         }

         // A test might be trying to send Transfer outside of session scope to check for error handling
         // of unexpected performatives so we just allow no session cases and send what we are told.
         if (session != null)
         {
            // Here we could check if the delivery Id is set and if not grab a valid
            // next Id from the driver as well as checking for a session and using last
            // created one if none set.

            session.HandleLocalTransfer(transfer, Payload);
         }
      }

      private static Binary GenerateUniqueDeliveryTag()
      {
         return new Binary(Guid.NewGuid().ToByteArray());
      }

      private Header GetOrCreateHeader()
      {
         if (header == null)
         {
            header = new Header();
         }
         return header;
      }

      private DeliveryAnnotations GetOrCreateDeliveryAnnotations()
      {
         if (deliveryAnnotations == null)
         {
            deliveryAnnotations = new DeliveryAnnotations();
         }
         return deliveryAnnotations;
      }

      private MessageAnnotations GetOrCreateMessageAnnotations()
      {
         if (messageAnnotations == null)
         {
            messageAnnotations = new MessageAnnotations();
         }
         return messageAnnotations;
      }

      private Properties GetOrCreateProperties()
      {
         if (properties == null)
         {
            properties = new Properties();
         }
         return properties;
      }

      private ApplicationProperties GetOrCreateApplicationProperties()
      {
         if (applicationProperties == null)
         {
            applicationProperties = new ApplicationProperties();
         }
         return applicationProperties;
      }

      private Footer GetOrCreateFooter()
      {
         if (footer == null)
         {
            footer = new Footer();
         }
         return footer;
      }

      private byte[] EncodePayload()
      {
         ICodec codec = CodecFactory.Create();
         MemoryStream encoding = new MemoryStream();

         if (header != null)
         {
            codec.PutDescribedType(header);
         }
         if (deliveryAnnotations != null)
         {
            codec.PutDescribedType(deliveryAnnotations);
         }
         if (messageAnnotations != null)
         {
            codec.PutDescribedType(messageAnnotations);
         }
         if (properties != null)
         {
            codec.PutDescribedType(properties);
         }
         if (applicationProperties != null)
         {
            codec.PutDescribedType(applicationProperties);
         }
         if (body != null)
         {
            codec.PutDescribedType(body);
         }
         if (footer != null)
         {
            codec.PutDescribedType(footer);
         }

         codec.Encode(encoding);

         return encoding.ToArray();
      }

      #region Builders for AMQP message body sections

      public abstract class SectionBuilder
      {
         protected readonly TransferInjectAction action;

         public SectionBuilder(TransferInjectAction action)
         {
            this.action = action;
         }

         public TransferInjectAction Also()
         {
            return action;
         }
      }

      public sealed class HeaderBuilder : SectionBuilder
      {
         public HeaderBuilder(TransferInjectAction action) : base(action)
         {
         }

         public HeaderBuilder WithDurability(bool durable)
         {
            action.GetOrCreateHeader().Durable = durable;
            return this;
         }

         public HeaderBuilder WithPriority(byte priority)
         {
            action.GetOrCreateHeader().Priority = priority;
            return this;
         }

         public HeaderBuilder WithTimeToLive(uint ttl)
         {
            action.GetOrCreateHeader().Ttl = ttl;
            return this;
         }

         public HeaderBuilder WithFirstAcquirer(bool first)
         {
            action.GetOrCreateHeader().FirstAcquirer = first;
            return this;
         }

         public HeaderBuilder WithDeliveryCount(uint count)
         {
            action.GetOrCreateHeader().DeliveryCount = count;
            return this;
         }
      }

      public sealed class DeliveryAnnotationsBuilder : SectionBuilder
      {
         public DeliveryAnnotationsBuilder(TransferInjectAction action) : base(action)
         {
         }

         public DeliveryAnnotationsBuilder WithAnnotation(string key, object value)
         {
            action.GetOrCreateDeliveryAnnotations().AddSymbolKeyedAnnotation(key, value);
            return this;
         }

         public DeliveryAnnotationsBuilder WithAnnotation(Symbol key, object value)
         {
            action.GetOrCreateDeliveryAnnotations().AddSymbolKeyedAnnotation(key, value);
            return this;
         }
      }

      public sealed class MessageAnnotationsBuilder : SectionBuilder
      {
         public MessageAnnotationsBuilder(TransferInjectAction action) : base(action)
         {
         }

         public MessageAnnotationsBuilder WithAnnotation(String key, Object value)
         {
            action.GetOrCreateMessageAnnotations().AddSymbolKeyedAnnotation(key, value);
            return this;
         }

         public MessageAnnotationsBuilder WithAnnotation(Symbol key, Object value)
         {
            action.GetOrCreateMessageAnnotations().AddSymbolKeyedAnnotation(key, value);
            return this;
         }
      }

      public sealed class PropertiesBuilder : SectionBuilder
      {
         public PropertiesBuilder(TransferInjectAction action) : base(action)
         {
         }

         public PropertiesBuilder WithMessageId(object value)
         {
            action.GetOrCreateProperties().MessageId = value;
            return this;
         }

         public PropertiesBuilder WithUserID(Binary value)
         {
            action.GetOrCreateProperties().UserId = value;
            return this;
         }

         public PropertiesBuilder WithTo(string value)
         {
            action.GetOrCreateProperties().To = value;
            return this;
         }

         public PropertiesBuilder WithSubject(string value)
         {
            action.GetOrCreateProperties().Subject = value;
            return this;
         }

         public PropertiesBuilder WithReplyTp(string value)
         {
            action.GetOrCreateProperties().ReplyTo = value;
            return this;
         }

         public PropertiesBuilder WithCorrelationId(object value)
         {
            action.GetOrCreateProperties().CorrelationId = value;
            return this;
         }

         public PropertiesBuilder WithContentType(string value)
         {
            action.GetOrCreateProperties().ContentType = new Symbol(value);
            return this;
         }

         public PropertiesBuilder WithContentType(Symbol value)
         {
            action.GetOrCreateProperties().ContentType = value;
            return this;
         }

         public PropertiesBuilder WithContentEncoding(string value)
         {
            action.GetOrCreateProperties().ContentEncoding = new Symbol(value);
            return this;
         }

         public PropertiesBuilder WithContentEncoding(Symbol value)
         {
            action.GetOrCreateProperties().ContentEncoding = value;
            return this;
         }

         public PropertiesBuilder WithAbsoluteExpiryTime(ulong value)
         {
            action.GetOrCreateProperties().AbsoluteExpiryTime = value;
            return this;
         }

         public PropertiesBuilder WithCreationTime(ulong value)
         {
            action.GetOrCreateProperties().CreationTime = value;
            return this;
         }

         public PropertiesBuilder WithGroupId(string value)
         {
            action.GetOrCreateProperties().GroupId = value;
            return this;
         }

         public PropertiesBuilder WithGroupSequence(uint value)
         {
            action.GetOrCreateProperties().GroupSequence = value;
            return this;
         }

         public PropertiesBuilder WithReplyToGroupId(string value)
         {
            action.GetOrCreateProperties().ReplyToGroupId = value;
            return this;
         }
      }

      public sealed class ApplicationPropertiesBuilder : SectionBuilder
      {
         public ApplicationPropertiesBuilder(TransferInjectAction action) : base(action)
         {
         }

         public ApplicationPropertiesBuilder WithApplicationProperty(string key, object value)
         {
            action.GetOrCreateApplicationProperties().AddApplicationProperty(key, value);
            return this;
         }
      }

      public sealed class FooterBuilder : SectionBuilder
      {
         public FooterBuilder(TransferInjectAction action) : base(action)
         {
         }

         public FooterBuilder WithFooter(Symbol key, object value)
         {
            action.GetOrCreateFooter().AddFooterProperty(key, value);
            return this;
         }
      }

      public sealed class BodySectionBuilder : SectionBuilder
      {
         public BodySectionBuilder(TransferInjectAction action) : base(action)
         {
         }

         public BodySectionBuilder WithString(String body)
         {
            action.body = new AmqpValue(body);
            return this;
         }

         public BodySectionBuilder WithValue(String body)
         {
            action.body = new AmqpValue(body);
            return this;
         }

         public BodySectionBuilder WithValue(byte[] body)
         {
            action.body = new AmqpValue(new Binary(body));
            return this;
         }

         public BodySectionBuilder WithValue(Binary body)
         {
            action.body = new Data(body);
            return this;
         }

         public BodySectionBuilder WithData(byte[] body)
         {
            action.body = new Data(new Binary(body));
            return this;
         }

         public BodySectionBuilder WithData(Binary body)
         {
            action.body = new Data(body);
            return this;
         }

         public BodySectionBuilder WithSequence(IList sequence)
         {
            action.body = new AmqpSequence(sequence);
            return this;
         }

         public BodySectionBuilder WithDescribed(IDescribedType described)
         {
            action.body = new AmqpValue(described);
            return this;
         }
      }

      #endregion

      #region Builders for Delivery State types

      public sealed class DeliveryStateBuilder
      {
         private readonly TransferInjectAction action;

         public DeliveryStateBuilder(TransferInjectAction action)
         {
            this.action = action;
         }

         public TransferInjectAction Accepted()
         {
            return action.WithState(new Accepted());
         }

         public TransferInjectAction Released()
         {
            return action.WithState(new Released());
         }

         public TransferInjectAction Rejected()
         {
            return action.WithState(new Rejected());
         }

         public TransferInjectAction Rejected(String condition, String description)
         {
            Rejected rejected = new Rejected();
            rejected.Error = new ErrorCondition(new Symbol(condition), description);

            return action.WithState(rejected);
         }

         public TransferInjectAction Modified()
         {
            return action.WithState(new Modified());
         }

         public TransferInjectAction modified(bool failed)
         {
            Modified modified = new Modified();
            modified.DeliveryFailed = failed;

            return action.WithState(modified);
         }

         public TransferInjectAction modified(bool failed, bool undeliverableHere)
         {
            Modified modified = new Modified();
            modified.DeliveryFailed = failed;
            modified.UndeliverableHere = undeliverableHere;

            return action.WithState(modified);
         }

         public TransactionalStateBuilder transactional()
         {
            TransactionalStateBuilder builder = new TransactionalStateBuilder(action);
            action.WithState(builder.State());
            return builder;
         }
      }

      public sealed class TransactionalStateBuilder
      {
         private readonly TransferInjectAction action;
         private readonly TransactionalState state = new TransactionalState();

         public TransactionalStateBuilder(TransferInjectAction action)
         {
            this.action = action;
         }

         public TransactionalState State()
         {
            return state;
         }

         public TransferInjectAction Also()
         {
            return action;
         }

         public TransferInjectAction And()
         {
            return action;
         }

         public TransactionalStateBuilder WithTxnId(byte[] txnId)
         {
            state.TxnId = new Binary(txnId);
            return this;
         }

         public TransactionalStateBuilder WithTxnId(Binary txnId)
         {
            state.TxnId = txnId;
            return this;
         }

         public TransactionalStateBuilder WithOutcome(IOutcome outcome)
         {
            state.Outcome = outcome;
            return this;
         }

         public TransactionalStateBuilder WithAccepted()
         {
            return WithOutcome(new Accepted());
         }

         public TransactionalStateBuilder WithReleased()
         {
            return WithOutcome(new Released());
         }

         public TransactionalStateBuilder WithRejected()
         {
            return WithOutcome(new Rejected());
         }

         public TransactionalStateBuilder WithRejected(String condition, String description)
         {
            Rejected rejected = new Rejected();
            rejected.Error = new ErrorCondition(new Symbol(condition), description);

            return WithOutcome(rejected);
         }

         public TransactionalStateBuilder WithModified()
         {
            return WithOutcome(new Modified());
         }

         public TransactionalStateBuilder WithModified(bool failed)
         {
            Modified modified = new Modified();
            modified.DeliveryFailed = failed;

            return WithOutcome(modified);
         }

         public TransactionalStateBuilder WithModified(bool failed, bool undeliverableHere)
         {
            Modified modified = new Modified();
            modified.DeliveryFailed = failed;
            modified.UndeliverableHere = undeliverableHere;

            return WithOutcome(modified);
         }
      }

      #endregion
   }
}