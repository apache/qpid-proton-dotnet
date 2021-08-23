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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Base endpoint class that provides some of the most common endpoint
   /// implementations and some API for internal use when dealing with events.
   /// </summary>
   public abstract class ProtonEndpoint<T> : IEndpoint<T> where T : IEndpoint<T>
   {
      protected readonly ProtonEngine engine;

      private ProtonAttachments attachments;
      private Object linkedResource;

      private ErrorCondition localError;
      private ErrorCondition remoteError;

      private Action<T> remoteOpenHandler;
      private Action<T> remoteCloseHandler;
      private Action<T> localOpenHandler;
      private Action<T> localCloseHandler;
      private Action<IEngine> engineShutdownHandler;

      /// <summary>
      /// Creates a new instance of this endpoint implementation which is owned by
      /// the provided engine instance.
      /// </summary>
      /// <param name="engine">The engine that owns this endpoint instance.</param>
      public ProtonEndpoint(ProtonEngine engine)
      {
         this.engine = engine;
      }

      #region Endpoint API implemtations

      public virtual IEngine Engine => engine;

      internal ProtonEngine ProtonEngine => engine;

      public IAttachments Attachments => attachments != null ? attachments : attachments = new ProtonAttachments();

      public object LinkedResource
      {
         get => linkedResource;
         set => linkedResource = value;
      }

      public ErrorCondition ErrorCondition
      {
         get => localError;
         set => localError = value;
      }

      public ErrorCondition RemoteCondition
      {
         get => remoteError;
         internal set => remoteError = value;
      }

      public T OpenHandler(Action<T> openHandler)
      {
         this.remoteOpenHandler = openHandler;
         return Self();
      }

      internal bool HasOpenHandler => remoteOpenHandler != null;

      internal virtual T FireRemoteOpen()
      {
         remoteOpenHandler?.Invoke(Self());
         return Self();
      }

      public T CloseHandler(Action<T> closeHandler)
      {
         this.remoteCloseHandler = closeHandler;
         return Self();
      }

      internal bool HasCloseHandler => remoteCloseHandler != null;

      internal T FireRemoteClose()
      {
         remoteCloseHandler?.Invoke(Self());
         return Self();
      }

      public T LocalOpenHandler(Action<T> localOpenHandler)
      {
         this.localOpenHandler = localOpenHandler;
         return Self();
      }

      internal bool HasLocalOpenHandler => localOpenHandler != null;

      internal T FireLocalOpen()
      {
         localOpenHandler?.Invoke(Self());
         return Self();
      }

      public T LocalCloseHandler(Action<T> localCloseHandler)
      {
         this.localCloseHandler = localCloseHandler;
         return Self();
      }

      internal bool HasLocalCloseHandler => localCloseHandler != null;

      internal T FireLocalClose()
      {
         localCloseHandler?.Invoke(Self());
         return Self();
      }

      public T EngineShutdownHandler(Action<IEngine> shutdownHandler)
      {
         this.engineShutdownHandler = shutdownHandler;
         return Self();
      }

      internal bool HasEngineShutdownHandler => engineShutdownHandler != null;

      internal T FireEngineShutdown()
      {
         engineShutdownHandler?.Invoke(engine);
         return Self();
      }

      #endregion

      #region Abstract Endpoint API implemented in the derived classes

      internal abstract T Self();

      public abstract T Close();

      public abstract T Open();

      public abstract bool IsLocallyOpen { get; }

      public abstract bool IsLocallyClosed { get; }

      public abstract bool IsRemotelyOpen { get; }

      public abstract bool IsRemotelyClosed { get; }

      public abstract Symbol[] OfferedCapabilities { get; set; }

      public abstract Symbol[] DesiredCapabilities { get; set; }

      public abstract Symbol[] RemoteOfferedCapabilities { get; }

      public abstract Symbol[] RemoteDesiredCapabilities { get; }

      public abstract IReadOnlyDictionary<Symbol, object> Properties { get; set; }

      public abstract IReadOnlyDictionary<Symbol, object> RemoteProperties { get; }

      #endregion
   }
}