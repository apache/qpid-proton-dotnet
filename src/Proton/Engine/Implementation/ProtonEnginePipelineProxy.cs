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

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Wrapper around the internal ProtonEnginePipeline used to present a guarded pipeline
   /// to the outside world when the Engine#pipeline method is used to gain access to the
   /// pipeline. The proxy will ensure that any read or write calls enforce Engine state
   /// such as not started and shutdown.
   /// </summary>
   public sealed class ProtonEnginePipelineProxy : IEnginePipeline
   {
      private readonly ProtonEnginePipeline pipeline;
      private readonly ProtonEngine engine;

      public ProtonEnginePipelineProxy(ProtonEnginePipeline pipeline) : base()
      {
         this.pipeline = pipeline;
         this.engine = (ProtonEngine)pipeline.Engine;
      }

      public IEngine Engine => pipeline.Engine;

      internal ProtonEnginePipeline Pipeline => pipeline;

      public IEnginePipeline AddFirst(string name, IEngineHandler handler)
      {
         engine.CheckShutdownOrFailed("Cannot add pipeline resources when Engine is shutdown or failed");
         pipeline.AddFirst(name, handler);
         return this;
      }

      public IEnginePipeline AddLast(string name, IEngineHandler handler)
      {
         engine.CheckShutdownOrFailed("Cannot add pipeline resources when Engine is shutdown or failed");
         pipeline.AddLast(name, handler);
         return this;
      }

      public IEnginePipeline RemoveFirst()
      {
         pipeline.RemoveFirst();
         return this;
      }

      public IEnginePipeline RemoveLast()
      {
         pipeline.RemoveLast();
         return this;
      }

      public IEnginePipeline Remove(string name)
      {
         pipeline.Remove(name);
         return this;
      }

      public IEnginePipeline Remove(IEngineHandler handler)
      {
         pipeline.Remove(handler);
         return this;
      }

      public IEngineHandler Find(string name)
      {
         engine.CheckShutdownOrFailed("Cannot access pipeline resource when Engine is shutdown or failed");
         return pipeline.Find(name);
      }

      public IEngineHandler First()
      {
         engine.CheckShutdownOrFailed("Cannot access pipeline resource when Engine is shutdown or failed");
         return pipeline.First();
      }

      public IEngineHandler Last()
      {
         engine.CheckShutdownOrFailed("Cannot access pipeline resource when Engine is shutdown or failed");
         return pipeline.Last();
      }

      public IEngineHandlerContext FirstContext()
      {
         engine.CheckShutdownOrFailed("Cannot access pipeline resource when Engine is shutdown or failed");
         return pipeline.FirstContext();
      }

      public IEngineHandlerContext LastContext()
      {
         engine.CheckShutdownOrFailed("Cannot access pipeline resource when Engine is shutdown or failed");
         return pipeline.LastContext();
      }

      public IEnginePipeline FireEngineStarting()
      {
         throw new InvalidOperationException("Cannot trigger starting on Engine owned Pipeline resource.");
      }

      public IEnginePipeline FireEngineStateChanged()
      {
         throw new InvalidOperationException("Cannot trigger state changed on Engine owned Pipeline resource.");
      }

      public IEnginePipeline FireFailed(EngineFailedException failure)
      {
         throw new InvalidOperationException("Cannot trigger failed on Engine owned Pipeline resource.");
      }

      public IEnginePipeline FireRead(IProtonBuffer input)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot inject new data into an Engine that is shutdown or failed");
         pipeline.FireRead(input);
         return this;
      }

      public IEnginePipeline FireRead(HeaderEnvelope header)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot inject new data into an Engine that is shutdown or failed");
         pipeline.FireRead(header);
         return this;
      }

      public IEnginePipeline FireRead(SaslEnvelope envelope)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot inject new data into an Engine that is shutdown or failed");
         pipeline.FireRead(envelope);
         return this;
      }

      public IEnginePipeline FireRead(IncomingAmqpEnvelope envelope)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot inject new data into an Engine that is shutdown or failed");
         pipeline.FireRead(envelope);
         return this;
      }

      public IEnginePipeline FireWrite(HeaderEnvelope envelope)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot write form an Engine that is shutdown or failed");

         pipeline.FireWrite(envelope);
         return this;
      }

      public IEnginePipeline FireWrite(OutgoingAmqpEnvelope envelope)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot write form an Engine that is shutdown or failed");

         pipeline.FireWrite(envelope);
         return this;
      }

      public IEnginePipeline FireWrite(SaslEnvelope envelope)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot write form an Engine that is shutdown or failed");

         pipeline.FireWrite(envelope);
         return this;
      }

      public IEnginePipeline FireWrite(IProtonBuffer buffer, Action ioComplete)
      {
         engine.CheckEngineNotStarted("Cannot inject new data into an not started Engine");
         engine.CheckShutdownOrFailed("Cannot write form an Engine that is shutdown or failed");

         pipeline.FireWrite(buffer, ioComplete);
         return this;
      }
   }
}