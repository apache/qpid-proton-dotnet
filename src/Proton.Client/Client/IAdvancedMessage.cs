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
using System.Collections.ObjectModel;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP Message instance used by senders and receivers to provide a lower
   /// level abstraction around an AMQP message than the basic IMessage type but still
   /// provides the IMessage interface simpler access where needed.
   /// </summary>
   /// <typeparam name="T">The type that comprises the message body</typename>
   public interface IAdvancedMessage<T> : IMessage<T>
   {
      /// <summary>
      /// Creates a new advanced message instance using the Proton implementation.
      /// </summary>
      /// <typeparam name="E">The body that should be conveyed in the message</typeparam>
      /// <returns>A new advanced message instance.</returns>
      static new IAdvancedMessage<E> Create<E>() => IMessage<E>.Create<E>() as IAdvancedMessage<E>;

      /// <summary>
      /// Provides access to the AMQP Header instance that is carried in the message.
      /// </summary>
      Header Header { get; set; }

      /// <summary>
      /// Provides access to the AMQP Properties instance that is carried in the message.
      /// </summary>
      Properties Properties { get; set; }

      /// <summary>
      /// Provides access to the AMQP MessageAnnotations instance that is carried in the message.
      /// </summary>
      MessageAnnotations Annotations { get; set; }

      /// <summary>
      /// Provides access to the AMQP ApplicationProperties instance that is carried in the message.
      /// </summary>
      ApplicationProperties ApplicationProperties { get; set; }

      /// <summary>
      /// Provides access to the AMQP Footer instance that is carried in the message.
      /// </summary>
      Footer Footer { get; set; }

      /// <summary>
      /// Access the message format value present in this message.  The exact structure of a
      /// message, together with its encoding, is defined by the message format (default is
      /// the AMQP defined message format zero.
      /// <para>
      /// This field MUST be specified for the first transfer of a streamed message, if
      /// it is not set at the time of send of the first transfer the sender uses the AMQP
      /// default value of zero for this field.
      /// </para>
      /// <para>
      /// The upper three octets of a message format code identify a particular message format.
      /// The lowest octet indicates the version of said message format. Any given version of
      /// a format is forwards compatible with all higher versions.
      /// </para>
      /// </summary>
      uint MessageFormat { get; set; }

      /// <summary>
      /// Adds the given section to the internal collection of sections that will be sent
      /// to the remote peer when this message is encoded. If a previous section was added by
      /// a call to the set body method it should be retained as the first element of the
      /// running list of body sections contained in this message.
      /// </summary>
      /// <remarks>
      /// The implementation should make an attempt to validate that sections added are valid
      /// for the message format that is assigned when they are added.
      /// </remarks>
      /// <param name="section">The section to add to the collection of sections in this message</param>
      /// <returns>This advanced message instance.</returns>
      IAdvancedMessage<T> AddBodySection(Section section);

      /// <summary>
      /// Sets the body section instances to use when encoding this message. The value set
      /// replaces any existing sections assigned to this message through the add body sections
      /// API or the singular body set method.  Calling the set method with a null or empty
      /// collection is equivalent to calling the clear body sections method. The passed collection
      /// is copied and changes to it following calls to this method are not reflected in the
      /// collection contained in this message.
      /// </summary>
      /// <param name="sections">The collection of body sections to assign to this message</param>
      /// <returns>This advanced message instance.</returns>
      IAdvancedMessage<T> SetBodySections(ICollection<Section> section);

      /// <summary>
      /// Create and return an unmodifiable read-only collection that contains the section instances
      /// currently assigned to this message.
      /// </summary>
      /// <returns>a read-only view of the sections in this message's body</returns>
      ICollection<Section> GetBodySections();

      /// <summary>
      /// Clears all currently set body sections from this message instance.
      /// </summary>
      /// <returns>This advanced message instance.</returns>
      IAdvancedMessage<T> ClearBodySections();

      /// <summary>
      /// Efficient enumeration over all currently assigned body sections in this message.
      /// </summary>
      /// <param name="consumer">Function to invoke for each section in the message</param>
      /// <returns>This advanced message instance.</returns>
      IAdvancedMessage<T> ForEachBodySection(Func<Section> consumer);

      /// <summary>
      /// Encodes the advanced message for transmission by the client. The provided delivery
      /// annotations can be included or augmented by the advanced implementation based on the
      /// target message format. The implementation is responsible for ensuring that the delivery
      /// annotations are treated correctly encoded into the correct location in the message.
      /// </summary>
      /// <param name="deliveryAnnotations">Options delivery annotations to encode with the message</param>
      /// <returns>The encoded message bytes in a proton buffer instance</returns>
      IProtonBuffer Encode(IDictionary<string, object> deliveryAnnotations);

   }
}