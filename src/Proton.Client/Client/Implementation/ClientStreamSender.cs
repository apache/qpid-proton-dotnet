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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Implements the stream sender using a stateful current outgoing message that prevents
   /// any sends other than to the current message until the current is completed.
   /// </summary>
   public sealed class ClientStreamSender : IStreamSender
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamSender>();

      private readonly AtomicBoolean closed = new AtomicBoolean();
      private ClientException failureCause;

      private readonly StreamSenderOptions options;
      private readonly ClientSession session;
      private readonly string senderId;
      private readonly bool sendsSettled;
      private readonly TaskCompletionSource<ISender> openFuture = new TaskCompletionSource<ISender>();
      private readonly TaskCompletionSource<ISender> closeFuture = new TaskCompletionSource<ISender>();

      private Engine.ISender protonSender;
      private Action<ISender> senderRemotelyClosedHandler;

      private volatile ISource remoteSource;
      private volatile ITarget remoteTarget;

      internal ClientStreamSender(ClientSession session, StreamSenderOptions options, string senderId, Engine.ISender protonSender)
      {
         this.options = new StreamSenderOptions(options);
         this.session = session;
         this.senderId = senderId;
         this.protonSender = protonSender;
         this.protonSender.LinkedResource = this;
         this.sendsSettled = protonSender.SenderSettleMode == Types.Transport.SenderSettleMode.Settled;
      }

      public IClient Client => session.Client;

      public IConnection Connection => session.Connection;

      public ISession Session => session;

      public Task<ISender> OpenTask => openFuture.Task;

      public string Address
      {
         get
         {
            if (IsDynamic)
            {
               WaitForOpenToComplete();
               return (protonSender.RemoteTerminus as ITarget)?.Address;
            }
            else
            {
               return protonSender.Target?.Address;
            }
         }
      }

      public ISource Source
      {
         get
         {
            WaitForOpenToComplete();
            return remoteSource;
         }
      }

      public ITarget Target
      {
         get
         {
            WaitForOpenToComplete();
            return remoteTarget;
         }
      }

      public IReadOnlyDictionary<string, object> Properties
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringKeyedMap(protonSender.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSender.OfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSender.DesiredCapabilities);
         }
      }

      public void Close(IErrorCondition error = null)
      {
         try
         {
            CloseAsync(error).Wait();
         }
         catch (Exception)
         {
         }
      }

      public Task<ISender> CloseAsync(IErrorCondition error = null)
      {
         return DoCloseOrDetach(true, error);
      }

      public void Detach(IErrorCondition error = null)
      {
         try
         {
            DetachAsync(error).Wait();
         }
         catch (Exception)
         {
         }
      }

      public Task<ISender> DetachAsync(IErrorCondition error = null)
      {
         return DoCloseOrDetach(false, error);
      }

      public void Dispose()
      {
         try
         {
            Close();
         }
         catch (Exception)
         {
         }
      }

      public ITracker Send<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         throw new NotImplementedException();
      }

      public ITracker TrySend<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         throw new NotImplementedException();
      }

      public IStreamSenderMessage BeginMessage(IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         DeliveryAnnotations annotations = null;
         TaskCompletionSource<IStreamSenderMessage> request = new TaskCompletionSource<IStreamSenderMessage>();

         if (deliveryAnnotations != null)
         {
            annotations = new DeliveryAnnotations(ClientConversionSupport.ToSymbolKeyedMap(deliveryAnnotations));
         }

         session.Execute(() =>
         {
            if (protonSender.Current != null)
            {
               request.TrySetException(new ClientIllegalStateException(
                   "Cannot initiate a new streaming send until the previous one is complete"));
            }
            else
            {
               // Grab the next delivery and hold for stream writes, no other sends
               // can occur while we hold the delivery.
               IOutgoingDelivery streamDelivery = protonSender.Next();
               ClientStreamTracker streamTracker = new ClientStreamTracker(this, streamDelivery);

               streamDelivery.LinkedResource = streamTracker;

               request.TrySetResult(new ClientStreamSenderMessage(this, streamTracker, annotations));
            }
         });

         return session.Request(this, request).Task.GetAwaiter().GetResult();
      }

      #region Internal Stream Sender API

      internal string SenderId => senderId;

      internal bool IsClosed => closed;

      internal bool IsDynamic => protonSender.Target?.Dynamic ?? false;

      internal Engine.ISender ProtonSender => protonSender;

      internal StreamSenderOptions Options => options;

      internal ClientStreamSender Open()
      {
         // TODO

         return this;
      }

      internal void Disposition(IOutgoingDelivery delivery, Types.Transport.IDeliveryState state, bool settled)
      {
         CheckClosedOrFailed();
         // TODO
         //   executor.execute(() -> {
         //       delivery.disposition(state, settled);
         //   });
      }

      internal void Abort(IOutgoingDelivery delivery, ClientStreamTracker tracker)
      {
         throw new NotImplementedException();
      }

      internal IStreamTracker SendMessage<E>(ClientStreamSenderMessage context, IAdvancedMessage<E> message)
      {
         throw new NotImplementedException();
      }

      internal void Complete(IOutgoingDelivery delivery, ClientStreamTracker tracker)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region Private Receiver Implementation

      private Task<ISender> DoCloseOrDetach(bool close, IErrorCondition error)
      {
         if (closed.CompareAndSet(false, true))
         {
            // Already closed by failure or shutdown so no need to
            if (!closeFuture.Task.IsCompleted)
            {
               session.Execute(() =>
               {
                  if (protonSender.IsLocallyOpen)
                  {
                     try
                     {
                        protonSender.ErrorCondition = ClientErrorCondition.AsProtonErrorCondition(error);
                        if (close)
                        {
                           protonSender.Close();
                        }
                        else
                        {
                           protonSender.Detach();
                        }
                     }
                     catch (Exception)
                     {
                        // The engine event handlers will deal with errors
                     }
                  }
               });
            }
         }

         return closeFuture.Task;
      }

      private void CheckClosedOrFailed()
      {
         if (IsClosed)
         {
            throw new ClientIllegalStateException("The Stream Sender was explicitly closed", failureCause);
         }
         else if (failureCause != null)
         {
            throw failureCause;
         }
      }

      private void WaitForOpenToComplete()
      {
         if (!openFuture.Task.IsCompleted || openFuture.Task.IsFaulted)
         {
            try
            {
               openFuture.Task.Wait();
            }
            catch (Exception e)
            {
               throw failureCause ?? ClientExceptionSupport.CreateNonFatalOrPassthrough(e);
            }
         }
      }

      #endregion
   }
}