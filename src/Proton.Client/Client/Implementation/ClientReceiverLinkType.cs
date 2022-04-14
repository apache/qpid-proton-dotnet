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
using System.Linq;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Base type used to implement common AMQP receiver operations shared by all the client's receiver types.
   /// </summary>
   /// <typeparam name="LinkType"></typeparam>
   public abstract class ClientReceiverLinkType<LinkType> : ClientLinkType<LinkType, Engine.IReceiver> where LinkType : class, ILink
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientReceiverLinkType<LinkType>>();

      protected TaskCompletionSource<LinkType> drainingFuture;

      protected readonly ReceiverOptions options;
      protected readonly string receiverId;

      internal ClientReceiverLinkType(ClientSession session, ReceiverOptions options, string receiverId, Engine.IReceiver protonLink) : base(session, protonLink)
      {
         this.protonLink.LinkedResource = Self;
         this.receiverId = receiverId;
         this.options = options;

         if (options.CreditWindow > 0)
         {
            protonLink.AddCredit(options.CreditWindow);
         }
      }

      public virtual int QueuedDeliveries => protonLink.Unsettled.Count(delivery => delivery.LinkedResource == null);

      public LinkType AddCredit(uint credit)
      {
         return AddCreditAsync(credit).ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<LinkType> AddCreditAsync(uint credit)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<LinkType> creditAdded = new();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(creditAdded))
            {
               if (options.CreditWindow != 0)
               {
                  creditAdded.TrySetException(new ClientIllegalStateException("Cannot add credit when a credit window has been configured"));
               }
               else if (protonLink.IsDraining)
               {
                  creditAdded.TrySetException(new ClientIllegalStateException("Cannot add credit while a drain is pending"));
               }
               else
               {
                  try
                  {
                     protonLink.AddCredit(credit);
                     creditAdded.TrySetResult(Self);
                  }
                  catch (Exception ex)
                  {
                     creditAdded.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(ex));
                  }
               }
            }
         });

         return creditAdded.Task;
      }

      public LinkType Drain()
      {
         return DrainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<LinkType> DrainAsync()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<LinkType> drainComplete = new();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(drainComplete))
            {
               if (protonLink.IsDraining)
               {
                  drainComplete.TrySetException(new ClientIllegalStateException("The Receiver is already draining"));
                  return;
               }

               try
               {
                  if (protonLink.Drain())
                  {
                     drainingFuture = drainComplete;
                     session.ScheduleRequestTimeout(drainingFuture, options.DrainTimeout,
                         () => new ClientOperationTimedOutException("Timed out waiting for remote to respond to drain request"));
                  }
                  else
                  {
                     drainComplete.TrySetResult(Self);
                  }
               }
               catch (Exception ex)
               {
                  drainComplete.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(ex));
               }
            }
         });

         return drainComplete.Task;
      }

      #region Non-Public receiver link APIs

      internal string ReceiverId => receiverId;

      protected void AsyncApplyDisposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         session.Execute(() =>
         {
            session.TransactionContext.Disposition(delivery, state, settle);
            ReplenishCreditIfNeeded();
         });
      }

      protected void AsyncReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            session.Execute(ReplenishCreditIfNeeded);
         }
      }

      protected void ReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            uint currentCredit = protonLink.Credit;
            if (currentCredit <= creditWindow * 0.5)
            {
               uint potentialPrefetch = currentCredit +
                  (uint)protonLink.Unsettled.Count(delivery => delivery.LinkedResource == null);

               if (potentialPrefetch <= creditWindow * 0.7)
               {
                  uint additionalCredit = creditWindow - potentialPrefetch;

                  LOG.Trace("Receiver granting additional credit: {0}", additionalCredit);
                  try
                  {
                     protonLink.AddCredit(additionalCredit);
                  }
                  catch (Exception ex)
                  {
                     LOG.Debug("Error caught during credit top-up", ex);
                  }
               }
            }
         }
      }

      #endregion

      #region Abstract receiver link APIs

      protected abstract LinkType Self { get; }

      #endregion
   }
}