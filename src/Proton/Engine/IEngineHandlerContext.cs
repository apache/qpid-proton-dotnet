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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Context object that is provided to the engine handler APIs to allow
   /// for forwarding of events to the next handler or other updates.
   /// </summary>
   public interface IEngineHandlerContext
   {
      /// <summary>
      /// Access the engine handler that this context is assigned to.
      /// </summary>
      IEngineHandler Handler { get; }

      /// <summary>
      /// Access the engine instance where this context and its handler are assigned.
      /// </summary>
      IEngine Engine { get; }

      /// <summary>
      /// Access the name that was given to the handler assigned to this context.
      /// </summary>
      string Name { get; }

      /// <summary>
      /// Fires the engine starting event into the next handler in the engine pipeline.
      /// </summary>
      void FireEngineStarting();

      /// <summary>
      /// Fires the engine state changed event into the next handler in the {@link EnginePipeline}
      /// chain. The state change events occur after the engine starting event and generally signify
      /// that the engine has been shutdown normally.
      /// </summary>
      void FireEngineStateChanged();

      /// <summary>
      /// Fires the engine failed event into the next handler in the {engine pipeline chain.
      /// </summary>
      /// <param name="ex">The exception that triggered the failure</param>
      void FireFailed(EngineFailedException ex);

      /// <summary>
      /// Fires a read event into the previous handler in the engine pipeline for further
      /// processing or dispatch to the next in line.
      /// </summary>
      /// <param name="buffer">The buffer containing the bytes read</param>
      void FireRead(IProtonBuffer buffer);

      /// <summary>
      /// Fires a read of AMQP header events into the previous handler in the engine pipeline
      /// for further processing.
      /// </summary>
      /// <param name="header"></param>
      void FireRead(HeaderEnvelope header);

      /// <summary>
      /// Fires a read of SASL performative envelope events into the previous handler in the
      /// engine pipeline for further processing.
      /// </summary>
      /// <param name="envelope">The incoming envelope that was read.</param>
      void FireRead(SaslEnvelope envelope);

      /// <summary>
      /// Fires a read of AMQP performative envelope events into the previous handler in the
      /// engine pipeline for further processing.
      /// </summary>
      /// <param name="envelope">The incoming envelope that was read.</param>
      void FireRead(IncomingAmqpEnvelope envelope);

      /// <summary>
      /// Fires a write of the given AMQP performative envelope which should be passed along
      /// the engine pipeline for processing.
      /// </summary>
      /// <param name="envelope">The outgoing envelope that should be written.</param>
      void FireWrite(OutgoingAmqpEnvelope envelope);

      /// <summary>
      /// Fires a write of the given SASL performative envelope which should be passed along
      /// the engine pipeline for processing.
      /// </summary>
      /// <param name="envelope">The outgoing envelope that should be written.</param>
      void FireWrite(SaslEnvelope envelope);

      /// <summary>
      /// Fires a write of the given AMQP Header envelope which should be passed along
      /// the engine pipeline for processing.
      /// </summary>
      /// <param name="envelope">The outgoing envelope that should be written.</param>
      void FireWrite(HeaderEnvelope envelope);

      /// <summary>
      /// Fires a write of the given proton buffer into the engine pipeline for processing
      /// and provides an action delegate which should be invoked when the IO has written
      /// the bytes fully.
      /// </summary>
      /// <param name="buffer">The buffer that should be written to the IO layer</param>
      /// <param name="ioComplete">The completion action to invoke when the IO is done</param>
      void FireWrite(IProtonBuffer buffer, Action ioComplete);

   }
}
