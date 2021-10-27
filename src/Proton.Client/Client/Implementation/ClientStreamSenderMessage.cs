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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public class ClientStreamSenderMessage : IStreamSenderMessage
   {
      private readonly ClientStreamSender sender;
      private readonly DeliveryAnnotations deliveryAnnotations;
      private readonly uint writeBufferSize;
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

      public Header Header { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Properties Properties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public MessageAnnotations Annotations { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public ApplicationProperties ApplicationProperties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Footer Footer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public bool Durable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public byte Priority { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public uint TimeToLive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public bool FirstAcquirer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public uint DeliveryCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public object MessageId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public byte[] UserId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public string To { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public string Subject { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public string ReplyTo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public object CorrelationId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public string ContentEncoding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public ulong AbsoluteExpiryTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public ulong CreationTime { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public string GroupId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public uint GroupSequence { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public string ReplyToGroupId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public bool HasAnnotations => throw new NotImplementedException();

      public bool HasProperties => throw new NotImplementedException();

      public Stream Body { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public bool HasFooters => throw new NotImplementedException();

      public IStreamSenderMessage Abort()
      {
         throw new NotImplementedException();
      }

      public IAdvancedMessage<Stream> AddBodySection(ISection section)
      {
         throw new NotImplementedException();
      }

      public IAdvancedMessage<Stream> ClearBodySections()
      {
         throw new NotImplementedException();
      }

      public IStreamSenderMessage Complete()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Encode(IDictionary<string, object> deliveryAnnotations)
      {
         throw new NotImplementedException();
      }

      public IMessage<Stream> ForEachAnnotation(Action<string, object> consumer)
      {
         throw new NotImplementedException();
      }

      public IAdvancedMessage<Stream> ForEachBodySection(Action<ISection> consumer)
      {
         throw new NotImplementedException();
      }

      public IMessage<Stream> ForEachFooter(Action<string, object> consumer)
      {
         throw new NotImplementedException();
      }

      public IMessage<Stream> ForEachProperty(Action<string, object> consumer)
      {
         throw new NotImplementedException();
      }

      public object GetAnnotation(string key)
      {
         throw new NotImplementedException();
      }

      public IEnumerable<ISection> GetBodySections()
      {
         throw new NotImplementedException();
      }

      public Stream GetBodyStream(OutputStreamOptions options)
      {
         throw new NotImplementedException();
      }

      public object GetFooter(string key)
      {
         throw new NotImplementedException();
      }

      public object GetProperty(string key)
      {
         throw new NotImplementedException();
      }

      public bool HasAnnotation(string key)
      {
         throw new NotImplementedException();
      }

      public bool HasFooter(string key)
      {
         throw new NotImplementedException();
      }

      public bool HasProperty(string key)
      {
         throw new NotImplementedException();
      }

      public Stream RawOutputStream()
      {
         throw new NotImplementedException();
      }

      public object RemoveAnnotation(string key)
      {
         throw new NotImplementedException();
      }

      public object RemoveFooter(string key)
      {
         throw new NotImplementedException();
      }

      public object RemoveProperty(string key)
      {
         throw new NotImplementedException();
      }

      public IMessage<Stream> SetAnnotation(string key, object value)
      {
         throw new NotImplementedException();
      }

      public IAdvancedMessage<Stream> SetBodySections(IEnumerable<ISection> section)
      {
         throw new NotImplementedException();
      }

      public IMessage<Stream> SetFooter(string key, object value)
      {
         throw new NotImplementedException();
      }

      public IMessage<Stream> SetProperty(string key, object value)
      {
         throw new NotImplementedException();
      }

      #region Private stream sender message stream state management

      private enum StreamState
      {
         PREAMBLE,
         BODY_WRITABLE,
         BODY_WRITTING,
         COMPLETE,
         ABORTED
      }

      #endregion
   }
}