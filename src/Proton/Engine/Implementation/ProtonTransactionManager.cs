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
   public sealed class ProtonTransactionManager : ITransactionManager
   {
      private ProtonReceiver protonReceiver;

      public ProtonTransactionManager(ProtonReceiver protonReceiver)
      {
         this.protonReceiver = protonReceiver;
      }

      public uint Credit => throw new NotImplementedException();

      public Source Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Coordinator Coordinator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Source RemoteSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public Coordinator RemoteCoordinator { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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

      public ITransactionManager AddCredit(uint credit)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager Close()
      {
         throw new NotImplementedException();
      }

      public ITransactionManager CloseHandler(Action<ITransactionManager> localCloseHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager Declared(ITransaction<ITransactionManager> transaction, byte[] txnId)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager Declared(ITransaction<ITransactionManager> transaction, IProtonBuffer txnId)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager DeclareFailed(ITransaction<ITransactionManager> transaction, ErrorCondition condition)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager DeclareHandler(Action<ITransaction<ITransactionManager>> handler)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager Discharged(ITransaction<ITransactionManager> transaction)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager DischargeFailed(ITransaction<ITransactionManager> transaction, ErrorCondition condition)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager DischargeHandler(Action<ITransaction<ITransactionManager>> handler)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager EngineShutdownHandler(Action<IEngine> shutdownHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager LocalCloseHandler(Action<ITransactionManager> localCloseHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager LocalOpenHandler(Action<ITransactionManager> localOpenHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionManager Open()
      {
         throw new NotImplementedException();
      }

      public ITransactionManager OpenHandler(Action<ITransactionManager> localOpenHandler)
      {
         throw new NotImplementedException();
      }

      public ITransactionController ParentEndpointClosedHandler(Action<ITransactionController> handler)
      {
         throw new NotImplementedException();
      }
   }
}