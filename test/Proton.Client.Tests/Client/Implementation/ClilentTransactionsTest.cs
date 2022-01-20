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

using System.Threading;
using Apache.Qpid.Proton.Test.Driver;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Types.Transport;
using System;
using Apache.Qpid.Proton.Types.Transactions;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientTransactionsTest : ClientBaseTestFixture
   {
      [Test]
      public void TestCoordinatorLinkSupportedOutcomes()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().WithSource().WithOutcomes(Accepted.DescriptorSymbol.ToString(),
                                                                     Rejected.DescriptorSymbol.ToString(),
                                                                     Released.DescriptorSymbol.ToString(),
                                                                     Modified.DescriptorSymbol.ToString()).And().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();
            session.CommitTransaction();

            session.CloseAsync();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestTimedOutExceptionOnBeginWithNoResponse()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               RequestTimeout = 50
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession().OpenTask.Result;

            try
            {
               session.BeginTransaction();
               Assert.Fail("Begin should have timed out after no response.");
            }
            catch (ClientTransactionDeclarationException)
            {
               // Expect this to time out.
            }

            try
            {
               session.CommitTransaction();
               Assert.Fail("Commit should have failed due to no active transaction.");
            }
            catch (ClientIllegalStateException)
            {
               // Expect this to fail since transaction not declared
            }

            try
            {
               session.RollbackTransaction();
               Assert.Fail("Rollback should have failed due to no active transaction.");
            }
            catch (ClientIllegalStateException)
            {
               // Expect this to fail since transaction not declared
            }

            session.CloseAsync();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestTimedOutExceptionOnBeginWithNoResponseThenRecoverWithNextBegin()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare();
            peer.ExpectDetach().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               RequestTimeout = 150
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession().OpenTask.Result;

            try
            {
               session.BeginTransaction();
               Assert.Fail("Begin should have timed out after no response.");
            }
            catch (ClientTransactionDeclarationException)
            {
               // Expect this to time out.
            }

            try
            {
               session.CommitTransaction();
               Assert.Fail("Commit should have failed due to no active transaction.");
            }
            catch (ClientIllegalStateException)
            {
               // Expect this to fail since transaction not declared
            }

            try
            {
               session.RollbackTransaction();
               Assert.Fail("Rollback should have failed due to no active transaction.");
            }
            catch (ClientIllegalStateException)
            {
               // Expect this to fail since transaction not declared
            }

            peer.WaitForScriptToComplete();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept();
            peer.ExpectDischarge().Accept();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            session.BeginTransaction();
            session.CommitTransaction();
            session.CloseAsync().Wait();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestExceptionOnBeginWhenCoordinatorLinkRefused()
      {
         string errorMessage = "CoordinatorLinkRefusal-breadcrumb";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Reject(true, AmqpError.NOT_IMPLEMENTED.ToString(), errorMessage);
            peer.ExpectDetach();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            try
            {
               session.BeginTransaction();
               Assert.Fail("Begin should have failed after link closed.");
            }
            catch (ClientTransactionDeclarationException expected)
            {
               // Expect this to time out.
               string message = expected.Message;
               Assert.IsTrue(message.Contains(errorMessage));
            }

            try
            {
               session.CommitTransaction();
               Assert.Fail("Commit should have failed due to no active transaction.");
            }
            catch (ClientTransactionNotActiveException)
            {
               // Expect this as the begin failed on coordinator rejected
            }

            try
            {
               session.RollbackTransaction();
               Assert.Fail("Rollback should have failed due to no active transaction.");
            }
            catch (ClientTransactionNotActiveException)
            {
               // Expect this as the begin failed on coordinator rejected
            }

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestExceptionOnBeginWhenCoordinatorLinkClosedAfterDeclare()
      {
         string errorMessage = "CoordinatorLinkClosed-breadcrumb";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare();
            peer.RemoteDetach().WithClosed(true)
                               .WithErrorCondition(AmqpError.NOT_IMPLEMENTED.ToString(), errorMessage).Queue();
            peer.ExpectDetach();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            try
            {
               session.BeginTransaction();
               Assert.Fail("Begin should have failed after link closed.");
            }
            catch (ClientException expected)
            {
               // Expect this to time out.
               string message = expected.Message;
               Assert.IsTrue(message.Contains(errorMessage));
            }

            try
            {
               session.CommitTransaction();
               Assert.Fail("Commit should have failed due to no active transaction.");
            }
            catch (ClientTransactionNotActiveException)
            {
               // Expect this as the begin failed on coordinator close
            }

            try
            {
               session.RollbackTransaction();
               Assert.Fail("Rollback should have failed due to no active transaction.");
            }
            catch (ClientTransactionNotActiveException)
            {
               // Expect this as the begin failed on coordinator close
            }

            session.CloseAsync();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestExceptionOnBeginWhenCoordinatorLinkClosedAfterDeclareAllowsNewTransactionDeclaration()
      {
         string errorMessage = "CoordinatorLinkClosed-breadcrumb";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare();
            peer.RemoteDetach().WithClosed(true)
                               .WithErrorCondition(AmqpError.NOT_IMPLEMENTED.ToString(), errorMessage).Queue();
            peer.ExpectDetach();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept();
            peer.ExpectDischarge().Accept();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            try
            {
               session.BeginTransaction();
               Assert.Fail("Begin should have failed after link closed.");
            }
            catch (ClientException expected)
            {
               // Expect this to time out.
               string message = expected.Message;
               Assert.IsTrue(message.Contains(errorMessage));
            }

            // Try again and expect to return to normal state now.
            session.BeginTransaction();
            session.CommitTransaction();

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestExceptionOnCommitWhenCoordinatorLinkClosedAfterDischargeSent()
      {
         string errorMessage = "CoordinatorLinkClosed-breadcrumb";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept();
            peer.ExpectDischarge();
            peer.RemoteDetach().WithClosed(true)
                               .WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), errorMessage).Queue();
            peer.ExpectDetach();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept();
            peer.ExpectDischarge().Accept();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();

            try
            {
               session.CommitTransaction();
               Assert.Fail("Commit should have failed after link closed.");
            }
            catch (ClientTransactionRolledBackException expected)
            {
               // Expect this to time out.
               string message = expected.Message;
               Assert.IsTrue(message.Contains(errorMessage));
            }

            session.BeginTransaction();
            session.RollbackTransaction();

            session.CloseAsync().Wait();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestExceptionOnCommitWhenCoordinatorLinkClosedAfterTxnDeclaration()
      {
         DoTestExceptionOnDischargeWhenCoordinatorLinkClosedAfterTxnDeclaration(true);
      }

      [Test]
      public void TestExceptionOnRollbackWhenCoordinatorLinkClosedAfterTxnDeclaration()
      {
         DoTestExceptionOnDischargeWhenCoordinatorLinkClosedAfterTxnDeclaration(false);
      }

      private void DoTestExceptionOnDischargeWhenCoordinatorLinkClosedAfterTxnDeclaration(bool commit)
      {
         string errorMessage = "CoordinatorLinkClosed-breadcrumb";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept();
            peer.RemoteDetachLastCoordinatorLink().WithClosed(true)
                                                  .WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), errorMessage).Queue();
            peer.ExpectDischarge().Optional();  // No discharge if close processed before commit or rollback triggered
            peer.ExpectDetach();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();

            if (commit)
            {
               try
               {
                  session.CommitTransaction();
                  Assert.Fail("Commit should have failed after link closed.");
               }
               catch (ClientTransactionRolledBackException expected)
               {
                  // Expect this to time out.
                  string message = expected.Message;
                  Assert.IsTrue(message.Contains(errorMessage));
               }
            }
            else
            {
               try
               {
                  session.RollbackTransaction();
               }
               catch (Exception ex)
               {
                  logger.LogDebug("Caught unexpected exception from rollback", ex);
                  Assert.Fail("Rollback should not have failed after link closed.");
               }
            }

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestExceptionOnCommitWhenCoordinatorRejectsDischarge()
      {
         string errorMessage = "Transaction aborted due to timeout";
         byte[] txnId1 = new byte[] { 0, 1, 2, 3 };
         byte[] txnId2 = new byte[] { 1, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(4).Queue();
            peer.ExpectDeclare().Accept(txnId1);
            peer.ExpectDischarge().WithFail(false)
                                  .WithTxnId(txnId1)
                                  .Reject(TransactionError.TRANSACTION_TIMEOUT.ToString(), "Transaction aborted due to timeout");
            peer.ExpectDeclare().Accept(txnId2);
            peer.ExpectDischarge().WithFail(true).WithTxnId(txnId2).Accept();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();

            try
            {
               session.CommitTransaction();
               Assert.Fail("Commit should have failed after link closed.");
            }
            catch (ClientTransactionRolledBackException expected)
            {
               // Expect this to time out.
               string message = expected.Message;
               Assert.IsTrue(message.Contains(errorMessage));
            }

            session.BeginTransaction();
            session.RollbackTransaction();

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}