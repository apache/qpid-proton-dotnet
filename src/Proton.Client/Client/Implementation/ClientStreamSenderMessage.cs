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
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientStreamSenderMessage : IStreamSenderMessage
   {
      private static readonly int DATA_SECTION_HEADER_ENCODING_SIZE = 8;

      // Standard encoding data for a Data Section (Requires four byte size written before writing the actual data)
      private static readonly byte[] DATA_SECTION_PREAMBLE = { (byte)EncodingCodes.DescribedTypeIndicator,
                                                               (byte)EncodingCodes.SmallULong,
                                                               (byte)Data.DescriptorCode,
                                                               (byte)EncodingCodes.VBin32 };

      private readonly ClientStreamSender sender;
      private readonly DeliveryAnnotations deliveryAnnotations;
      private readonly uint writeBufferSize;
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

      public IStreamTracker Tracker => Completed ? tracker : null;

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
         if (Completed)
         {
            throw new ClientIllegalStateException("Cannot create an Stream from a completed send context");
         }

         if (Aborted)
         {
            throw new ClientIllegalStateException("Cannot create an Stream from a aborted send context");
         }

         if (currentState == StreamState.BODY_WRITTING)
         {
            throw new ClientIllegalStateException("Cannot add more body sections while an Stream is active");
         }

         TransitionToWritableState();

         return new SendContextRawBytesOutputStream(
            this, ProtonByteBufferAllocator.Instance.Allocate(writeBufferSize, writeBufferSize));
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
         set => throw new ClientUnsupportedOperationException("Cannot set an Stream body on a StreamSenderMessage");
      }

      public IAdvancedMessage<Stream> ForEachBodySection(Action<ISection> consumer)
      {
         return this;
      }

      public IAdvancedMessage<Stream> AddBodySection(ISection section)
      {
         if (Completed)
         {
            throw new ClientIllegalStateException("Cannot add more body sections to a completed message");
         }

         if (Aborted)
         {
            throw new ClientIllegalStateException("Cannot add more body sections to an aborted message");
         }

         if (currentState == StreamState.BODY_WRITTING)
         {
            throw new ClientIllegalStateException("Cannot add more body sections while an OutputStream is active");
         }

         TransitionToWritableState();

         AppendDataToBuffer(ClientMessageSupport.EncodeSection(section, ProtonByteBufferAllocator.Instance.Allocate()).Split());

         return this;
      }

      public IAdvancedMessage<Stream> ClearBodySections()
      {
         return this;
      }

      public IEnumerable<ISection> GetBodySections()
      {
         return Array.Empty<ISection>(); // Non null empty result to indicate no sections
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
         if (Completed)
         {
            throw new ClientIllegalStateException("Cannot create an OutputStream from a completed send context");
         }

         if (Aborted)
         {
            throw new ClientIllegalStateException("Cannot create an OutputStream from a aborted send context");
         }

         if (currentState == StreamState.BODY_WRITTING)
         {
            throw new ClientIllegalStateException("Cannot add more body sections while an OutputStream is active");
         }

         TransitionToWritableState();

         IProtonBuffer streamBuffer = ProtonByteBufferAllocator.Instance.Allocate(writeBufferSize, writeBufferSize);

         if (options.BodyLength > 0)
         {
            return new SingularDataSectionOutputStream(this, options, streamBuffer);
         }
         else
         {
            return new MultipleDataSectionsOutputStream(this, options, streamBuffer);
         }
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
         return header ??= new Header();
      }

      private Properties LazyCreateProperties()
      {
         CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Properties after body writing has started.");
         return properties ??= new Properties();
      }

      private ApplicationProperties LazyCreateApplicationProperties()
      {
         CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Application Properties after body writing has started.");
         return applicationProperties ??= new ApplicationProperties(new Dictionary<string, object>());
      }

      private MessageAnnotations LazyCreateMessageAnnotations()
      {
         CheckStreamState(StreamState.PREAMBLE, "Cannot write to Message Annotations after body writing has started.");
         return annotations ??= new MessageAnnotations(new Dictionary<Symbol, object>());
      }

      private Footer LazyCreateFooter()
      {
         if (currentState >= StreamState.COMPLETE)
         {
            throw new ClientIllegalStateException(
                "Cannot write to Message Footer after message has been marked completed or aborted.");
         }

         return footer ??= new Footer(new Dictionary<Symbol, object>());
      }

      private void AppendDataToBuffer(IProtonBuffer incoming)
      {
         if (buffer == null)
         {
            buffer = incoming;
         }
         else
         {
            // When appending buffers we should ensure that the buffer instance is only
            // readable to ensure that the composite will accept a chain of buffers which
            // must not have any unwritten gaps.
            if (buffer is ProtonCompositeBuffer composite)
            {
               composite.Append(incoming);
            }
            else
            {
               buffer = IProtonCompositeBuffer.Compose(buffer, incoming);
            }
         }

         // Were aren't currently attempting to optimize each outbound chunk of the streaming
         // send, if the block accumulated is larger than the write buffer we don't try and
         // split it but instead let the frame writer just write multiple frames.  This can
         // result in a trailing single tiny frame but for now this case isn't being optimized

         if (buffer.ReadableBytes >= writeBufferSize)
         {
            try
            {
               sender.DoStreamMessage(this, buffer, messageFormat);
            }
            finally
            {
               buffer = null;
            }
         }
      }

      private void DoFlush()
      {
         if (buffer != null && buffer.IsReadable)
         {
            try
            {
               sender.DoStreamMessage(this, buffer, messageFormat);
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

         AppendDataToBuffer(ClientMessageSupport.EncodeSection(section, ProtonByteBufferAllocator.Instance.Allocate()).Split());

         return this;
      }

      private void TransitionToWritableState()
      {
         if (currentState == StreamState.PREAMBLE)
         {

            if (header != null)
            {
               AppendDataToBuffer(
                  ClientMessageSupport.EncodeSection(header, ProtonByteBufferAllocator.Instance.Allocate()).Split());
            }
            if (deliveryAnnotations != null)
            {
               AppendDataToBuffer(
                  ClientMessageSupport.EncodeSection(deliveryAnnotations, ProtonByteBufferAllocator.Instance.Allocate()).Split());
            }
            if (annotations != null)
            {
               AppendDataToBuffer(
                  ClientMessageSupport.EncodeSection(annotations, ProtonByteBufferAllocator.Instance.Allocate()).Split());
            }
            if (properties != null)
            {
               AppendDataToBuffer(
                  ClientMessageSupport.EncodeSection(properties, ProtonByteBufferAllocator.Instance.Allocate()).Split());
            }
            if (applicationProperties != null)
            {
               AppendDataToBuffer(
                  ClientMessageSupport.EncodeSection(applicationProperties, ProtonByteBufferAllocator.Instance.Allocate()).Split());
            }

            currentState = StreamState.BODY_WRITABLE;
         }
      }

      #endregion

      #region Stream implementations used for writing body sections

      private abstract class StreamMessageOutputStream : Stream
      {
         protected readonly AtomicBoolean closed = new AtomicBoolean();
         protected readonly OutputStreamOptions options;
         protected IProtonBuffer streamBuffer;
         protected readonly ClientStreamSenderMessage message;

         protected int bytesWritten;

         public StreamMessageOutputStream(ClientStreamSenderMessage message, OutputStreamOptions options, IProtonBuffer buffer)
         {
            this.options = options;
            this.streamBuffer = buffer;
            this.message = message;

            // Stream takes control of state until closed.
            this.message.currentState = StreamState.BODY_WRITTING;
         }

         #region Stream API implementation

         // The output of the data section on a per frame basis prevents
         // the stream from being seekable and of course cannot be read from.
         // Length is never really know because it could be that we've written
         // one data section and moved onto another, position cannot be changed
         // because we are writing in batches and cannot go back to a batch that
         // was written and forgotten.

         public sealed override bool CanTimeout => false;

         public sealed override bool CanRead => false;

         public sealed override bool CanWrite => true;

         public sealed override bool CanSeek => false;

         public sealed override long Length => throw new NotImplementedException("No length value available");

         public sealed override long Position
         {
            get => throw new NotImplementedException("Cannot read a position from a streamed message stream");
            set => throw new NotImplementedException("Cannot assign a position to a streamed message stream");
         }

         #endregion

         public override void WriteByte(byte value)
         {
            CheckClosed();
            CheckOutputLimitReached(1);
            streamBuffer.WriteUnsignedByte(value);
            if (!streamBuffer.IsWritable)
            {
               Flush();
            }
            bytesWritten++;
         }

         public override void Write(ReadOnlySpan<byte> bytes)
         {
            Write(bytes.ToArray(), 0, bytes.Length);
         }

         public override void Write(byte[] bytes, int offset, int length)
         {
            CheckClosed();
            CheckOutputLimitReached(length);
            if (streamBuffer.WritableBytes >= length)
            {
               streamBuffer.WriteBytes(bytes, offset, length);
               bytesWritten += length;
               if (!streamBuffer.IsWritable)
               {
                  Flush();
               }
            }
            else
            {
               int remaining = length;

               while (remaining > 0)
               {
                  int toWrite = (int)Math.Min(remaining, streamBuffer.WritableBytes);
                  bytesWritten += toWrite;
                  streamBuffer.WriteBytes(bytes, offset + (length - remaining), toWrite);
                  if (!streamBuffer.IsWritable)
                  {
                     Flush();
                  }
                  remaining -= toWrite;
               }
            }
         }

         public override void Flush()
         {
            CheckClosed();

            if (options.BodyLength <= 0)
            {
               DoFlushPending(false);
            }
            else
            {
               DoFlushPending(bytesWritten == options.BodyLength && options.CompleteSendOnClose);
            }
         }

         public override void Close()
         {
            if (closed.CompareAndSet(false, true) && !message.Completed)
            {
               message.currentState = StreamState.BODY_WRITABLE;

               if (options.BodyLength > 0 && options.BodyLength != bytesWritten)
               {
                  // Limit was set but user did not write all of it so we must abort.
                  try
                  {
                     message.Abort();
                  }
                  catch (ClientException e)
                  {
                     throw new IOException(e.Message, e);
                  }
               }
               else
               {
                  // Limit not set or was set and user wrote that many bytes so we can complete.
                  DoFlushPending(options.CompleteSendOnClose);
               }
            }
         }

         private void CheckOutputLimitReached(int writeSize)
         {
            int outputLimit = options.BodyLength;

            if (message.Completed)
            {
               throw new IOException("Cannot write to an already completed message output stream");
            }

            if (outputLimit > 0 && (bytesWritten + writeSize) > outputLimit)
            {
               throw new IOException("Cannot write beyond configured stream output limit");
            }
         }

         private void CheckClosed()
         {
            if (closed.Get())
            {
               throw new IOException("The OutputStream has already been closed.");
            }

            if (message.sender.IsClosed)
            {
               throw new IOException("The parent Sender instance has already been closed.");
            }
         }

         protected virtual void DoFlushPending(bool complete)
         {
            try
            {
               if (streamBuffer.IsReadable)
               {
                  // Copy the buffer as it will be reset and reused and we cannot
                  // assume that the buffer will be fully written by the IO layer
                  // before the next write operation is allowed to proceed.
                  message.AppendDataToBuffer(streamBuffer.Copy());
               }

               if (complete)
               {
                  message.Complete();
               }
               else
               {
                  message.DoFlush();
               }

               if (!complete)
               {
                  streamBuffer.Reset();
               }
            }
            catch (ClientException e)
            {
               throw new IOException(e.Message, e);
            }
         }

         public override int Read(byte[] buffer, int offset, int count)
         {
            throw new NotImplementedException("Cannot read from a streamed message output stream");
         }

         public override long Seek(long offset, SeekOrigin origin)
         {
            throw new NotImplementedException("Cannot seek within a streamed message output stream");
         }

         public override void SetLength(long value)
         {
            throw new NotImplementedException("Cannot change length of a streamed message output stream");
         }
      }

      private sealed class SendContextRawBytesOutputStream : StreamMessageOutputStream
      {
         public SendContextRawBytesOutputStream(ClientStreamSenderMessage message, IProtonBuffer buffer)
             : base(message, new OutputStreamOptions(), buffer)
         {
         }
      }

      private sealed class SingularDataSectionOutputStream : StreamMessageOutputStream
      {
         public SingularDataSectionOutputStream(ClientStreamSenderMessage message, OutputStreamOptions options, IProtonBuffer buffer)
             : base(message, options, buffer)
         {
            IProtonBuffer preamble = ProtonByteBufferAllocator.Instance.Allocate(
               DATA_SECTION_HEADER_ENCODING_SIZE, DATA_SECTION_HEADER_ENCODING_SIZE);

            preamble.WriteBytes(DATA_SECTION_PREAMBLE);
            preamble.WriteInt(options.BodyLength);

            message.AppendDataToBuffer(preamble);
         }
      }

      private sealed class MultipleDataSectionsOutputStream : StreamMessageOutputStream
      {

         public MultipleDataSectionsOutputStream(ClientStreamSenderMessage message, OutputStreamOptions options, IProtonBuffer buffer)
              : base(message, options, buffer)
         {
         }

         protected override void DoFlushPending(bool complete)
         {
            if (streamBuffer.IsReadable)
            {
               IProtonBuffer preamble = ProtonByteBufferAllocator.Instance.Allocate(
                  DATA_SECTION_HEADER_ENCODING_SIZE, DATA_SECTION_HEADER_ENCODING_SIZE);

               preamble.WriteBytes(DATA_SECTION_PREAMBLE);
               preamble.WriteInt((int)streamBuffer.ReadableBytes);

               try
               {
                  message.AppendDataToBuffer(preamble);
               }
               catch (ClientException e)
               {
                  throw new IOException(e.Message, e);
               }
            }

            base.DoFlushPending(complete);
         }
      }

      #endregion
   }
}