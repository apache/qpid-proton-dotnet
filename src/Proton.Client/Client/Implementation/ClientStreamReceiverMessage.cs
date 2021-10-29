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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Codec.Decoders;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientStreamReceiverMessage : IStreamReceiverMessage
   {
      private readonly ClientStreamReceiver receiver;
      private readonly ClientStreamDelivery delivery;
      private readonly Stream deliveryStream;
      private readonly IIncomingDelivery protonDelivery;
      private readonly IStreamDecoder protonDecoder;
      private readonly IStreamDecoderState decoderState;

      private Header header;
      private DeliveryAnnotations deliveryAnnotations;
      private MessageAnnotations annotations;
      private Properties properties;
      private ApplicationProperties applicationProperties;
      private Footer footer;

      private StreamState currentState = StreamState.IDLE;
      private Stream bodyStream; // TODO temporary variable definition
      // TODO private MessageBodyInputStream bodyStream;

      internal ClientStreamReceiverMessage(ClientStreamReceiver receiver, ClientStreamDelivery delivery, Stream deliveryStream)
      {
         this.receiver = receiver;
         this.delivery = delivery;
         this.deliveryStream = deliveryStream;
         this.protonDelivery = delivery.ProtonDelivery;

         this.protonDecoder = ProtonStreamDecoderFactory.Create();
         this.decoderState = protonDecoder.NewDecoderState();
      }

      public IStreamDelivery Delivery => delivery;

      public IStreamReceiver Receiver => receiver;

      public bool Aborted => protonDelivery?.IsAborted ?? false;

      public bool Completed => (!protonDelivery?.IsPartial ?? false) && (!protonDelivery?.IsAborted ?? false);

      public uint MessageFormat
      {
         get => protonDelivery?.MessageFormat ?? 0;
         set => new ClientUnsupportedOperationException("Cannot write to a StreamReceiverMessage");
      }

      public IProtonBuffer Encode(IDictionary<string, object> deliveryAnnotations)
      {
         throw new ClientUnsupportedOperationException("Cannot encode from an StreamReceiverMessage instance.");
      }

      #region AMQP Header Access API

      public Header Header
      {
         get => EnsureStreamDecodedTo(StreamState.HEADER_READ).header;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool Durable
      {
         get => Header?.Durable ?? Header.DEFAULT_DURABILITY;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public byte Priority
      {
         get => Header?.Priority ?? Header.DEFAULT_PRIORITY;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public uint TimeToLive
      {
         get => Header?.TimeToLive ?? Header.DEFAULT_TIME_TO_LIVE;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool FirstAcquirer
      {
         get => Header?.FirstAcquirer ?? Header.DEFAULT_FIRST_ACQUIRER;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public uint DeliveryCount
      {
         get => Header?.DeliveryCount ?? Header.DEFAULT_DELIVERY_COUNT;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      #endregion

      #region AMQP Header Access API

      public Properties Properties
      {
         get => EnsureStreamDecodedTo(StreamState.PROPERTIES_READ).properties;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public object MessageId
      {
         get => Properties?.MessageId;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public byte[] UserId
      {
         get
         {
            byte[] userId = null;
            if (Properties != null && Properties.UserId != null)
            {
               userId = new byte[Properties.UserId.ReadableBytes];
               Properties.UserId.CopyInto(Properties.UserId.ReadOffset, userId, 0, userId.LongLength);
            }

            return userId;
         }
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public string To
      {
         get => Properties?.To;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public string Subject
      {
         get => Properties?.Subject;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public string ReplyTo
      {
         get => Properties?.ReplyTo;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public object CorrelationId
      {
         get => Properties?.CorrelationId;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public string ContentType
      {
         get => Properties?.ContentType;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public string ContentEncoding
      {
         get => Properties?.ContentEncoding;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public ulong AbsoluteExpiryTime
      {
         get => Properties?.AbsoluteExpiryTime ?? 0;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public ulong CreationTime
      {
         get => Properties?.CreationTime ?? 0;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public string GroupId
      {
         get => Properties?.GroupId;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public uint GroupSequence
      {
         get => Properties?.GroupSequence ?? 0;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public string ReplyToGroupId
      {
         get => Properties?.ReplyToGroupId;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      #endregion

      #region Internal Delivery Annotations access for use by the managing delivery object

      internal DeliveryAnnotations DeliveryAnnotations => EnsureStreamDecodedTo(StreamState.DELIVERY_ANNOTATIONS_READ).deliveryAnnotations;

      #endregion

      #region AMQP Message Annotations Access API

      public MessageAnnotations Annotations
      {
         get => EnsureStreamDecodedTo(StreamState.MESSAGE_ANNOTATIONS_READ).annotations;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool HasAnnotations
      {
         get
         {
            EnsureStreamDecodedTo(StreamState.MESSAGE_ANNOTATIONS_READ);
            return annotations?.Value?.Count > 0;
         }
      }

      public IMessage<Stream> ForEachAnnotation(Action<string, object> consumer)
      {
         if (HasAnnotations)
         {
            foreach (KeyValuePair<Symbol, object> entry in annotations.Value)
            {
               consumer.Invoke(entry.Key.ToString(), entry.Value);
            }
         }

         return this;
      }

      public object GetAnnotation(string key)
      {
         object result = null;
         Annotations?.Value?.TryGetValue(Symbol.Lookup(key), out result);
         return result;
      }

      public IMessage<Stream> SetAnnotation(string key, object value)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool HasAnnotation(string key)
      {
         return Annotations?.Value?.ContainsKey(Symbol.Lookup(key)) ?? false;
      }

      public object RemoveAnnotation(string key)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      #endregion

      #region AMQP Application Properties Access API

      public ApplicationProperties ApplicationProperties
      {
         get => EnsureStreamDecodedTo(StreamState.APPLICATION_PROPERTIES_READ).applicationProperties;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool HasProperties
      {
         get
         {
            EnsureStreamDecodedTo(StreamState.APPLICATION_PROPERTIES_READ);
            return applicationProperties?.Value?.Count > 0;
         }
      }

      public IMessage<Stream> ForEachProperty(Action<string, object> consumer)
      {
         if (HasProperties)
         {
            foreach (KeyValuePair<string, object> entry in applicationProperties.Value)
            {
               consumer.Invoke(entry.Key, entry.Value);
            }
         }

         return this;
      }

      public object GetProperty(string key)
      {
         object result = null;
         ApplicationProperties?.Value?.TryGetValue(key, out result);
         return result;
      }

      public IMessage<Stream> SetProperty(string key, object value)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool HasProperty(string key)
      {
         return ApplicationProperties?.Value?.ContainsKey(key) ?? false;
      }

      public object RemoveProperty(string key)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      #endregion

      #region AMQP Footer Access API

      public Footer Footer
      {
         get => EnsureStreamDecodedTo(StreamState.FOOTER_READ).footer;
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool HasFooters
      {
         get
         {
            EnsureStreamDecodedTo(StreamState.BODY_READABLE);

            if (currentState != StreamState.FOOTER_READ)
            {
               if (currentState == StreamState.DECODE_ERROR)
               {
                  throw new ClientException("Cannot read Footer due to decoding error in message payload");
               }
               else
               {
                  throw new ClientIllegalStateException("Cannot read message Footer until message body fully read");
               }
            }

            return footer?.Value?.Count > 0;
         }
      }

      public IMessage<Stream> ForEachFooter(Action<string, object> consumer)
      {
         if (HasFooters)
         {
            foreach (KeyValuePair<Symbol, object> entry in footer.Value)
            {
               consumer.Invoke(entry.Key.ToString(), entry.Value);
            }
         }

         return this;
      }

      public object GetFooter(string key)
      {
         object result = null;
         Footer?.Value?.TryGetValue(Symbol.Lookup(key), out result);
         return result;
      }

      public IMessage<Stream> SetFooter(string key, object value)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public bool HasFooter(string key)
      {
         return Footer?.Value?.ContainsKey(Symbol.Lookup(key)) ?? false;
      }

      public object RemoveFooter(string key)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      #endregion

      #region AMQP Message body access API

      public Stream Body
      {
         get
         {
            if (currentState > StreamState.BODY_READABLE)
            {
               if (currentState == StreamState.DECODE_ERROR)
               {
                  throw new ClientException("Cannot read body due to decoding error in message payload");
               }
               else if (bodyStream != null)
               {
                  throw new ClientIllegalStateException("Cannot read body from message whose body has already been read.");
               }
            }

            EnsureStreamDecodedTo(StreamState.BODY_READABLE);

            return bodyStream;
         }
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public IAdvancedMessage<Stream> ForEachBodySection(Action<ISection> consumer)
      {
         throw new ClientUnsupportedOperationException("Cannot iterate all body sections from a StreamReceiverMessage instance.");
      }

      public IAdvancedMessage<Stream> AddBodySection(ISection section)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public IAdvancedMessage<Stream> ClearBodySections()
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      public IEnumerable<ISection> GetBodySections()
      {
         throw new ClientUnsupportedOperationException("Cannot iterate all body sections from a StreamReceiverMessage instance.");
      }

      public IAdvancedMessage<Stream> SetBodySections(IEnumerable<ISection> section)
      {
         throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiveMessage");
      }

      #endregion

      #region Private stream receiver message implementation

      /// <summary>
      /// Used to locate where in the stream read precess the message is currently
      /// </summary>
      private enum StreamState
      {
         IDLE,
         HEADER_READ,
         DELIVERY_ANNOTATIONS_READ,
         MESSAGE_ANNOTATIONS_READ,
         PROPERTIES_READ,
         APPLICATION_PROPERTIES_READ,
         BODY_PENDING,
         BODY_READABLE,
         FOOTER_READ,
         DECODE_ERROR
      }

      private void CheckClosedOrAborted()
      {
         if (receiver.IsClosed)
         {
            throw new ClientIllegalStateException("The parent Receiver instance has already been closed.");
         }

         if (Aborted)
         {
            throw new ClientIllegalStateException("The incoming delivery was aborted.");
         }
      }

      private ClientStreamReceiverMessage EnsureStreamDecodedTo(StreamState desiredState)
      {
         CheckClosedOrAborted();

         while (currentState < desiredState)
         {
            // TODO
         }

         return this;
      }

      #endregion
   }
}