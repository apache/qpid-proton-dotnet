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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   ///
   /// </summary>
   public sealed class ProtonTransactionController : ITransactionController
   {
      private ProtonSender sender;

      public ProtonTransactionController(ProtonSender sender)
      {
         this.sender = sender;
      }

      public IConnection Connection => throw new NotImplementedException();

      public bool HasCapacity => throw new NotImplementedException();

      public Source Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Coordinator Coordinator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Source RemoteSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Coordinator RemoteCoordinator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public IEnumerator<ITransaction<ITransactionController>> Transactions => throw new NotImplementedException();

      public IEngine Engine => throw new NotImplementedException();

      public IAttachments Attachments => throw new NotImplementedException();

      public object LinkedResource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public ErrorCondition ErrorCondition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public ErrorCondition RemoteCondition => throw new NotImplementedException();

      public bool IsLocallyOpen => throw new NotImplementedException();

      public bool IsLocallyClosed => throw new NotImplementedException();

      public bool IsRemotelyOpen => throw new NotImplementedException();

      public bool IsRemotelyClosed => throw new NotImplementedException();

      public Symbol[] OfferedCapabilities { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Symbol[] DesiredCapabilities { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public IReadOnlyDictionary<Symbol, object> Properties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public Symbol[] RemoteOfferedCapabilities => throw new NotImplementedException();

      public Symbol[] RemoteDesiredCapabilities => throw new NotImplementedException();

      public IReadOnlyDictionary<Symbol, object> RemoteProperties => throw new NotImplementedException();

      public ITransactionController AddCapacityAvailableHandler(Action<ITransactionController> handler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController Close()
      {
         throw new NotImplementedException();
      }

      public ITransactionController CloseHandler(Action<ITransactionController> localCloseHandler)
      {
         throw new NotImplementedException();
      }

      public ITransaction<ITransactionController> Declare()
      {
         throw new NotImplementedException();
      }

      public ITransactionController Declare(ITransaction<ITransactionController> transaction)
      {
         throw new NotImplementedException();
      }

      public ITransactionController DeclaredHandler(Action<ITransaction<ITransactionController>> handler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController DeclareFailedHandler(Action<ITransaction<ITransactionController>> handler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController Discharge(ITransaction<ITransactionController> transaction, bool failed)
      {
         throw new NotImplementedException();
      }

      public ITransactionController DischargeFailedHandler(Action<ITransaction<ITransactionController>> handler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController DischargeHandler(Action<ITransaction<ITransactionController>> handler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController EngineShutdownHandler(Action<IEngine> shutdownHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController LocalCloseHandler(Action<ITransactionController> localCloseHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController LocalOpenHandler(Action<ITransactionController> localOpenHandler)
      {
         throw new NotImplementedException();
      }

      public ITransaction<ITransactionController> NewTransaction()
      {
         throw new NotImplementedException();
      }

      public ITransactionController Open()
      {
         throw new NotImplementedException();
      }

      public ITransactionController OpenHandler(Action<ITransactionController> localOpenHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController ParentEndpointClosedHandler(Action<ITransactionController> handler)
      {
         throw new NotImplementedException();
      }
   }
}