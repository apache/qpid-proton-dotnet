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
   /// Implements the pipeline of engine read and write handlers used by the
   /// proton engine to read and write AMQP performatives.
   /// </summary>
   public sealed class ProtonEnginePipeline : IEnginePipeline
   {
      private readonly ProtonEngine engine;

      private readonly EngineHandlerContextReadBoundary head;
      private readonly EngineHandlerContextWriteBoundary tail;

      public ProtonEnginePipeline(ProtonEngine engine)
      {
         this.engine = engine ?? throw new ArgumentNullException(nameof(engine), "Parent transport cannot be null");

         head = new EngineHandlerContextReadBoundary(this);
         tail = new EngineHandlerContextWriteBoundary(this);

         // Ensure Pipeline starts out empty but initialized.
         head.next = tail;
         head.previous = head;

         tail.previous = head;
         tail.next = tail;
      }

      public IEngine Engine => engine;

      #region Handler configuration API

      public IEnginePipeline AddFirst(string name, IEngineHandler handler)
      {
         if (name == null || name.Length == 0)
         {
            throw new ArgumentException("Handler name cannot be null or empty");
         }

         if (handler == null)
         {
            throw new ArgumentNullException(nameof(handler), "Handler provided cannot be null");
         }

         ProtonEngineHandlerContext oldFirst = head.next;
         ProtonEngineHandlerContext newFirst = CreateContext(name, handler);

         newFirst.next = oldFirst;
         newFirst.previous = head;

         oldFirst.previous = newFirst;
         head.next = newFirst;

         try
         {
            newFirst.Handler.HandlerAdded(newFirst);
         }
         catch (Exception e)
         {
            engine.EngineFailed(e);
         }

         return this;
      }

      public IEnginePipeline AddLast(string name, IEngineHandler handler)
      {
         if (name == null || name.Length == 0)
         {
            throw new ArgumentException("Handler name cannot be null or empty");
         }

         if (handler == null)
         {
            throw new ArgumentNullException(nameof(handler), "Handler provided cannot be null");
         }

         ProtonEngineHandlerContext oldLast = tail.previous;
         ProtonEngineHandlerContext newLast = CreateContext(name, handler);

         newLast.next = tail;
         newLast.previous = oldLast;

         oldLast.next = newLast;
         tail.previous = newLast;

         try
         {
            newLast.Handler.HandlerAdded(newLast);
         }
         catch (Exception e)
         {
            engine.EngineFailed(e);
         }

         return this;
      }

      public IEnginePipeline RemoveFirst()
      {
         if (head.next != tail)
         {
            ProtonEngineHandlerContext oldFirst = head.next;

            head.next = oldFirst.next;
            head.next.previous = head;

            try
            {
               oldFirst.Handler.HandlerRemoved(oldFirst);
            }
            catch (Exception e)
            {
               engine.EngineFailed(e);
            }
         }

         return this;
      }

      public IEnginePipeline RemoveLast()
      {
         if (tail.previous != head)
         {
            ProtonEngineHandlerContext oldLast = tail.previous;

            tail.previous = oldLast.previous;
            tail.previous.next = tail;

            try
            {
               oldLast.Handler.HandlerRemoved(oldLast);
            }
            catch (Exception e)
            {
               engine.EngineFailed(e);
            }
         }

         return this;
      }

      public IEnginePipeline Remove(string name)
      {
         if (name != null && name.Length > 0)
         {
            ProtonEngineHandlerContext current = head.next;
            ProtonEngineHandlerContext removed = null;
            while (current != tail)
            {
               if (current.Name.Equals(name))
               {
                  removed = current;

                  ProtonEngineHandlerContext newNext = current.next;

                  current.previous.next = newNext;
                  newNext.previous = current.previous;

                  break;
               }

               current = current.next;
            }

            if (removed != null)
            {
               try
               {
                  removed.Handler.HandlerRemoved(removed);
               }
               catch (Exception e)
               {
                  engine.EngineFailed(e);
               }
            }
         }

         return this;
      }

      public IEnginePipeline Remove(IEngineHandler handler)
      {
         if (handler != null)
         {
            ProtonEngineHandlerContext current = head.next;
            ProtonEngineHandlerContext removed = null;
            while (current != tail)
            {
               if (current.Handler == handler)
               {
                  removed = current;

                  ProtonEngineHandlerContext newNext = current.next;

                  current.previous.next = newNext;
                  newNext.previous = current.previous;

                  break;
               }

               current = current.next;
            }

            if (removed != null)
            {
               try
               {
                  removed.Handler.HandlerRemoved(removed);
               }
               catch (Exception e)
               {
                  engine.EngineFailed(e);
               }
            }
         }

         return this;
      }

      public IEngineHandler Find(string name)
      {
         IEngineHandler handler = null;

         if (name != null && name.Length > 0)
         {
            ProtonEngineHandlerContext current = head.next;
            while (current != tail)
            {
               if (current.Name.Equals(name))
               {
                  handler = current.Handler;
                  break;
               }

               current = current.next;
            }
         }

         return handler;
      }

      public IEngineHandler First()
      {
         return head.next == tail ? null : head.next.Handler;
      }

      public IEngineHandler Last()
      {
         return tail.previous == head ? null : tail.previous.Handler;
      }

      public IEngineHandlerContext FirstContext()
      {
         return head.next == tail ? null : head.next;
      }

      public IEngineHandlerContext LastContext()
      {
         return tail.previous == head ? null : tail.previous;
      }

      #endregion

      #region Engine event propagation APIs

      public IEnginePipeline FireEngineStarting()
      {
         ProtonEngineHandlerContext current = head;
         while (current != tail)
         {
            if (engine.EngineState < EngineState.ShuttingDown)
            {
               try
               {
                  current.FireEngineStarting();
               }
               catch (Exception error)
               {
                  engine.EngineFailed(error);
                  break;
               }
               current = current.next;
            }
         }
         return this;
      }

      public IEnginePipeline FireEngineStateChanged()
      {
         try
         {
            head.FireEngineStateChanged();
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
         }
         return this;
      }

      public IEnginePipeline FireFailed(EngineFailedException failure)
      {
         try
         {
            head.FireFailed(failure);
         }
         catch (Exception)
         {
            // Ignore errors from handlers as engine is already failed.
         }
         return this;
      }

      public IEnginePipeline FireRead(IProtonBuffer input)
      {
         try
         {
            tail.FireRead(input);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      public IEnginePipeline FireRead(HeaderEnvelope header)
      {
         try
         {
            tail.FireRead(header);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      public IEnginePipeline FireRead(SaslEnvelope envelope)
      {
         try
         {
            tail.FireRead(envelope);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      public IEnginePipeline FireRead(IncomingAmqpEnvelope envelope)
      {
         try
         {
            tail.FireRead(envelope);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      public IEnginePipeline FireWrite(HeaderEnvelope envelope)
      {
         try
         {
            head.FireWrite(envelope);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      public IEnginePipeline FireWrite(OutgoingAmqpEnvelope envelope)
      {
         try
         {
            head.FireWrite(envelope);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      public IEnginePipeline FireWrite(SaslEnvelope envelope)
      {
         try
         {
            head.FireWrite(envelope);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      public IEnginePipeline FireWrite(IProtonBuffer buffer, Action ioComplete)
      {
         try
         {
            head.FireWrite(buffer, ioComplete);
         }
         catch (Exception error)
         {
            engine.EngineFailed(error);
            throw;
         }
         return this;
      }

      #endregion

      #region Internal API and Synthetic handler context that bounds the pipeline

      private ProtonEngineHandlerContext CreateContext(string name, IEngineHandler handler)
      {
         return new ProtonEngineHandlerContext(name, engine, handler);
      }

      private class EngineHandlerContextReadBoundary : ProtonEngineHandlerContext
      {
         public EngineHandlerContextReadBoundary(ProtonEnginePipeline pipeline) :
            base("Read Boundary", pipeline.engine, new BoundaryEngineHandler(pipeline.engine))
         {
         }

         public override void FireRead(IProtonBuffer buffer)
         {
            throw Engine.EngineFailed(new ProtonException("No handler processed Transport read event."));
         }

         public override void FireRead(HeaderEnvelope header)
         {
            throw Engine.EngineFailed(new ProtonException("No handler processed AMQP Header event."));
         }

         public override void FireRead(SaslEnvelope envelope)
         {
            throw Engine.EngineFailed(new ProtonException("No handler processed SASL performative event."));
         }

         public override void FireRead(IncomingAmqpEnvelope envelope)
         {
            throw Engine.EngineFailed(new ProtonException("No handler processed protocol performative event."));
         }
      }

      private class EngineHandlerContextWriteBoundary : ProtonEngineHandlerContext
      {
         private readonly ProtonEnginePipeline pipeline;

         public EngineHandlerContextWriteBoundary(ProtonEnginePipeline pipeline) :
            base("Write Boundary", pipeline.engine, new BoundaryEngineHandler(pipeline.engine))
         {
            this.pipeline = pipeline;
         }

         public override void FireWrite(HeaderEnvelope envelope)
         {
            throw Engine.EngineFailed(new ProtonException("No handler processed write AMQP Header event."));
         }

         public override void FireWrite(OutgoingAmqpEnvelope envelope)
         {
            throw Engine.EngineFailed(new ProtonException("No handler processed write AMQP performative event."));
         }

         public override void FireWrite(SaslEnvelope envelope)
         {
            throw Engine.EngineFailed(new ProtonException("No handler processed write SASL performative event."));
         }

         public override void FireWrite(IProtonBuffer buffer, Action ioComplete)
         {
            // When not handled in the handler chain the buffer write propagates to the
            // engine to be handed to any registered output handler.  The engine is then
            // responsible for error handling if nothing is registered there to handle the
            // output of frame data.
            try
            {
               pipeline.engine.DispatchWriteToEventHandler(buffer, ioComplete);
            }
            catch (Exception error)
            {
               throw Engine.EngineFailed(error);
            }
         }
      }

      #endregion

      #region Default TransportHandler Used at the pipeline boundary

      private class BoundaryEngineHandler : IEngineHandler
      {
         private readonly IEngine engine;

         public BoundaryEngineHandler(IEngine engine)
         {
            this.engine = engine;
         }

         public void EngineFailed(IEngineHandlerContext context, EngineFailedException failure)
         {
         }

         public void EngineStarting(IEngineHandlerContext context)
         {
         }

         public void HandleEngineStateChanged(IEngineHandlerContext context)
         {
         }

         public void HandlerAdded(IEngineHandlerContext context)
         {
         }

         public void HandlerRemoved(IEngineHandlerContext context)
         {
         }

         public void HandleRead(IEngineHandlerContext context, IProtonBuffer buffer)
         {
            throw engine.EngineFailed(new ProtonException("No handler processed Transport read event."));
         }

         public void HandleRead(IEngineHandlerContext context, HeaderEnvelope header)
         {
            throw engine.EngineFailed(new ProtonException("No handler processed AMQP Header event."));
         }

         public void HandleRead(IEngineHandlerContext context, SaslEnvelope envelope)
         {
            throw engine.EngineFailed(new ProtonException("No handler processed SASL performative read event."));
         }

         public void HandleRead(IEngineHandlerContext context, IncomingAmqpEnvelope envelope)
         {
            throw engine.EngineFailed(new ProtonException("No handler processed protocol performative read event."));
         }

         public void HandleWrite(IEngineHandlerContext context, HeaderEnvelope envelope)
         {
            throw engine.EngineFailed(new ProtonException("No handler processed write AMQP Header event."));
         }

         public void HandleWrite(IEngineHandlerContext context, OutgoingAmqpEnvelope envelope)
         {
            throw engine.EngineFailed(new ProtonException("No handler processed write AMQP performative event."));
         }

         public void HandleWrite(IEngineHandlerContext context, SaslEnvelope envelope)
         {
            throw engine.EngineFailed(new ProtonException("No handler processed write SASL performative event."));
         }

         public void HandleWrite(IEngineHandlerContext context, IProtonBuffer buffer, Action ioComplete)
         {
            context.FireWrite(buffer, ioComplete);
         }
      }

      #endregion
   }
}