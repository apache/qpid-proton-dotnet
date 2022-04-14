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
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Client.Exceptions;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public abstract class ClientLinkType<LinkType, ProtonLinkType> : IDisposable where LinkType : class, ILink
                                                                                where ProtonLinkType : class, Engine.ILink<ProtonLinkType>
   {
      protected readonly AtomicBoolean closed = new();
      protected readonly ClientSession session;

      protected ClientException failureCause;

      protected readonly TaskCompletionSource<LinkType> openFuture =
         new(TaskCreationOptions.RunContinuationsAsynchronously);
      protected readonly TaskCompletionSource<LinkType> closeFuture =
         new(TaskCreationOptions.RunContinuationsAsynchronously);

      protected volatile ISource remoteSource;
      protected volatile ITarget remoteTarget;

      protected ProtonLinkType protonLink;

      internal ClientLinkType(ClientSession session, ProtonLinkType protonLink)
      {
         this.session = session;
         this.protonLink = protonLink;
      }

      public IClient Client => session.Client;

      public IConnection Connection => session.Connection;

      public ISession Session => session;

      public Task<LinkType> OpenTask => openFuture.Task;

      public string Address
      {
         get
         {
            if (IsDynamic)
            {
               WaitForOpenToComplete();
               if (protonLink.IsSender)
               {
                  return (protonLink.RemoteTerminus as Types.Messaging.Target)?.Address;
               }
               else
               {
                  return (protonLink.RemoteSource as Types.Messaging.Source)?.Address;
               }
            }
            else
            {
               return protonLink.Target?.Address;
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
            return ClientConversionSupport.ToStringKeyedMap(protonLink.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonLink.RemoteOfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonLink.RemoteDesiredCapabilities);
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

      public Task<LinkType> CloseAsync(IErrorCondition error = null)
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

      public Task<LinkType> DetachAsync(IErrorCondition error = null)
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

         GC.SuppressFinalize(this);
      }

      #region Abstract and virtual Link APIs which the subclass might implement

      protected virtual Task<LinkType> DoCloseOrDetach(bool close, IErrorCondition error)
      {
         if (closed.CompareAndSet(false, true))
         {
            // Already closed by failure or shutdown so no need to
            if (!closeFuture.Task.IsCompleted)
            {
               session.Execute(() =>
               {
                  if (protonLink.IsLocallyOpen)
                  {
                     try
                     {
                        protonLink.ErrorCondition = ClientErrorCondition.AsProtonErrorCondition(error);
                        if (close)
                        {
                           protonLink.Close();
                        }
                        else
                        {
                           protonLink.Detach();
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

      #endregion

      #region Internal API used by other client resources

      internal bool IsClosed => closed;

      internal ClientSession ClientSession => session;

      internal bool IsDynamic => protonLink.IsSender ? protonLink.Target?.Dynamic ?? false :
                                                       protonLink.Source?.Dynamic ?? false;

      #endregion

      #region Protected API for subclasses to leverage

      protected void CheckClosedOrFailed()
      {
         if (IsClosed)
         {
            throw new ClientIllegalStateException("The Link was explicitly closed", failureCause);
         }
         else if (failureCause != null)
         {
            throw failureCause;
         }
      }

      protected void WaitForOpenToComplete()
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

      protected bool NotClosedOrFailed<T>(TaskCompletionSource<T> request)
      {
         return NotClosedOrFailed(request, protonLink);
      }

      protected bool NotClosedOrFailed<T>(TaskCompletionSource<T> request, ProtonLinkType sender)
      {
         if (IsClosed)
         {
            request.TrySetException(new ClientIllegalStateException("The link was explicitly closed", failureCause));
            return false;
         }
         else if (failureCause != null)
         {
            request.TrySetException(failureCause);
            return false;
         }
         else if (sender.IsLocallyClosedOrDetached)
         {
            if (sender.Connection.RemoteErrorCondition != null)
            {
               request.TrySetException(ClientExceptionSupport.ConvertToConnectionClosedException(sender.Connection.RemoteErrorCondition));
            }
            else if (sender.Session.RemoteErrorCondition != null)
            {
               request.TrySetException(ClientExceptionSupport.ConvertToSessionClosedException(sender.Session.RemoteErrorCondition));
            }
            else if (sender.Engine.FailureCause != null)
            {
               request.TrySetException(ClientExceptionSupport.ConvertToConnectionClosedException(sender.Engine.FailureCause));
            }
            else
            {
               request.TrySetException(new ClientIllegalStateException("Link closed without a specific error condition"));
            }
            return false;
         }
         else
         {
            return true;
         }
      }

      #endregion
   }
}