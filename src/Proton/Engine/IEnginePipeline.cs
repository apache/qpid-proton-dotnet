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
   /// The engine pipeline contains a list of handlers that deal with incoming and
   /// outgoing AMQP frames such as logging and encoders and decoders.
   /// </summary>
   public interface IEnginePipeline
   {
      /// <summary>
      /// Provides access to the engine that owns this engine pipeline.
      /// </summary>
      IEngine Engine { get; }

      /// <summary>
      /// Adds the given handler to the front of the pipeline with the given name
      /// stored for later lookup or remove operations. It is not mandatory that
      /// each handler have unique names although if handlers do share a name the
      /// remove method will only remove them one at a time starting from the first
      /// in the pipeline.
      /// </summary>
      /// <param name="name">The name to assign to the added handler</param>
      /// <param name="handler">The handler to add</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline AddFirst(string name, IEngineHandler handler);

      /// <summary>
      /// Adds the given handler to the end of the pipeline with the given name
      /// stored for later lookup or remove operations. It is not mandatory that
      /// each handler have unique names although if handlers do share a name the
      /// remove method will only remove them one at a time starting from the first
      /// in the pipeline.
      /// </summary>
      /// <param name="name">The name to assign to the added handler</param>
      /// <param name="handler">The handler to add</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline AddLast(string name, IEngineHandler handler);

      /// <summary>
      /// Removes the first handler in the pipeline.
      /// </summary>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline RemoveFirst();

      /// <summary>
      /// Removes the first handler in the pipeline.
      /// </summary>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline RemoveLast();

      /// <summary>
      /// Removes the first handler in the pipeline that is assigned the given name.
      /// </summary>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline Remove(string name);

      /// <summary>
      /// Removes the given handler if found in the pipeline.
      /// </summary>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline Remove(IEngineHandler handler);

      /// <summary>
      /// Finds and returns first handler that is found in the pipeline that matches the given name.
      /// </summary>
      /// <param name="name">The name to search for in the pipeline moving from first to last.</param>
      /// <returns>The removed handler or null if not found.</returns>
      IEngineHandler Find(string name);

      /// <summary>
      /// Finds and returns first handler in the pipeline.
      /// </summary>
      /// <returns>The removed handler or null if not found.</returns>
      IEngineHandler First();

      /// <summary>
      /// Finds and returns last handler in the pipeline.
      /// </summary>
      /// <returns>The removed handler or null if not found.</returns>
      IEngineHandler Last();

      /// <summary>
      /// Returns a reference to the first engine handler context in the pipeline.
      /// </summary>
      /// <returns>An engine handler context instance or null if no handlers in the pipeline</returns>
      IEngineHandlerContext FirstContext();

      /// <summary>
      /// Returns a reference to the last engine handler context in the pipeline.
      /// </summary>
      /// <returns>An engine handler context instance or null if no handlers in the pipeline</returns>
      IEngineHandlerContext LastContext();

      /// <summary>
      /// Fires an engine starting event to each handler in the pipeline. Should be used
      /// by the engine implementation to signal its handlers that they should initialize.
      /// </summary>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireEngineStarting();

      /// <summary>
      /// Fires an engine state changed event to each handler in the pipeline. Should be used
      /// by the engine implementation to signal its handlers that they should respond to the
      /// new engine state, e.g. the engine failed or was shutdown.
      /// </summary>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireEngineStateChanged();

      /// <summary>
      /// Fires a read event consisting of the given proton buffer into the pipeline starting
      /// from the last engine handler in the pipeline and moving through each until the
      /// incoming work is fully processed. If the read events reaches the head of the pipeline
      /// and is not handled by any handler an error is thrown and the engine should enter the
      /// failed state.
      /// </summary>
      /// <param name="input">The incoming bytes read in a proton buffer instance.</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireRead(IProtonBuffer input);

      /// <summary>
      /// Fires a read event consisting of the given header envelope into the pipeline
      /// starting from the last engine handler in the pipeline and moving through each
      /// until the incoming work is fully processed. If the read events reaches the head
      /// of the pipeline and is not handled by any handler an error is thrown and the
      /// engine should enter the failed state.
      /// </summary>
      /// <param name="header">The header envelope to process</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireRead(HeaderEnvelope header);

      /// <summary>
      /// Fires a read event consisting of the given SASL envelope into the pipeline
      /// starting from the last engine handler in the pipeline and moving through each
      /// until the incoming work is fully processed. If the read events reaches the head
      /// of the pipeline and is not handled by any handler an error is thrown and the
      /// engine should enter the failed state.
      /// </summary>
      /// <param name="envelope">The SASL envelope to process</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireRead(SaslEnvelope envelope);

      /// <summary>
      /// Fires a read event consisting of the given AMQP envelope into the pipeline
      /// starting from the last engine handler in the pipeline and moving through each
      /// until the incoming work is fully processed. If the read events reaches the head
      /// of the pipeline and is not handled by any handler an error is thrown and the
      /// engine should enter the failed state.
      /// </summary>
      /// <param name="envelope">The AMQP envelope to process</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireRead(IncomingAmqpEnvelope envelope);

      /// <summary>
      /// Fires a write event consisting of the given header envelope into the
      /// pipeline starting from the first engine in the pipeline and moving through
      /// each until the outgoing work is fully processed. If the write events reaches
      /// the tail of the pipeline and is not handled by any handler an error is thrown
      /// and the engine should enter the failed state.
      /// <para/>
      /// It is expected that after the fire write method returns the given envelope will
      /// have been written or if held for later the object must be copied.
      /// </summary>
      /// <param name="envelope">The SASL envelope to process</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireWrite(HeaderEnvelope envelope);

      /// <summary>
      /// Fires a write event consisting of the given AMQP envelope into the
      /// pipeline starting from the first engine in the pipeline and moving through
      /// each until the outgoing work is fully processed. If the write events reaches
      /// the tail of the pipeline and is not handled by any handler an error is thrown
      /// and the engine should enter the failed state.
      /// <para/>
      /// It is expected that after the fire write method returns the given envelope will
      /// have been written or if held for later the object must be copied.
      /// <para/>
      /// When the payload given exceeds the maximum allowed frame size when encoded into
      /// an outbound frame the encoding handler should either throw an error in the case
      /// that the performative being written cannot truncate its payload or should invoke
      /// the payload to large handler of the envelope before re-encoding the outbound
      /// performative and truncating the payload.
      /// </summary>
      /// <param name="envelope">The AMQP envelope to process</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireWrite(OutgoingAmqpEnvelope envelope);

      /// <summary>
      /// Fires a write event consisting of the given SASL envelope into the
      /// pipeline starting from the first engine in the pipeline and moving through
      /// each until the outgoing work is fully processed. If the write events reaches
      /// the tail of the pipeline and is not handled by any handler an error is thrown
      /// and the engine should enter the failed state.
      /// <para/>
      /// It is expected that after the fire write method returns the given envelope will
      /// have been written or if held for later the object must be copied.
      /// </summary>
      /// <param name="envelope">The SASL envelope to process</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireWrite(SaslEnvelope envelope);

      /// <summary>
      /// Fires a write event consisting of the given proton buffer into the
      /// pipeline starting from the first engine in the pipeline and moving through
      /// each until the outgoing work is fully processed. If the write events reaches
      /// the tail of the pipeline and is not handled by any handler an error is thrown
      /// and the engine should enter the failed state.
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="ioComplete">The delegate to invoke when the IO operation is complete</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireWrite(IProtonBuffer buffer, Action ioComplete);

      /// <summary>
      /// Fires an engine failed event into each {@link EngineHandler} in the pipeline
      /// indicating that the engine is now failed and should not accept or produce new
      /// work.
      /// </summary>
      /// <param name="failure">The error that indicates why the engine has failed</param>
      /// <returns>This engine pipeline instance.</returns>
      IEnginePipeline FireFailed(EngineFailedException failure);

   }
}