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

      /// <summary>
      /// The message Id, if set, uniquely identifies a message within the message system. The
      /// message producer is usually responsible for setting the message-id in such a way that
      /// it is assured to be globally unique. A remote peer MAY discard a message as a duplicate
      /// if the value of the message-id matches that of a previously received message sent to
      /// the same node.
      /// </summary>
      object MessageId { get; set; }

      /// <summary>
      /// The identity of the user responsible for producing the message. The client sets this
      /// value, and it MAY be authenticated by intermediaries.
      /// </summary>
      byte[] UserId { get; set; }

      /// <summary>
      /// The to field identifies the node that is the intended destination of the message. On
      /// any given transfer this might not be the node at the receiving end of the link.
      /// </summary>
      string To { get; set; }

      /// <summary>
      /// The Subject field is a common field for summary information about the message content
      /// and purpose.
      /// </summary>
      string Subject { get; set; }

      /// <summary>
      /// The reply to field identifies a node that is the intended destination for responses
      /// to this message.
      /// </summary>
      string ReplyTo { get; set; }

      /// <summary>
      /// This is a client-specific id that can be used to mark or identify messages between
      /// clients.
      /// </summary>
      object CorrelationId { get; set; }

      /// <summary>
      /// The RFC-2046 MIME type for the message's application-data section (body). As per
      /// RFC-2046 this can contain a charset parameter defining the character encoding used:
      /// e.g., 'text/plain; charset="utf-8"'
      ///
      /// When using an application-data section with a section code other than data,
      /// content-type SHOULD NOT be set.
      /// </summary>
      /// <remarks>
      /// For clarity, as per section 7.2.1 of RFC-2616, where the content type is unknown the
      /// content-type SHOULD NOT be set. This allows the recipient the opportunity to determine
      /// the actual type. Where the section is known to be truly opaque binary data, the
      /// content-type SHOULD be set to application/octet-stream.
      /// </remarks>
      string ContentType { get; set; }

      /// <summary>
      /// The content-encoding property is used as a modifier to the content-type. When present,
      /// its value indicates what additional content encodings have been applied to the
      /// application-data, and thus what decoding mechanisms need to be applied in order to
      /// obtain the media-type referenced by the content-type header field. Content-encoding is
      /// primarily used to allow a document to be compressed without losing the identity of its
      /// underlying content type.
      /// </summary>
      /// <remarks>
      /// <para>
      /// Content-encodings are to be interpreted as per section 3.5 of RFC 2616 [RFC2616]. Valid
      /// content-encodings are registered at IANA [IANAHTTPPARAMS].
      /// </para>
      /// <para>
      /// The content-encoding MUST NOT be set when the application-data section is other than data.
      /// The binary representation of all other application-data section types is defined completely
      /// in terms of the AMQP type system.
      /// </para>
      /// Implementations MUST NOT use the identity encoding. Instead, implementations SHOULD NOT set
      /// this property. Implementations SHOULD NOT use the compress encoding, except as to remain
      /// compatible with messages originally sent with other protocols, e.g. HTTP or SMTP.
      /// <para>
      /// Implementations SHOULD NOT specify multiple content-encoding values except as to be
      /// compatible with messages originally sent with other protocols, e.g. HTTP or SMTP.
      /// </para>
      /// </remarks>
      string ContentEncoding { get; set; }

      /// <summary>
      /// An absolute time when this message is considered to be expired.
      /// </summary>
      uint AbsoluteExpiryTime { get; set; }

      /// <summary>
      /// An absolute time when this message was created.
      /// </summary>
      uint CreationTime { get; set; }

      /// <summary>
      /// Identifies the group the message belongs to.
      /// </summary>
      string GroupId { get; set; }

      /// <summary>
      /// The relative position of this message within its group.
      /// </summary>
      uint GroupSequence { get; set; }

      /// <summary>
      /// This is a client-specific id that is used so that client can send replies to
      /// this message to a specific group.
      /// </summary>
      string ReplyToGroupId { get; set; }

      #endregion

      #region AMQP Message Annotations Access

      /// <summary>
      /// Checks if the message carries any annotations.
      /// </summary>
      /// <returns>true if the message instance carries any annotations</returns>
      bool HasAnnotations();

      /// <summary>
      /// Query the message to determine if the message carries the given annotation
      /// keyed value.
      /// </summary>
      /// <returns>true if the message instance carries the annotation</returns>
      bool HasAnnotation(string key);

      /// <summary>
      /// Returns the requested message annotation value from this message if it
      /// exists or returns null otherwise.
      /// </summary>
      /// <param name="key">The message annotation key</param>
      /// <returns>The value that is stored in the message annotation mapping</returns>
      object GetAnnotation(string key);

      /// <summary>
      /// Add the annotation to he set of message annotations or update the value stored
      /// with the given annotation key.
      /// </summary>
      /// <param name="key">The whose value is being added or updated</param>
      /// <param name="value">The value to store with the given key</param>
      /// <returns>the previous value stored with this key or null if none present</returns>
      object SetAnnotation(string key, object value);

      /// <summary>
      /// Removes the given annotation from the message if present and returns the value
      /// that was stored within.
      /// </summary>
      /// <param name="key">The annotation key whose value should be removed.</param>
      /// <returns>The annotation value removed or null if not present</returns>
      object RemoveAnnotation(string key);

      /// <summary>
      /// Efficient walk of all the current message annotations contained in this
      /// message.
      /// </summary>
      /// <param name="consumer">Function that will be called for each annotation</param>
      void ForEachAnnotation(Func<string, object> consumer);

      #endregion

      #region AMQP Application Properties Access

      /// <summary>
      /// Checks if the message carries any message properties.
      /// </summary>
      /// <returns>true if the message instance carries any propties</returns>
      bool HashProperties();

      /// <summary>
      /// Query the message to determine if the message carries the given property.
      /// </summary>
      /// <returns>true if the message instance carries the property</returns>
      bool HasProperty(string key);

      /// <summary>
      /// Returns the requested message property value from this message if it
      /// exists or returns null otherwise.
      /// </summary>
      /// <param name="key">The message property key</param>
      /// <returns>The value that is mapped to the given key</returns>
      object GetProperty(string key);

      /// <summary>
      /// Add the property to he set of message properties or update the value stored
      /// with the given mapping.
      /// </summary>
      /// <param name="key">The whose value is being added or updated</param>
      /// <param name="value">The value to store with the given key</param>
      /// <returns>the previous value stored with this key or null if none present</returns>
      object SetProperty(string key, object value);

      /// <summary>
      /// Removes the given property from the message if present and returns the value
      /// that was stored within.
      /// </summary>
      /// <param name="key">The property key which is to be removed</param>
      /// <returns>The property value removed or null if not present</returns>
      object RemoveProperty(string key);

      /// <summary>
      /// Efficient walk of all the current message properties contained in this
      /// message.
      /// </summary>
      /// <param name="consumer"></param>
      void ForEachProperty(Func<string, object> consumer);

      #endregion

      #region AMQP Message body access

      /// <summary>
      /// Returns true if the message allows reading of the message body.
      /// </summary>
      bool IsBodyReadable { get; }

      /// <summary>
      /// Returns true if the message allows writing of the message body.
      /// </summary>
      bool IsBodyWritable { get; }

      /// <summary>
      /// Access the body of this message. Depending on the current state of the message
      /// an exception might be thrown indicating that the body is not readable or is not
      /// writable.
      /// </summary>
      /// <returns>The message body</returns>
      /// <exception cref="ClientException">If the message body cannot be read or written</exception>
      T Body { get; set; }

      #endregion

      #region AMQP Footer access

      /// <summary>
      /// Checks if the message carries any footers.
      /// </summary>
      /// <returns>true if the message instance carries any footers</returns>
      bool HasFooters();

      /// <summary>
      /// Query the message to determine if the message carries the given footer entry.
      /// </summary>
      /// <returns>true if the message instance carries the footer entry</returns>
      bool HasFooter(string key);

      /// <summary>
      /// Returns the requested message footer value from this message if it
      /// exists or returns null otherwise.
      /// </summary>
      /// <param name="key">The message footer key</param>
      /// <returns>The value that is mapped to the given key</returns>
      object GetFooter(string key);

      /// <summary>
      /// Add the footer to he set of message footers or update the value stored
      /// with the given mapping.
      /// </summary>
      /// <param name="key">The whose value is being added or updated</param>
      /// <param name="value">The value to store with the given key</param>
      /// <returns>the previous value stored with this key or null if none present</returns>
      object SetFooter(string key, object value);

      /// <summary>
      /// Removes the given property from the message if present and returns the value
      /// that was stored within.
      /// </summary>
      /// <param name="key">The property key which is to be removed</param>
      /// <returns>The property value removed or null if not present</returns>
      object RemoveFooter(string key);

      /// <summary>
      /// Efficient walk of all the current message footers contained in this
      /// message.
      /// </summary>
      /// <param name="consumer"></param>
      void ForEachFooter(Func<string, object> consumer);

      #endregion

   }
}