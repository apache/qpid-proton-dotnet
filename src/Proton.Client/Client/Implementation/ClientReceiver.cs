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
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// TODO
   /// </summary>
   public class ClientReceiver : IReceiver
   {
      public IClient Client => throw new NotImplementedException();

      public IConnection Connection => throw new NotImplementedException();

      public ISession Session => throw new NotImplementedException();

      public Task<IReceiver> OpenTask => throw new NotImplementedException();

      public string Address => throw new NotImplementedException();

      public ISource Source => throw new NotImplementedException();

      public ITarget Target => throw new NotImplementedException();

      public IDictionary<string, object> Properties => throw new NotImplementedException();

      public ICollection<string> OfferedCapabilities => throw new NotImplementedException();

      public ICollection<string> DesiredCapabilities => throw new NotImplementedException();

      public int QueuedDeliveries => throw new NotImplementedException();

      public IReceiver AddCredit(int credit)
      {
         throw new NotImplementedException();
      }

      public void Close()
      {
         throw new NotImplementedException();
      }

      public void Close(IErrorCondition error)
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> CloseAsync()
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> CloseAsync(IErrorCondition error)
      {
         throw new NotImplementedException();
      }

      public void Detach()
      {
         throw new NotImplementedException();
      }

      public void Detach(IErrorCondition error)
      {
         throw new NotImplementedException();
      }

      public Task<ISender> DetachAsync()
      {
         throw new NotImplementedException();
      }

      public Task<ISender> DetachAsync(IErrorCondition error)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> Drain()
      {
         throw new NotImplementedException();
      }

      public IDelivery Receive()
      {
         throw new NotImplementedException();
      }

      public IDelivery Receive(TimeSpan timeout)
      {
         throw new NotImplementedException();
      }

      public IDelivery TryReceive()
      {
         throw new NotImplementedException();
      }

      internal void Disposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         // TODO CheckClosedOrFailed();
         // asyncApplyDisposition(delivery, state, settle);
      }
   }
}