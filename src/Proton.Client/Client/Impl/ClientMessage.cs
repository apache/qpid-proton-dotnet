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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Impl
{
   public class ClientMessage<T> : IAdvancedMessage<T>
   {
      private Header header;
      private MessageAnnotations messageAnnotations;
      private Properties properties;
      private ApplicationProperties applicationProperties;
      private ISection body;
      private List<ISection> bodySections;
      private Footer footer;

      private uint messageFormat;

      /// <summary>
      /// Create a new empty client message instance
      /// </summary>
      internal ClientMessage()
      {
         this.body = null;
      }

      /// <summary>
      /// Create a new client message instance with the given body section.
      /// </summary>
      /// <param name="body"></param>
      internal ClientMessage(ISection body)
      {
         this.body = body;
      }

      #region Static ClientMessage factory methods

      public static ClientMessage<T> Create() {
         return new ClientMessage<T>();
      }

      public static ClientMessage<T> Create(ISection body) {
         return new ClientMessage<T>(body);
      }

      public static ClientMessage<T> CreateAdvancedMessage() {
         return new ClientMessage<T>();
      }

      #endregion

      /// <summary>
      /// Returns this message as an advanced message instance.
      /// </summary>
      /// <returns>This message instance as an advanced message interface</returns>
      public IAdvancedMessage<T> ToAdvancedMessage()
      {
         return this;
      }

      public uint MessageFormat
      {
         get => messageFormat;
         set => messageFormat = value;
      }

      public IProtonBuffer Encode(IDictionary<string, object> deliveryAnnotations)
      {
         return ClientMessageSupport.EncodeMessage(this, deliveryAnnotations);
      }

      #region Message API for the AMQP Header section of the message

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

      #region Message API for the AMQP Properties section of the message

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

            if (properties != null && properties.UserId != null)
            {
               result = new byte[properties.UserId.ReadableBytes];
               properties.UserId.CopyInto(properties.UserId.ReadOffset, result, 0, result.LongLength);
            }

            return result;
         }
         set
         {
            LazyCreateProperties().UserId =
               value == null ? null :
                  ProtonByteBufferAllocator.Instance.Allocate(value.Length, value.Length).WriteBytes(value);
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

      #region Message Annotations Access API

      public bool HasAnnotations => messageAnnotations?.Value?.Count > 0;

      public bool HasAnnotation(string key)
      {
         return messageAnnotations?.Value?.ContainsKey(Symbol.Lookup(key)) ?? false;
      }

      public object GetAnnotation(string key)
      {
         object annotation = null;
         messageAnnotations?.Value?.TryGetValue(Symbol.Lookup(key), out annotation);
         return annotation;
      }

      public IMessage<T> SetAnnotation(string key, object value)
      {
         LazyCreateMessageAnnotations().Value[Symbol.Lookup(key)] = value;
         return this;
      }

      public object RemoveAnnotation(string key)
      {
         object oldValue = null;

         if (HasAnnotations)
         {
            messageAnnotations.Value.TryGetValue(Symbol.Lookup(key), out oldValue);
            messageAnnotations.Value.Remove(Symbol.Lookup(key));
         }

         return oldValue;
      }

      public IMessage<T> ForEachAnnotation(Action<string, object> consumer)
      {
         if (HasAnnotations)
         {
            foreach (KeyValuePair<Symbol, object> item in messageAnnotations.Value)
            {
               consumer.Invoke(item.Key.ToString(), item.Value);
            }
         }

         return this;
      }

      #endregion

      #region Application Properties Access API

      public bool HasProperties => applicationProperties?.Value?.Count > 0;

      public bool HasProperty(string key)
      {
         return applicationProperties?.Value?.ContainsKey(key) ?? false;
      }

      public object GetProperty(string key)
      {
         object property = null;
         applicationProperties?.Value?.TryGetValue(key, out property);
         return property;
      }

      public IMessage<T> SetProperty(string key, object value)
      {
         LazyCreateApplicationProperties().Value[key] = value;
         return this;
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

      public IMessage<T> ForEachProperty(Action<string, object> consumer)
      {
         if (HasProperties)
         {
            foreach (KeyValuePair<string, object> item in applicationProperties.Value)
            {
               consumer.Invoke(item.Key, item.Value);
            }
         }

         return this;
      }

      #endregion

      #region Footer Access API

      public bool HasFooters => footer?.Value?.Count > 0;

      public bool HasFooter(string key)
      {
         return footer?.Value?.ContainsKey(Symbol.Lookup(key)) ?? false;
      }

      public object GetFooter(string key)
      {
         object result = null;
         footer?.Value?.TryGetValue(Symbol.Lookup(key), out result);
         return result;
      }

      public IMessage<T> SetFooter(string key, object value)
      {
         LazyCreateFooter().Value[Symbol.Lookup(key)] = value;
         return this;
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

      public IMessage<T> ForEachFooter(Action<string, object> consumer)
      {
         if (HasFooters)
         {
            foreach (KeyValuePair<Symbol, object> item in footer.Value)
            {
               consumer.Invoke(item.Key.ToString(), item.Value);
            }
         }

         return this;
      }

      #endregion

      #region Direct access API for message sections other than body

      public Header Header
      {
         get => header;
         set => header = value;
      }

      public Properties Properties
      {
         get => properties;
         set => properties = value;
      }

      public MessageAnnotations Annotations
      {
         get => messageAnnotations;
         set => messageAnnotations = value;
      }

      public ApplicationProperties ApplicationProperties
      {
         get => applicationProperties;
         set => applicationProperties = value;
      }

      public Footer Footer
      {
         get => footer;
         set => footer = value;
      }

      #endregion

      public T Body
      {
         get
         {
            if (body != null)
            {
               return (T)body.Value;
            }
            else if (bodySections != null)
            {
               return (T)bodySections[0].Value;
            }
            else
            {
               return default(T);
            }
         }
         set
         {
            ClearBodySections();
            body = ClientMessageSupport.CreateSectionFromValue(value);
         }
      }

      public IAdvancedMessage<T> AddBodySection(ISection section)
      {
         if (section == null)
         {
            throw new ArgumentNullException("Additional body section cannot be null.");
         }

         if (body == null && bodySections == null)
         {
            body = (ISection)section;
         }
         else
         {
            if (bodySections == null)
            {
               bodySections = new List<ISection>();

               // Preserve older section from original message creation.
               if (body != null)
               {
                  bodySections.Add(body);
                  body = null;
               }
            }

            bodySections.Add(ValidateBodySections(messageFormat, bodySections, section));
         }

         return this;
      }

      public IEnumerable<ISection> GetBodySections()
      {
         List<ISection> result = new List<ISection>();

         if (body != null)
         {
            result.Add(body);
         }
         else if (bodySections != null)
         {
            foreach (ISection section in bodySections)
            {
               result.Add(section);
            }
         }

         return result;
      }

      public IAdvancedMessage<T> SetBodySections(IEnumerable<ISection> sections)
      {
         bodySections = null;
         body = null;

         if (sections != null)
         {
            List<ISection> result = new List<ISection>();
            foreach (ISection section in sections)
            {
               result.Add(ValidateBodySections(messageFormat, result, section));
            }
            bodySections = result.Count > 0 ? result : null;
         }

         return this;
      }

      public IAdvancedMessage<T> ClearBodySections()
      {
         body = null;
         bodySections = null;

         return this;
      }

      public IAdvancedMessage<T> ForEachBodySection(Action<ISection> consumer)
      {
         if (bodySections != null)
         {
            foreach (ISection section in bodySections)
            {
               consumer.Invoke(section);
            }
         }
         else
         {
            if (body != null)
            {
               consumer.Invoke(body);
            }
         }

         return this;
      }

      #region Private message support methods

      private Header LazyCreateHeader()
      {
         return header ?? (header = new Header());
      }

      private Properties LazyCreateProperties()
      {
         return properties ?? (properties = new Properties());
      }

      private ApplicationProperties LazyCreateApplicationProperties()
      {
         return applicationProperties ?? (applicationProperties = new ApplicationProperties(new Dictionary<string, object>()));
      }

      private MessageAnnotations LazyCreateMessageAnnotations()
      {
         return messageAnnotations ?? (messageAnnotations = new MessageAnnotations(new Dictionary<Symbol, object>()));
      }

      private Footer LazyCreateFooter()
      {
         return footer ?? (footer = new Footer(new Dictionary<Symbol, object>()));
      }

      private static ISection ValidateBodySections(uint messageFormat, List<ISection> target, ISection section)
      {
         if (messageFormat == 0 && target != null && target.Count > 0)
         {
            switch (section.Type)
            {
               case SectionType.AmqpSequence:
                  if (target[0].Type != SectionType.AmqpSequence)
                  {
                     throw new ArgumentException(
                         "Message Format violation: AmqpSequence expected but got type: " + section.Type);
                  }
                  break;
               case SectionType.AmqpValue:
                  throw new ArgumentException(
                      "Message Format violation: Only one AmqpValue section allowed");
               case SectionType.Data:
                  if (target[0].Type != SectionType.Data)
                  {
                     throw new ArgumentException(
                         "Message Format violation: Data Section expected but got type: " + section.Type);
                  }
                  break;
               default:
                  break;
            }
         }

         return section;
      }

      #endregion
   }
}