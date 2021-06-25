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
using System.IO;

namespace Apache.Qpid.Proton.Client.Impl
{
   public class ClientDelivery : IDelivery
   {
      private bool disposedValue;

      public IReceiver Receiver => throw new NotImplementedException();

      public uint MessageFormat => throw new NotImplementedException();

      public Stream RawInputStream => throw new NotImplementedException();

      public IDictionary<string, object> Annotations => throw new NotImplementedException();

      public bool Settled => throw new NotImplementedException();

      public IDeliveryState State => throw new NotImplementedException();

      public bool RemoteSettled => throw new NotImplementedException();

      public IDeliveryState RemoteState => throw new NotImplementedException();

      public IDelivery Accept()
      {
         throw new NotImplementedException();
      }

      public IDelivery Disposition(IDeliveryState state, bool settled)
      {
         throw new NotImplementedException();
      }

      public IMessage<T> Message<T>()
      {
         throw new NotImplementedException();
      }

      public IDelivery Modified(bool deliveryFailed, bool undeliverableHere)
      {
         throw new NotImplementedException();
      }

      public IDelivery Reject(string condition, string description)
      {
         throw new NotImplementedException();
      }

      public IDelivery Release()
      {
         throw new NotImplementedException();
      }

      public IDelivery Settle()
      {
         throw new NotImplementedException();
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
         }
      }

      public void Dispose()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
   }
}