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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP Message instance used by senders and receivers to provide a high
   /// level abstraction around an AMQP message.
   /// </summary>
   /// <typeparam name="T">The type that comprises the message body</typename>
   public interface IMessage<T>
   {
      #region Static Message Factory Methods

      /// <summary>
      /// Create and return an IMessage that will carry no body section unless one
      /// is assigned by the caller.
      /// </summary>
      /// <typeparam name="E">The type that the message body will be</typeparam>
      /// <returns>a new message instance with an empty body.</returns>
      static IMessage<E> Create<E>()
      {
         return null;  // TODO
      }

      /// <summary>
      /// Create and return an IMessage that will carry the body section provided
      /// </summary>
      /// <typeparam name="E">The type that the message body will be</typeparam>
      /// <returns>a new message instance with the provided body.</returns>
      static IMessage<E> Create<E>(E value)
      {
         return null;  // TODO
      }

      /// <summary>
      /// Create and return an IMessage that will carry the body section provided
      /// as an AMQP Data section that carries the provided bytes.
      /// </summary>
      /// <param name="value">The byte array to wrap in the AMQP message body</param>
      /// <returns>a new message instance with the provided body.</returns>
      static IMessage<byte[]> Create(byte[] value)
      {
         return null;  // TODO
      }

      /// <summary>
      /// Create and return an IMessage that will carry the body section provided
      /// as an AMQP Sequence section that carries the provided list entries.
      /// </summary>
      /// <param name="value">The list to wrap in the AMQP message body</param>
      /// <typeparam name="E">The type that the message list body will be</typeparam>
      /// <returns>a new message instance with the provided body.</returns>
      static IMessage<IList<E>> Create<E>(IList<E> value)
      {
         return null;  // TODO
      }

      /// <summary>
      /// Create and return an IMessage that will carry the body section provided
      /// as an AMQP Value section that carries the provided map entries.
      /// </summary>
      /// <param name="value">The map to wrap in the AMQP message body</param>
      /// <typeparam name="K">The type that the message dictionary keys body will be</typeparam>
      /// <typeparam name="V">The type that the message dictionary values body will be</typeparam>
      /// <returns>a new message instance with the provided body.</returns>
      static IMessage<IDictionary<K, V>> Create<K, V>(IDictionary<K, V> value)
      {
         return null;  // TODO
      }

      #endregion

      #region Conversion helper to Advanced Message handling

      /// <summary>
      /// Safely converts this message to an advacned message instance which allows lower level
      /// access to AMQP message constructs.
      ///
      /// The default implementation first checks if the current instance is already of the correct
      /// type before performing a brute force conversion of the current message to the client's
      /// own internal IAdvancedMessage implementation.  Users should override this method if the
      /// internal conversion implementation is insufficient to obtain the proper message structure
      /// to encode a meaningful 'on the wire' encoding of their custom implementation.
      /// </summary>
      /// <returns>An advanced message view of the original message</returns>
      /// <exception cref="ClientException">If an error occurs during the conversion</exception>
      IAdvancedMessage<T> ToAdvancedMessage()
      {
         if (this is IAdvancedMessage<T>)
         {
            return (IAdvancedMessage<T>) this;
         }
         else
         {
            return null; // TODO ClientMessageSupport.convertMessage(this);
         }
      }

      #endregion

      #region AMQP Header access

      /// <summary>
      /// For a message being sent this gets and sets the durability flag on the
      /// message.  For a received message this gets or overwrites the durability
      /// flag set by the original sender (unless already locally updated).
      /// </summary>
      bool Durable { get; set; }

      /// <summary>
      /// For a message being sent this gets and sets the message priority on the
      /// message.  For a received message this gets or overwrites the priority
      /// value set by the original sender (unless already locally updated).
      /// </summary>
      byte Priority { get; set; }

      /// <summary>
      /// For a message being sent this gets and sets the message time to live on
      /// the message.  For a received message this gets or overwrites the time to
      /// live value set by the original sender (unless already locally updated).
      ///
      /// The time to live duration in milliseconds for which the message is to be
      /// considered "live". If this is set then a message expiration time will be
      /// computed based on the time of arrival at an intermediary. Messages that live
      /// longer than their expiration time will be discarded (or dead lettered). When
      /// a message is transmitted by an intermediary that was received with a time to
      /// live, the transmitted message's header SHOULD contain a time to live that is
      /// computed as the difference between the current time and the formerly computed
      /// message expiration time, i.e., the reduced time to live, so that messages
      /// will eventually die if they end up in a delivery loop.
      /// </summary>
      uint TimeToLive { get; set; }

      /// <summary>
      /// For a message being sent this gets and sets the first acquirer flag on the
      /// message.  For a received message this gets or overwrites the first acquirer
      /// flag set by the original sender (unless already locally updated).
      ///
      /// If this value is true, then this message has not been acquired by any other link.
      /// If this value is false, then this message MAY have previously been acquired by
      /// another link or links.
      /// </summary>
      bool FirstAcquirer { get; set; }

      /// <summary>
      /// For a message being sent this gets and sets the message delivery count on
      /// the message.  For a received message this gets or overwrites the delivery
      /// count set by the original sender (unless already locally updated).
      /// </summary>
      uint DeliveryCount { get; set; }

      #endregion

      #region AMQP Properties Access

      #endregion

      #region AMQP Message Annotations Access

      #endregion

      #region AMQP Application Properties Access

      #endregion

      #region AMQP Message body access

      #endregion

      #region AMQP Footer access

      #endregion

   }
}