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

namespace Apache.Qpid.Proton.Types.Transport
{
   /// <summary>
   /// Interface that defines a visitor for AMQP Header instances which can
   /// be used to process incoming and outgoing AMQP headers or other related
   /// processing.
   /// </summary>
   /// <typeparam name="E">The context that is provided for the event handler</typeparam>
   public interface IHeaderHandler<E>
   {
      /// <summary>
      /// Handles AMQP Header events
      /// </summary>
      /// <param name="header">The AMQP Header instance</param>
      /// <param name="context">The context provided to the event handler</param>
      void HandleAMQPHeader(AmqpHeader header, E context) { }

      /// <summary>
      /// Handles SASL Header events
      /// </summary>
      /// <param name="header">The SASL Header instance</param>
      /// <param name="context">The context provided to the event handler</param>
      void HandleSASLHeader(AmqpHeader header, E context) { }

   }
}