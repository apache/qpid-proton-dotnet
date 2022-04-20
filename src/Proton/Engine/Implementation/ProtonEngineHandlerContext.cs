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
   /// A context object that is assigned to each new engine handler that is inserted into an
   /// engine handler pipeline.
   /// </summary>
   public class ProtonEngineHandlerContext : IEngineHandlerContext
   {
      /// <summary>
      /// The context indicator for a handler that wants to be sent read events.
      /// </summary>
      public static readonly uint HANDLER_READS = 1 << 1;

      /// <summary>
      /// The context indicator for a handler that wants to be sent write events.
      /// </summary>
      public static readonly uint HANDLER_WRITES = 1 << 2;

      /// <summary>
      /// The context indicator for a handler that wants to be sent all read and write events.
      /// </summary>
      public static readonly uint HANDLER_ALL_EVENTS = HANDLER_READS | HANDLER_WRITES;

      internal ProtonEngineHandlerContext previous;
      internal ProtonEngineHandlerContext next;

      private readonly string name;
      private readonly IEngine engine;
      private readonly IEngineHandler handler;

      private uint interestMask = HANDLER_ALL_EVENTS;

      public ProtonEngineHandlerContext(string name, IEngine engine, IEngineHandler handler)
      {
         this.name = name;
         this.engine = engine;
         this.handler = handler;
      }

      public IEngineHandler Handler => handler;

      public IEngine Engine => engine;

      public string Name => name;

      /// <summary>
      /// Allows a handler to indicate if it wants to be notified of a Engine Handler events for
      /// specific operations or opt into all engine handler events.  By opting out of the events
      /// that the handler does not process the call chain can be reduced when processing engine
      /// events.
      /// </summary>
      public uint InterestMask
      {
         get => interestMask;
         set => interestMask = value;
      }

      public virtual void FireEngineStarting()
      {
         next.InvokeEngineStarting();
      }

      public virtual void FireEngineStateChanged()
      {
         next.InvokeEngineStateChanged();
      }

      public virtual void FireFailed(EngineFailedException ex)
      {
         next.InvokeEngineFailed(ex);
      }

      public virtual void FireRead(IProtonBuffer buffer)
      {
         FindNextReadHandler().InvokeHandlerRead(buffer);
      }

      public virtual void FireRead(HeaderEnvelope header)
      {
         FindNextReadHandler().InvokeHandlerRead(header);
      }

      public virtual void FireRead(SaslEnvelope envelope)
      {
         FindNextReadHandler().InvokeHandlerRead(envelope);
      }

      public virtual void FireRead(IncomingAmqpEnvelope envelope)
      {
         FindNextReadHandler().InvokeHandlerRead(envelope);
      }

      public virtual void FireWrite(OutgoingAmqpEnvelope envelope)
      {
         FindNextWriteHandler().InvokeHandlerWrite(envelope);
      }

      public virtual void FireWrite(SaslEnvelope envelope)
      {
         FindNextWriteHandler().InvokeHandlerWrite(envelope);
      }

      public virtual void FireWrite(HeaderEnvelope envelope)
      {
         FindNextWriteHandler().InvokeHandlerWrite(envelope);
      }

      public virtual void FireWrite(IProtonBuffer buffer, Action ioComplete)
      {
         FindNextWriteHandler().InvokeHandlerWrite(buffer, ioComplete);
      }

      #region  Internal invoke of Engine and Handler state methods

      internal void InvokeEngineStarting()
      {
         handler.EngineStarting(this);
      }

      internal void InvokeEngineStateChanged()
      {
         handler.HandleEngineStateChanged(this);
      }

      internal void InvokeEngineFailed(EngineFailedException failure)
      {
         handler.EngineFailed(this, failure);
      }

      #endregion

      #region  Internal invoke of Read methods

      internal void InvokeHandlerRead(IncomingAmqpEnvelope envelope)
      {
         handler.HandleRead(this, envelope);
      }

      internal void InvokeHandlerRead(SaslEnvelope envelope)
      {
         handler.HandleRead(this, envelope);
      }

      internal void InvokeHandlerRead(HeaderEnvelope envelope)
      {
         handler.HandleRead(this, envelope);
      }

      internal void InvokeHandlerRead(IProtonBuffer buffer)
      {
         handler.HandleRead(this, buffer);
      }

      #endregion

      #region Internal invoke of Write methods

      internal void InvokeHandlerWrite(OutgoingAmqpEnvelope envelope)
      {
         handler.HandleWrite(this, envelope);
      }

      internal void InvokeHandlerWrite(SaslEnvelope envelope)
      {
         handler.HandleWrite(this, envelope);
      }

      internal void InvokeHandlerWrite(HeaderEnvelope envelope)
      {
         handler.HandleWrite(this, envelope);
      }

      internal void InvokeHandlerWrite(IProtonBuffer buffer, Action ioComplete)
      {
         next.Handler.HandleWrite(next, buffer, ioComplete);
      }

      #endregion

      private ProtonEngineHandlerContext FindNextReadHandler()
      {
         ProtonEngineHandlerContext ctx = this;
         do
         {
            ctx = ctx.previous;
         } while (SkipContext(ctx, HANDLER_READS));
         return ctx;
      }

      private ProtonEngineHandlerContext FindNextWriteHandler()
      {
         ProtonEngineHandlerContext ctx = this;
         do
         {
            ctx = ctx.next;
         } while (SkipContext(ctx, HANDLER_WRITES));
         return ctx;
      }

      private static bool SkipContext(ProtonEngineHandlerContext ctx, uint interestMask)
      {
         return (ctx.InterestMask & interestMask) == 0;
      }
   }
}