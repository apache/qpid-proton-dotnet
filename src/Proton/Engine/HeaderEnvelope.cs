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

using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Frame object that carries an AMQP Performative.
   /// </summary>
   public class HeaderEnvelope : PerformativeEnvelope<AmqpHeader>
   {
      public static readonly byte HeaderFrameType = (byte)1;

      /// <summary>
      /// A singleton instance of an SASL header that can be used to avoid additional allocations.
      /// </summary>
      public static readonly HeaderEnvelope SASL_HEADER_ENVELOPE = new HeaderEnvelope(AmqpHeader.GetSASLHeader());

      /// <summary>
      /// A singleton instance of an AMQP header that can be used to avoid additional allocations.
      /// </summary>
      public static readonly HeaderEnvelope AMQP_HEADER_ENVELOPE = new HeaderEnvelope(AmqpHeader.GetAMQPHeader());

      /// <summary>
      /// Create an header envelope with the given AMQHeader body.
      /// </summary>
      /// <param name="body">The AMQP Header to carry in this envelope</param>
      internal HeaderEnvelope(AmqpHeader body) : base(HeaderFrameType)
      {
         Initialize(body, 0, null);
      }

      /// <summary>
      /// Reads the protocol Id value from the conveyed header.
      /// </summary>
      public int ProtocolId { get => Body.ProtocolId; }

      /// <summary>
      /// Reads the major version number value from the conveyed header.
      /// </summary>
      public int Major { get => Body.Major; }

      /// <summary>
      /// Reads the minor version number value from the conveyed header.
      /// </summary>
      public int Minor { get => Body.Minor; }

      /// <summary>
      /// Reads the revision version number value from the conveyed header.
      /// </summary>
      public int Revision { get => Body.Revision; }

      /// <summary>
      /// Returns true if the conveyed header is a SASL header.
      /// </summary>
      public bool IsSaslHeader { get => Body.IsSaslHeader(); }

      /// <summary>
      /// Invoke the correct handler based on whether this header is a SASL or AMQP header instance.
      /// </summary>
      /// <typeparam name="T">The context type for the handler invocation</typeparam>
      /// <param name="handler">The header hander to invoke</param>
      /// <param name="context">The context to supply to the handler method.</param>
      public void invoke<T>(IHeaderHandler<T> handler, T context)
      {
         Body.Invoke(handler, context);
      }
   }
}