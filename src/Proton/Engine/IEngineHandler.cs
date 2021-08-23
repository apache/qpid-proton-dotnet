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
   /// Handler of engine events that is queued into the events pipeline.
   /// </summary>
   public interface IEngineHandler
   {
      /// <summary>
      /// Called when the handler is successfully added to the engine pipeline
      /// and will later be initialized before use.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      void HandlerAdded(IEngineHandlerContext context) { }

      /// <summary>
      /// Called when the handler is successfully removed from the engine pipeline
      /// and will not be invoked again or ever.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      void HandlerRemoved(IEngineHandlerContext context) { }

      /// <summary>
      /// Called when the engine is started to allow handlers to prepare for use based
      /// on the configuration state at start of the engine. A handler can fail the engine
      /// start by throwing an exception.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      void EngineStarting(IEngineHandlerContext context) { }

      /// <summary>
      /// Called when the engine state has changed and handlers may need to update their
      /// internal state to respond to the change or prompt some new work based on the change,
      /// e.g state changes from not writable to writable.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      void HandleEngineStateChanged(IEngineHandlerContext context)
      {
         context.FireEngineStateChanged();
      }

      /// <summary>
      /// Called when the engine has transitioned to a failed state and cannot process
      /// any additional input or output. The handler can free and resources used for
      /// normal operations at this point as the engine is now considered shutdown.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="failure">The exception that caused the engine to fail</param>
      void EngineFailed(IEngineHandlerContext context, EngineFailedException failure)
      {
         context.FireFailed(failure);
      }

      /// <summary>
      /// Handle the read of new incoming bytes from a remote sender. The handler should
      /// generally decode these bytes into an AMQP Performative or SASL Performative
      /// based on the current state of the connection and the handler in question.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="buffer">The buffer containing the incoming bytes read.</param>
      void HandleRead(IEngineHandlerContext context, IProtonBuffer buffer)
      {
         context.FireRead(buffer);
      }

      /// <summary>
      /// Handle the receipt of an incoming AMQP Header or SASL Header based on the
      /// current state of this handler.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="envelope">The envelope that was read</param>
      void HandleRead(IEngineHandlerContext context, HeaderEnvelope envelope)
      {
         context.FireRead(envelope);
      }

      /// <summary>
      /// Handle the receipt of an incoming SASL performative envelope based on the
      /// current state of this handler.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="envelope">The envelope that was read</param>
      void HandleRead(IEngineHandlerContext context, SaslEnvelope envelope)
      {
         context.FireRead(envelope);
      }

      /// <summary>
      /// Handle the receipt of an incoming AMQP performative envelope based on the
      /// current state of this handler.
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="envelope">The envelope that was read</param>
      void HandleRead(IEngineHandlerContext context, IncomingAmqpEnvelope envelope)
      {
         context.FireRead(envelope);
      }

      /// <summary>
      /// Handles write of AMQP Header either by directly writing it to the output target
      /// or by converting it to bytes and firing a write using the buffer based write API
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="envelope">The envelope that is to be written</param>
      void HandleWrite(IEngineHandlerContext context, HeaderEnvelope envelope)
      {
         context.FireWrite(envelope);
      }

      /// <summary>
      /// Handles write of SASL performative either by directly writing it to the output target
      /// or by converting it to bytes and firing a write using the buffer based write API
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="envelope">The envelope that is to be written</param>
      void HandleWrite(IEngineHandlerContext context, SaslEnvelope envelope)
      {
         context.FireWrite(envelope);
      }

      /// <summary>
      /// Handles write of AMQP performative either by directly writing it to the output target
      /// or by converting it to bytes and firing a write using the buffer based write API
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="envelope">The envelope that is to be written</param>
      void HandleWrite(IEngineHandlerContext context, OutgoingAmqpEnvelope envelope)
      {
         context.FireWrite(envelope);
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="context">The handler context that is assigned to this handler</param>
      /// <param name="buffer">The buffer to be written into the IO layer</param>
      /// <param name="ioComplete">The delegate to invoke when the IO operation is complete</param>
      void HandleWrite(IEngineHandlerContext context, IProtonBuffer buffer, Action ioComplete)
      {
         context.FireWrite(buffer, ioComplete);
      }
   }
}
