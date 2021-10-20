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
using System.Collections;
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Impl
{
   public static class ClientMessageSupport
   {
      private static readonly IEncoder DEFAULT_ENCODER = CodecFactory.DefaultEncoder;
      private static readonly IDecoder DEFAULT_DECODER = CodecFactory.DefaultDecoder;

      /// <summary>
      /// Converts an unknown Message instance into a client Message instance
      /// either by cast or by construction of a new instance with a copy of the
      /// values carried in the given message.
      /// </summary>
      /// <typeparam name="T">The type of body the message carries</typeparam>
      /// <param name="message">The message to convert</param>
      /// <returns>A converted client message instance from the source message.</returns>
      public static IAdvancedMessage<T> ConvertMessage<T>(IMessage<T> message)
      {
         if (message is IAdvancedMessage<T>)
         {
            return (IAdvancedMessage<T>)message;
         }
         else
         {
            try
            {
               return message.ToAdvancedMessage();
            }
            catch (NotImplementedException)
            {
               return ConvertFromOutsideMessage(message);
            }
         }
      }

      /// <summary>
      /// Simple encode of section instance into a given buffer using the default encoder
      /// </summary>
      /// <param name="section">The section to encode</param>
      /// <param name="buffer">The buffer to encode into and return</param>
      /// <returns>The provided buffer with the encoded bytes added.</returns>
      public static IProtonBuffer EncodeSection(ISection section, IProtonBuffer buffer)
      {
         DEFAULT_ENCODER.WriteObject(buffer, DEFAULT_ENCODER.NewEncoderState(), section);
         return buffer;
      }

      /// <summary>
      /// Given a value of some type attempt to convert to the most appropriate AMQP
      /// body section type and return that to the caller.
      /// </summary>
      /// <typeparam name="T"></typeparam>
      /// <param name="body"></param>
      /// <returns></returns>
      public static ISection CreateSectionFromValue<T>(T body)
      {
         if (body == null)
         {
            return null;
         }
         else if (body is byte[])
         {
            return new Data(body as byte[]);
         }
         else if (body is IList)
         {
            return new AmqpSequence(body as IList);
         }
         else
         {
            return new AmqpValue(body);
         }
      }

      #region Message Encoding support API

      public static IProtonBuffer EncodeMessage<T>(IAdvancedMessage<T> message, IDictionary<string, object> deliveryAnnotations)
      {
         return EncodeMessage(DEFAULT_ENCODER, DEFAULT_ENCODER.NewEncoderState(), ProtonByteBufferAllocator.Instance, message, deliveryAnnotations);
      }

      public static IProtonBuffer EncodeMessage<T>(IEncoder encoder, IProtonBufferAllocator allocator, IAdvancedMessage<T> message, IDictionary<string, object> deliveryAnnotations)
      {
         return EncodeMessage(encoder, encoder.NewEncoderState(), ProtonByteBufferAllocator.Instance, message, deliveryAnnotations);
      }

      public static IProtonBuffer EncodeMessage<T>(IEncoder encoder, IEncoderState encoderState, IProtonBufferAllocator allocator, IAdvancedMessage<T> message, IDictionary<string, object> deliveryAnnotations)
      {
         IProtonBuffer buffer = allocator.Allocate();

         Header header = message.Header;
         MessageAnnotations messageAnnotations = message.Annotations;
         Properties properties = message.Properties;
         ApplicationProperties applicationProperties = message.ApplicationProperties;
         Footer footer = message.Footer;

         if (header != null)
         {
            encoder.WriteObject(buffer, encoderState, header);
         }
         if (deliveryAnnotations != null)
         {
            encoder.WriteObject(buffer, encoderState, new DeliveryAnnotations(ClientConversionSupport.ToSymbolKeyedMap(deliveryAnnotations)));
         }
         if (messageAnnotations != null)
         {
            encoder.WriteObject(buffer, encoderState, messageAnnotations);
         }
         if (properties != null)
         {
            encoder.WriteObject(buffer, encoderState, properties);
         }
         if (applicationProperties != null)
         {
            encoder.WriteObject(buffer, encoderState, applicationProperties);
         }

         message.ForEachBodySection(section => encoder.WriteObject(buffer, encoderState, section));

         if (footer != null)
         {
            encoder.WriteObject(buffer, encoderState, footer);
         }

         return buffer;
      }

      #endregion

      #region Message Decoding support API

      public static IMessage<object> DecodeMessage(IProtonBuffer buffer, Action<DeliveryAnnotations> daConsumer)
      {
         return DecodeMessage(DEFAULT_DECODER, DEFAULT_DECODER.NewDecoderState(), buffer, daConsumer);
      }

      public static IMessage<object> DecodeMessage(IDecoder decoder, IProtonBuffer buffer, Action<DeliveryAnnotations> daConsumer)
      {
         return DecodeMessage(decoder, decoder.NewDecoderState(), buffer, daConsumer);
      }

      public static IMessage<object> DecodeMessage(IDecoder decoder, IDecoderState decoderState,
                                                   IProtonBuffer buffer, Action<DeliveryAnnotations> daConsumer)
      {

         ClientMessage<object> message = new ClientMessage<object>();

         ISection section = null;

         while (buffer.IsReadable)
         {
            try
            {
               section = (ISection)decoder.ReadObject(buffer, decoderState);
            }
            catch (Exception e)
            {
               throw ClientExceptionSupport.CreateNonFatalOrPassthrough(e);
            }

            switch (section.Type)
            {
               case SectionType.Header:
                  message.Header = (Header)section;
                  break;
               case SectionType.DeliveryAnnotations:
                  if (daConsumer != null)
                  {
                     daConsumer.Invoke((DeliveryAnnotations)section);
                  }
                  break;
               case SectionType.MessageAnnotations:
                  message.Annotations = (MessageAnnotations)section;
                  break;
               case SectionType.Properties:
                  message.Properties = (Properties)section;
                  break;
               case SectionType.ApplicationProperties:
                  message.ApplicationProperties = (ApplicationProperties)section;
                  break;
               case SectionType.Data:
               case SectionType.AmqpSequence:
               case SectionType.AmqpValue:
                  message.AddBodySection(section);
                  break;
               case SectionType.Footer:
                  message.Footer = (Footer)section;
                  break;
               default:
                  throw new ClientException("Unknown Message Section forced decode abort.");
            }
         }

         return message;
      }

      #endregion

      #region Private supporting conversion methods

      private static ClientMessage<T> ConvertFromOutsideMessage<T>(IMessage<T> source)
      {
         Header header = new Header();
         header.Durable = source.Durable;
         header.Priority = source.Priority;
         header.TimeToLive = source.TimeToLive;
         header.FirstAcquirer = source.FirstAcquirer;
         header.DeliveryCount = source.DeliveryCount;

         byte[] userId = source.UserId;

         Properties properties = new Properties();
         properties.MessageId = source.MessageId;
         properties.UserId = userId != null ? ProtonByteBufferAllocator.Instance.Wrap(userId) : null;
         properties.To = source.To;
         properties.Subject = source.Subject;
         properties.ReplyTo = source.ReplyTo;
         properties.CorrelationId = source.CorrelationId;
         properties.ContentType = source.ContentType;
         properties.ContentEncoding = source.ContentEncoding;
         properties.AbsoluteExpiryTime = source.AbsoluteExpiryTime;
         properties.CreationTime = source.CreationTime;
         properties.GroupId = source.GroupId;
         properties.GroupSequence = source.GroupSequence;
         properties.ReplyToGroupId = source.ReplyToGroupId;

         MessageAnnotations messageAnnotations;
         if (source.HasAnnotations)
         {
            messageAnnotations = new MessageAnnotations(new Dictionary<Symbol, object>());

            source.ForEachAnnotation((key, value) =>
            {
               messageAnnotations.Value.Add(Symbol.Lookup(key), value);
            });
         }
         else
         {
            messageAnnotations = null;
         }

         ApplicationProperties applicationProperties;
         if (source.HasProperties)
         {
            applicationProperties = new ApplicationProperties(new Dictionary<string, object>());

            source.ForEachProperty((key, value) =>
            {
               applicationProperties.Value.Add(key, value);
            });
         }
         else
         {
            applicationProperties = null;
         }

         Footer footer;
         if (source.HasFooters)
         {
            footer = new Footer(new Dictionary<Symbol, object>());

            source.ForEachFooter((key, value) =>
            {
               footer.Value.Add(Symbol.Lookup(key), value);
            });
         }
         else
         {
            footer = null;
         }

         ClientMessage<T> message = new ClientMessage<T>(CreateSectionFromValue(source.Body));

         message.Header = header;
         message.Properties = properties;
         message.Annotations = messageAnnotations;
         message.ApplicationProperties = applicationProperties;
         message.Footer = footer;

         return message;
      }

      #endregion
   }
}