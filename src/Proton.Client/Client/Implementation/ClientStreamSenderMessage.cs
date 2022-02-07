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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientStreamSenderMessage : IStreamSenderMessage
   {
      private readonly ClientStreamSender sender;
      private readonly DeliveryAnnotations deliveryAnnotations;
      private readonly uint writeBufferSize;
      private readonly IAdvancedMessage<object> streamMessagePacket; // TODO Temporary variable
      // TODO private readonly StreamMessagePacket streamMessagePacket = new StreamMessagePacket();
      private readonly ClientStreamTracker tracker;

      private Header header;
      private MessageAnnotations annotations;
      private Properties properties;
      private ApplicationProperties applicationProperties;
      private Footer footer;

      private IProtonBuffer buffer;
      private volatile uint messageFormat;
      private StreamState currentState = StreamState.PREAMBLE;

      internal ClientStreamSenderMessage(ClientStreamSender sender, ClientStreamTracker tracker, DeliveryAnnotations deliveryAnnotations)
      {
         this.sender = sender;
         this.deliveryAnnotations = deliveryAnnotations;
         this.tracker = tracker;

         if (sender.Options.WriteBufferSize > 0)
         {
            writeBufferSize = Math.Max(StreamSenderOptions.MIN_BUFFER_SIZE_LIMIT, sender.Options.WriteBufferSize);
         }
         else
         {
            writeBufferSize = Math.Max(StreamSenderOptions.MIN_BUFFER_SIZE_LIMIT,
                                       (uint)sender.ProtonSender.Connection.MaxFrameSize);
         }
      }

      internal Engine.IOutgoingDelivery ProtonDelivery => tracker.ProtonDelivery;

      public IStreamTracker Tracker => tracker;

      public IStreamSender Sender => sender;

      public uint MessageFormat
      {
         get => messageFormat;
         set
         {
            if (currentState != StreamState.PREAMBLE)
            {
               throw new ClientIllegalStateException("Cannot set message format after body writes have started.");
            }

            this.messageFormat = value;
         }
      }

      public bool Completed => currentState == StreamState.COMPLETE;

      public bool Aborted => currentState == StreamState.ABORTED;

      public IStreamSenderMessage Abort()
      {
         if (Completed)
         {
            throw new ClientIllegalStateException("Cannot abort an already completed send context");
         }

         if (!Aborted)
         {
            currentState = StreamState.ABORTED;
            sender.Abort(ProtonDelivery, tracker);
         }

         return this;
      }

      public IStreamSenderMessage Complete()
      {
         if (Aborted)
         {
            throw new ClientIllegalStateException("Cannot complete an already aborted send context");
         }

         if (!Completed)
         {
            // This may result in completion if the write surpasses the buffer limit but we still
            // need to check in case it does not, or if there are no footers...
            if (footer != null)
            {
               Write(footer);
            }

            currentState = StreamState.COMPLETE;

            // If there is buffered data we can flush and complete in one Transfer
            // frame otherwise we only need to do work if there was ever a send on
            // this context which would imply we have a Tracker and a Delivery.
            if (buffer != null && buffer.IsReadable)
            {
               DoFlush();
            }
            else
            {
               sender.Complete(ProtonDelivery, tracker);
            }
         }

         return this;
      }

      public IProtonBuffer Encode(IDictionary<string, object> deliveryAnnotations)
      {
         throw new ClientUnsupportedOperationException("StreamSenderMessage cannot be directly encoded");
      }

      public Stream RawOutputStream()
      {
         throw new NotImplementedException();
      }

      #region AMQP Header access APIs

      public Header Header
      {
         get => header;
         set
         {
            CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Header after body writing has started.");
            this.header = value;
         }
      }

      public bool Durable
      {
         get => header?.Durable ?? Header.DEFAULT_DURABILITY;
         set => LazyCreateHeader().Durable = value;
      }

      public byte Priority
      {
         get => header?.Priority ?? Header.DEFAULT_PRIORITY;
         set => LazyCreateHeader().Priority = value;
      }

      public uint TimeToLive
      {
         get => header?.TimeToLive ?? Header.DEFAULT_TIME_TO_LIVE;
         set => LazyCreateHeader().TimeToLive = value;
      }

      public bool FirstAcquirer
      {
         get => header?.FirstAcquirer ?? Header.DEFAULT_FIRST_ACQUIRER;
         set => LazyCreateHeader().FirstAcquirer = value;
      }

      public uint DeliveryCount
      {
         get => header?.DeliveryCount ?? Header.DEFAULT_DELIVERY_COUNT;
         set => LazyCreateHeader().DeliveryCount = value;
      }

      #endregion

      #region AMQP Properties access APIs

      public Properties Properties
      {
         get => properties;
         set
         {
            CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Properties after body writing has started.");
            this.properties = value;
         }
      }

      public object MessageId
      {
         get => properties?.MessageId;
         set => LazyCreateProperties().MessageId = value;
      }

      public byte[] UserId
      {
         get
         {
            byte[] result = null;
            if (properties?.UserId?.ReadableBytes > 0)
            {
               result = new byte[properties.UserId.ReadableBytes];
               properties.UserId.CopyInto(properties.UserId.ReadOffset, result, 0, result.LongLength);
            }

            return result;
         }
         set
         {
            LazyCreateProperties().UserId = ProtonByteBufferAllocator.Instance.Wrap(value);
         }
      }

      public string To
      {
         get => properties?.To;
         set => LazyCreateProperties().To = value;
      }

      public string Subject
      {
         get => properties?.Subject;
         set => LazyCreateProperties().Subject = value;
      }

      public string ReplyTo
      {
         get => properties?.ReplyTo;
         set => LazyCreateProperties().ReplyTo = value;
      }

      public object CorrelationId
      {
         get => properties?.CorrelationId;
         set => LazyCreateProperties().CorrelationId = value;
      }

      public string ContentType
      {
         get => properties?.ContentType;
         set => LazyCreateProperties().ContentType = value;
      }

      public string ContentEncoding
      {
         get => properties?.ContentEncoding;
         set => LazyCreateProperties().ContentEncoding = value;
      }

      public ulong AbsoluteExpiryTime
      {
         get => properties?.AbsoluteExpiryTime ?? 0;
         set => LazyCreateProperties().AbsoluteExpiryTime = value;
      }

      public ulong CreationTime
      {
         get => properties?.CreationTime ?? 0;
         set => LazyCreateProperties().CreationTime = value;
      }

      public string GroupId
      {
         get => properties?.GroupId;
         set => LazyCreateProperties().GroupId = value;
      }

      public uint GroupSequence
      {
         get => properties?.GroupSequence ?? 0;
         set => LazyCreateProperties().GroupSequence = value;
      }

      public string ReplyToGroupId
      {
         get => properties?.ReplyToGroupId;
         set => LazyCreateProperties().ReplyToGroupId = value;
      }

      #endregion

      #region AMQP Message Annotations access APIs

      public MessageAnnotations Annotations
      {
         get => annotations;
         set
         {
            CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Annotations after body writing has started.");
            this.annotations = value;
         }
      }

      public bool HasAnnotations => annotations?.Value?.Count > 0;

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
         LazyCreateMessageAnnotations().Value[Symbol.Lookup(key)] = value;
         return this;
      }

      public bool HasAnnotation(string key)
      {
         return annotations?.Value?.ContainsKey(Symbol.Lookup(key)) ?? false;
      }

      public object RemoveAnnotation(string key)
      {
         object oldValue = null;

         if (HasAnnotations)
         {
            annotations.Value.TryGetValue(Symbol.Lookup(key), out oldValue);
            annotations.Value.Remove(Symbol.Lookup(key));
         }

         return oldValue;
      }

      #endregion

      #region AMQP Application Properties access APIs

      public ApplicationProperties ApplicationProperties
      {
         get => applicationProperties;
         set
         {
            CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Application Properties after body writing has started.");
            this.applicationProperties = value;
         }
      }

      public bool HasProperties => applicationProperties?.Value?.Count > 0;

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
         applicationProperties?.Value?.TryGetValue(key, out result);
         return result;
      }

      public IMessage<Stream> SetProperty(string key, object value)
      {
         LazyCreateApplicationProperties().Value[key] = value;
         return this;
      }

      public bool HasProperty(string key)
      {
         return applicationProperties?.Value?.ContainsKey(key) ?? false;
      }

      public object RemoveProperty(string key)
      {
         object oldValue = null;

         if (HasProperties)
         {
            applicationProperties.Value.TryGetValue(key, out oldValue);
            applicationProperties.Value.Remove(key);
         }

         return oldValue;
      }

      #endregion

      #region AMQP Footer access APIs

      public Footer Footer
      {
         get => footer;
         set
         {
            if (currentState >= StreamState.COMPLETE)
            {
               throw new ClientIllegalStateException(
                   "Cannot write to Message Footer after message has been marked completed or aborted.");
            }
            this.footer = value;
         }
      }

      public bool HasFooters => footer?.Value?.Count > 0;

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
         footer?.Value?.TryGetValue(Symbol.Lookup(key), out result);
         return result;
      }

      public IMessage<Stream> SetFooter(string key, object value)
      {
         LazyCreateFooter().Value[Symbol.Lookup(key)] = value;
         return this;
      }

      public bool HasFooter(string key)
      {
         return footer?.Value?.ContainsKey(Symbol.Lookup(key)) ?? false;
      }

      public object RemoveFooter(string key)
      {
         object oldValue = null;

         if (HasFooters)
         {
            footer.Value.TryGetValue(Symbol.Lookup(key), out oldValue);
            footer.Value.Remove(Symbol.Lookup(key));
         }

         return oldValue;
      }

      #endregion

      #region AMQP Body access APIs

      public Stream Body
      {
         get => GetBodyStream(new OutputStreamOptions());
         set => throw new ClientUnsupportedOperationException("Cannot set an OutputStream body on a StreamSenderMessage");
      }

      public IAdvancedMessage<Stream> ForEachBodySection(Action<ISection> consumer)
      {
         return this;
      }

      public IAdvancedMessage<Stream> AddBodySection(ISection section)
      {
         throw new NotImplementedException();
      }

      public IAdvancedMessage<Stream> ClearBodySections()
      {
         return this;
      }

      public IEnumerable<ISection> GetBodySections()
      {
         return new ISection[0];
      }

      public IAdvancedMessage<Stream> SetBodySections(IEnumerable<ISection> sections)
      {
         if (sections == null)
         {
            throw new ArgumentNullException("Cannot set body sections with a null enumeration");
         }

         foreach (ISection section in sections)
         {
            AddBodySection(section);
         }

         return this;
      }

      public Stream GetBodyStream(OutputStreamOptions options)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region Private stream sender message stream state management

      private enum StreamState
      {
         PREAMBLE,
         BODY_WRITABLE,
         BODY_WRITTING,
         COMPLETE,
         ABORTED
      }

      private void CheckStreamState(StreamState state, string errorMessage)
      {
         if (currentState != state)
         {
            throw new ClientIllegalStateException(errorMessage);
         }
      }

      private Header LazyCreateHeader()
      {
         CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Header after body writing has started.");
         return header ?? (header = new Header());
      }

      private Properties LazyCreateProperties()
      {
         CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Properties after body writing has started.");
         return properties ?? (properties = new Properties());
      }

      private ApplicationProperties LazyCreateApplicationProperties()
      {
         CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Application Properties after body writing has started.");
         return applicationProperties ?? (applicationProperties = new ApplicationProperties());
      }

      private MessageAnnotations LazyCreateMessageAnnotations()
      {
         CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Annotations after body writing has started.");
         return annotations ?? (annotations = new MessageAnnotations());
      }

      private Footer LazyCreateFooter()
      {
         if (currentState >= StreamState.COMPLETE)
         {
            throw new ClientIllegalStateException(
                "Cannot write to Message Footer after message has been marked completed or aborted.");
         }

         return footer ?? (footer = new Footer());
      }

      private void AppendDataToBuffer(IProtonBuffer incoming)
      {
         throw new NotImplementedException();
      }

      private void DoFlush()
      {
         if (buffer != null && buffer.IsReadable)
         {
            try
            {
               sender.DoStreamMessage(this, streamMessagePacket);
            }
            finally
            {
               buffer = null;
            }
         }
      }

      private ClientStreamSenderMessage Write(ISection section)
      {
         if (Aborted)
         {
            throw new ClientIllegalStateException("Cannot write a Section to an already aborted send context");
         }

         if (Completed)
         {
            throw new ClientIllegalStateException("Cannot write a Section to an already completed send context");
         }

         AppendDataToBuffer(ClientMessageSupport.EncodeSection(section, ProtonByteBufferAllocator.Instance.Allocate()));

         return this;
      }

      #endregion
   }
}