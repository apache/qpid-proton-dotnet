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
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientStreamReceiverMessage : IStreamReceiverMessage
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamReceiverMessage>();

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
      private MessageBodyInputStream bodyStream;

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
         set => throw new ClientUnsupportedOperationException("Cannot write to a StreamReceiverMessage");
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

      internal DeliveryAnnotations DeliveryAnnotations =>
         EnsureStreamDecodedTo(StreamState.DELIVERY_ANNOTATIONS_READ).deliveryAnnotations;

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
            try
            {
               IStreamTypeDecoder decoder;
               try
               {
                  decoder = protonDecoder.ReadNextTypeDecoder(deliveryStream, decoderState);
               }
               catch (DecodeEOFException)
               {
                  currentState = StreamState.FOOTER_READ;
                  break;
               }

               Type typeClass = decoder.DecodesType;
               if (typeClass == typeof(Header))
               {
                  header = (Header)decoder.ReadValue(deliveryStream, decoderState);
                  currentState = StreamState.HEADER_READ;
               }
               else if (typeClass == typeof(DeliveryAnnotations))
               {
                  deliveryAnnotations = (DeliveryAnnotations)decoder.ReadValue(deliveryStream, decoderState);
                  currentState = StreamState.DELIVERY_ANNOTATIONS_READ;
               }
               else if (typeClass == typeof(MessageAnnotations))
               {
                  annotations = (MessageAnnotations)decoder.ReadValue(deliveryStream, decoderState);
                  currentState = StreamState.MESSAGE_ANNOTATIONS_READ;
               }
               else if (typeClass == typeof(Properties))
               {
                  properties = (Properties)decoder.ReadValue(deliveryStream, decoderState);
                  currentState = StreamState.PROPERTIES_READ;
               }
               else if (typeClass == typeof(ApplicationProperties))
               {
                  applicationProperties = (ApplicationProperties)decoder.ReadValue(deliveryStream, decoderState);
                  currentState = StreamState.APPLICATION_PROPERTIES_READ;
               }
               else if (typeClass == typeof(AmqpSequence))
               {
                  currentState = StreamState.BODY_READABLE;
                  if (bodyStream == null)
                  {
                     bodyStream = new AmqpSequenceInputStream(this);
                  }
               }
               else if (typeClass == typeof(AmqpValue))
               {
                  currentState = StreamState.BODY_READABLE;
                  if (bodyStream == null)
                  {
                     bodyStream = new AmqpValueInputStream(this);
                  }
               }
               else if (typeClass == typeof(Data))
               {
                  currentState = StreamState.BODY_READABLE;
                  if (bodyStream == null)
                  {
                     bodyStream = new DataSectionInputStream(this);
                  }
               }
               else if (typeClass == typeof(Footer))
               {
                  footer = (Footer)decoder.ReadValue(deliveryStream, decoderState);
                  currentState = StreamState.FOOTER_READ;
               }
               else
               {
                  throw new ClientMessageFormatViolationException("Incoming message carries unknown Section");
               }
            }
            catch (Exception ex) when (ex is ClientMessageFormatViolationException || ex is DecodeException)
            {
               currentState = StreamState.DECODE_ERROR;
               if (deliveryStream != null)
               {
                  try
                  {
                     deliveryStream.Close();
                  }
                  catch (IOException)
                  {
                  }
               }

               // TODO: At the moment there is no automatic rejection or release etc
               //       of the delivery.  The user is expected to apply a disposition in
               //       response to this error that initiates the desired outcome.  We
               //       could look to add auto settlement with a configured outcome in
               //       the future.

               throw ClientExceptionSupport.CreateNonFatalOrPassthrough(ex);
            }
         }

         return this;
      }

      #endregion

      #region Message Body Input Stream implementation

      internal abstract class MessageBodyInputStream : Stream
      {
         protected readonly Stream rawInputStream;
         protected readonly ClientStreamReceiverMessage message;

         protected bool closed;
         protected uint remainingSectionBytes = 0;

         public MessageBodyInputStream(ClientStreamReceiverMessage message)
         {
            this.message = message;
            this.rawInputStream = message.deliveryStream;

            ValidateAndScanNextSection();
         }

         public override bool CanRead => true;

         public override bool CanSeek => rawInputStream.CanSeek;

         public override bool CanWrite => false;

         public override long Length => rawInputStream.Length;

         public override long Position
         {
            get => rawInputStream.Position;
            set => rawInputStream.Position = value;
         }

         public abstract Type BodyTypeClass { get; }

         public override void Close()
         {
            try
            {
               // This will check is another body section is present or if there was a footer and if
               // a Footer is present it will be decoded and the message payload should be fully consumed
               // at that point.  Otherwise the underlying raw InputStream will handle the task of
               // discarding pending bytes for the message to ensure the receiver does not still on
               // waiting for session window to be opened.
               if (remainingSectionBytes == 0)
               {
                  message.EnsureStreamDecodedTo(StreamState.FOOTER_READ);
               }
            }
            catch (ClientException e)
            {
               throw new IOException("Caught error while attempting to advance past remaining message body", e);
            }
            finally
            {
               closed = true;
               rawInputStream.Close();
               base.Close();
            }
         }

         public override void Flush()
         {
         }

         public override int ReadByte()
         {
            CheckClosed();

            while (true)
            {
               if (remainingSectionBytes == 0 && !TryMoveToNextBodySection())
               {
                  return -1;  // Cannot read any further.
               }
               else
               {
                  remainingSectionBytes--;
                  return rawInputStream.ReadByte();
               }
            }
         }

         public override int Read(Span<byte> buffer)
         {
            CheckClosed();

            int bytesRead = 0;
            for (; bytesRead < buffer.Length; ++bytesRead)
            {
               if (remainingSectionBytes == 0 && !TryMoveToNextBodySection())
               {
                  break; // We are at the end of the body sections
               }

               int result = ReadByte();
               if (result >= 0)
               {
                  buffer[bytesRead] = (byte)result;
               }
               else
               {
                  break;
               }
            }

            return bytesRead;
         }

         public override int Read(byte[] buffer, int offset, int count)
         {
            CheckClosed();

            int bytesRead = 0;

            while (bytesRead != count)
            {
               if (remainingSectionBytes == 0 && !TryMoveToNextBodySection())
               {
                  bytesRead = bytesRead > 0 ? bytesRead : 0;
                  break; // We are at the end of the body sections
               }

               int readChunk = (int)Math.Min(remainingSectionBytes, count - bytesRead);
               int actualRead = rawInputStream.Read(buffer, offset + bytesRead, readChunk);

               if (actualRead > 0)
               {
                  bytesRead += actualRead;
                  remainingSectionBytes -= (uint)actualRead;
               }
            }

            return bytesRead;
         }

         public override long Seek(long offset, SeekOrigin origin)
         {
            return rawInputStream.Seek(offset, origin);
         }

         public override void SetLength(long value)
         {
            throw new NotSupportedException("Cannot set length on an incoming streamed message Stream");
         }

         public override void Write(byte[] buffer, int offset, int count)
         {
            throw new NotSupportedException("Cannot write to an incoming streamed message Stream");
         }

         protected void CheckClosed()
         {
            if (closed)
            {
               throw new IOException("Stream was closed previously");
            }
         }

         protected abstract void ValidateAndScanNextSection();

         protected bool TryMoveToNextBodySection()
         {
            try
            {
               if (message.currentState != StreamState.FOOTER_READ)
               {
                  message.currentState = StreamState.BODY_PENDING;
                  message.EnsureStreamDecodedTo(StreamState.BODY_READABLE);
                  if (message.currentState == StreamState.BODY_READABLE)
                  {
                     ValidateAndScanNextSection();
                     return true;
                  }
               }

               return false;
            }
            catch (ClientException e)
            {
               throw new IOException(e.Message, e);
            }
         }
      }

      internal class DataSectionInputStream : MessageBodyInputStream
      {
         public DataSectionInputStream(ClientStreamReceiverMessage message) : base(message)
         {
         }

         public override Type BodyTypeClass => typeof(Data);

         protected override void ValidateAndScanNextSection()
         {
            IStreamTypeDecoder typeDecoder =
               message.protonDecoder.ReadNextTypeDecoder(rawInputStream, message.decoderState);

            if (typeDecoder.DecodesType == typeof(IProtonBuffer))
            {
               LOG.Trace("Data Section of size {0} ready for read.", remainingSectionBytes);
               IBinaryTypeDecoder binaryDecoder = (IBinaryTypeDecoder)typeDecoder;
               remainingSectionBytes = (uint)binaryDecoder.ReadSize(rawInputStream, message.decoderState);
            }
            else if (typeDecoder.DecodesType == typeof(void))
            {
               // Null body in the Data section which can be skipped.
               LOG.Trace("Data Section with no Binary payload read and skipped.");
               remainingSectionBytes = 0;
            }
            else
            {
               throw new DecodeException("Unknown payload in body of Data Section encoding.");
            }
         }
      }

      internal class AmqpSequenceInputStream : MessageBodyInputStream
      {
         public AmqpSequenceInputStream(ClientStreamReceiverMessage message) : base(message)
         {
         }

         public override Type BodyTypeClass => typeof(System.Collections.IList);

         protected override void ValidateAndScanNextSection()
         {
            throw new DecodeException("Cannot read the payload of an AMQP Sequence payload.");
         }
      }

      internal class AmqpValueInputStream : MessageBodyInputStream
      {
         public AmqpValueInputStream(ClientStreamReceiverMessage message) : base(message)
         {
         }

         public override Type BodyTypeClass => typeof(void);

         protected override void ValidateAndScanNextSection()
         {
            throw new DecodeException("Cannot read the payload of an AMQP Value payload.");
         }
      }
   }

   #endregion
}