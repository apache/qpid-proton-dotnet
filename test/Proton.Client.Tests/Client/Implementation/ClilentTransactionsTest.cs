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
using Apache.Qpid.Proton.Client.TestSupport;
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

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

      [Ignore("Issue with link not waiting for detach before using old handle")]
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
            peer.ExpectDetach().Respond().AfterDelay(25);
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

      [Test]
      public void TestExceptionOnRollbackWhenCoordinatorRejectsDischarge()
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
            peer.ExpectDischarge().WithFail(true)
                                  .WithTxnId(txnId1)
                                  .Reject(TransactionError.TRANSACTION_TIMEOUT.ToString(), "Transaction aborted due to timeout");
            peer.ExpectDeclare().Accept(txnId2);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId2).Accept();
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
               session.RollbackTransaction();
               Assert.Fail("Commit should have failed after link closed.");
            }
            catch (ClientTransactionRolledBackException expected)
            {
               // Expect this to time out.
               String message = expected.Message;
               Assert.IsTrue(message.Contains(errorMessage));
            }

            session.BeginTransaction();
            session.CommitTransaction();

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      /// <summary>
      /// Create a transaction and then close the Session which result in the remote rolling
      /// back the transaction by default so the client doesn't manually roll it back itself.
      /// </summary>
      [Test]
      public void TestBeginTransactionAndClose()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept();
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

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestBeginAndCommitTransaction()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
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

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestBeginAndRollbackTransaction()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.ExpectDischarge().WithFail(true).WithTxnId(txnId).Accept();
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
            session.RollbackTransaction();

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestTransactionDeclaredDispositionWithoutTxnId()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectDeclare().Accept(null);
            peer.ExpectClose().WithError(AmqpError.DECODE_ERROR.ToString(), "The txn-id field cannot be omitted").Respond();
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
               Assert.Fail("Should not complete transaction begin due to client connection failure on decode issue.");
            }
            catch (ClientException)
            {
               // expected to fail
            }
            catch (Exception ex)
            {
               Assert.Fail("Should have thrown a client exception but was: {}", ex);
            }

            connection.Close();

            peer.WaitForScriptToCompleteIgnoreErrors();
         }
      }

      [Test]
      public void TestBeginAndCommitTransactions()
      {
         byte[] txnId1 = new byte[] { 0, 1, 2, 3 };
         byte[] txnId2 = new byte[] { 1, 1, 2, 3 };
         byte[] txnId3 = new byte[] { 2, 1, 2, 3 };
         byte[] txnId4 = new byte[] { 3, 1, 2, 3 };
         byte[] txnId5 = new byte[] { 4, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.ExpectDeclare().Accept(txnId1);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId1).Accept();
            peer.ExpectDeclare().Accept(txnId2);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId2).Accept();
            peer.ExpectDeclare().Accept(txnId3);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId3).Accept();
            peer.ExpectDeclare().Accept(txnId4);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId4).Accept();
            peer.ExpectDeclare().Accept(txnId5);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId5).Accept();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            for (int i = 0; i < 5; ++i)
            {
               logger.LogInformation("Transaction declare and discharge cycle: {}", i);
               session.BeginTransaction();
               session.CommitTransaction();
            }

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotBeginSecondTransactionWhileFirstIsActive()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept();
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
               session.BeginTransaction();
               Assert.Fail("Should not be allowed to begin another transaction");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessageInsideOfTransaction()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.ExpectTransfer().WithHandle(0)
                                 .WithNonNullPayload()
                                 .WithState().Transactional().WithTxnId(txnId).And()
                                 .Respond()
                                 .WithState().Transactional().WithTxnId(txnId).WithAccepted().And()
                                 .WithSettled(true);
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
            ISender sender = session.OpenSender("address").OpenTask.Result;

            session.BeginTransaction();

            ITracker tracker = sender.Send(IMessage<string>.Create("test-message"));

            Assert.IsNotNull(tracker);
            Assert.IsNotNull(tracker.SettlementTask.Result);
            Assert.AreEqual(tracker.RemoteState.Type, DeliveryStateType.Transactional,
                         "Delivery inside transaction should have Transactional state");
            Assert.IsNotNull(tracker.State);
            Assert.AreEqual(tracker.State.Type, DeliveryStateType.Transactional,
                         "Delivery inside transaction should have Transactional state: " + tracker.State.Type);

            Wait.AssertTrue("Delivery in transaction should be locally settled after response", () => tracker.Settled);

            session.CommitTransaction();

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessagesInsideOfUniqueTransactions()
      {
         byte[] txnId1 = new byte[] { 0, 1, 2, 3 };
         byte[] txnId2 = new byte[] { 1, 1, 2, 3 };
         byte[] txnId3 = new byte[] { 2, 1, 2, 3 };
         byte[] txnId4 = new byte[] { 3, 1, 2, 3 };

         byte[][] transactions = new byte[][] { txnId1, txnId2, txnId3, txnId4 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit((uint)transactions.Length).Queue();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit((uint)(transactions.Length * 2)).Queue();
            for (int i = 0; i < transactions.Length; ++i)
            {
               peer.ExpectDeclare().Accept(transactions[i]);
               peer.ExpectTransfer().WithHandle(0)
                                    .WithNonNullPayload()
                                    .WithState().Transactional().WithTxnId(transactions[i]).And()
                                    .Respond()
                                    .WithState().Transactional().WithTxnId(transactions[i]).WithAccepted().And()
                                    .WithSettled(true);
               peer.ExpectDischarge().WithFail(false).WithTxnId(transactions[i]).Accept();
            }
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("address").OpenTask.Result;

            for (int i = 0; i < transactions.Length; ++i)
            {
               session.BeginTransaction();

               ITracker tracker = sender.Send(IMessage<string>.Create("test-message-" + i));

               Assert.IsNotNull(tracker);
               Assert.IsNotNull(tracker.SettlementTask.Result);
               Assert.AreEqual(tracker.RemoteState.Type, DeliveryStateType.Transactional);
               Assert.IsNotNull(tracker.State);
               Assert.AreEqual(tracker.State.Type, DeliveryStateType.Transactional,
                   "Delivery inside transaction should have Transactional state: " + tracker.State.Type);
               Wait.AssertTrue("Delivery in transaction should be locally settled after response", () => tracker.Settled);

               session.CommitTransaction();
            }

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveMessageInsideOfTransaction()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.ExpectDisposition().WithSettled(true)
                                    .WithState().Transactional().WithTxnId(txnId).WithAccepted();
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            session.BeginTransaction();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(10));
            Assert.IsNotNull(delivery);
            IMessage<object> received = delivery.Message();
            Assert.IsNotNull(received);
            Assert.IsTrue(received.Body is string);
            string value = (string)received.Body;
            Assert.AreEqual("Hello World", value);

            session.CommitTransaction();
            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveMessageInsideOfTransactionNoAutoSettleSenderSettles()
      {
         DoTestReceiveMessageInsideOfTransactionNoAutoSettle(true);
      }

      [Test]
      public void TestReceiveMessageInsideOfTransactionNoAutoSettleSenderDoesNotSettle()
      {
         DoTestReceiveMessageInsideOfTransactionNoAutoSettle(false);
      }

      private void DoTestReceiveMessageInsideOfTransactionNoAutoSettle(bool settle)
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.Start();

            byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ReceiverOptions options = new ReceiverOptions()
            {
               AutoAccept = false,
               AutoSettle = false
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.ExpectDisposition().WithSettled(true)
                                    .WithState().Transactional().WithTxnId(txnId).WithAccepted();
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            session.BeginTransaction();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(10));
            Assert.IsNotNull(delivery);
            Assert.IsFalse(delivery.Settled);
            Assert.IsNull(delivery.State);

            IMessage<object> received = delivery.Message();
            Assert.IsNotNull(received);
            Assert.IsTrue(received.Body is string);
            string value = (string)received.Body;
            Assert.AreEqual("Hello World", value);

            // Manual Accept within the transaction, settlement is ignored.
            delivery.Disposition(ClientAccepted.Instance, settle);

            session.CommitTransaction();
            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveMessageInsideOfTransactionButAcceptAndSettleOutside()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.Start();

            byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ReceiverOptions options = new ReceiverOptions()
            {
               AutoAccept = false,
               AutoSettle = false
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();
            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();

            session.BeginTransaction();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(10));
            Assert.IsNotNull(delivery);
            Assert.IsFalse(delivery.Settled);
            Assert.IsNull(delivery.State);

            IMessage<object> received = delivery.Message();
            Assert.IsNotNull(received);
            Assert.IsTrue(received.Body is string);
            String value = (String)received.Body;
            Assert.AreEqual("Hello World", value);

            session.CommitTransaction();

            // Manual Accept outside the transaction and no auto settle or accept
            // so no transactional enlistment.
            delivery.Disposition(ClientAccepted.Instance, true);

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestTransactionCommitFailWithEmptyRejectedDisposition()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.ExpectTransfer().WithHandle(0)
                                 .WithNonNullPayload()
                                 .WithState().Transactional().WithTxnId(txnId).And()
                                 .Respond()
                                 .WithState().Transactional().WithTxnId(txnId).WithAccepted().And()
                                 .WithSettled(true);
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Reject();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("address").OpenTask.Result;

            session.BeginTransaction();

            ITracker tracker = sender.Send(IMessage<string>.Create("test-message"));
            Assert.IsNotNull(tracker.SettlementTask.Result);
            Assert.AreEqual(tracker.RemoteState.Type, DeliveryStateType.Transactional);

            try
            {
               session.CommitTransaction();
               Assert.Fail("Commit should fail with Rollback exception");
            }
            catch (ClientTransactionRolledBackException)
            {
               // Expected roll back due to discharge rejection
            }

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDeclareTransactionAfterConnectionDrops()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.DropAfterLastHandler();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            peer.WaitForScriptToComplete();

            try
            {
               session.BeginTransaction();
               Assert.Fail("Should have failed to discharge transaction");
            }
            catch (ClientException cliEx)
            {
               // Expected error as connection was dropped
               logger.LogDebug("Client threw error on begin after connection drop", cliEx);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCommitTransactionAfterConnectionDropsFollowingTxnDeclared()
      {
         DischargeTransactionAfterConnectionDropsFollowingTxnDeclared(true);
      }

      [Test]
      public void TestRollbackTransactionAfterConnectionDropsFollowingTxnDeclared()
      {
         DischargeTransactionAfterConnectionDropsFollowingTxnDeclared(false);
      }

      public void DischargeTransactionAfterConnectionDropsFollowingTxnDeclared(bool commit)
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.DropAfterLastHandler();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();

            peer.WaitForScriptToComplete();

            if (commit)
            {
               try
               {
                  session.CommitTransaction();
                  Assert.Fail("Should have failed to commit transaction");
               }
               catch (ClientException)
               {
                  // Expected error as connection was dropped
               }
            }
            else
            {
               try
               {
                  session.RollbackTransaction();
               }
               catch (ClientConnectionRemotelyClosedException)
               {
                  // Can get an error if the session processes the close before the
                  // roll back is called.  Mitigating that is tricky and still leaves
                  // the user needing to handle error when session is actually closed
                  // via Session.Close()
               }
               catch (Exception ex)
               {
                  logger.LogInformation("Caught unexpected error: {}", ex);
                  Assert.Fail("Connection drops will implicitly roll back TXN on remote");
               }
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessagesNoOpWhenTransactionInDoubt()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.RemoteDetach().WithClosed(true)
                               .WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Coordinator").Queue();
            peer.ExpectDetach();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();

            // After the wait TXN should be in doubt and send should no-op
            peer.WaitForScriptToComplete();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            ISender sender = session.OpenSender("address").OpenTask.Result;

            for (int i = 0; i < 10; ++i)
            {
               ITracker tracker = sender.Send(IMessage<string>.Create("test-message-"));

               Assert.IsNotNull(tracker);
               Assert.IsNotNull(tracker.SettlementTask.Result);
               Assert.AreEqual(ClientAccepted.Instance, tracker.RemoteState);
               Assert.IsTrue(tracker.RemoteSettled);
               Assert.IsNull(tracker.State);
               Assert.IsFalse(tracker.Settled);
               Assert.IsFalse(tracker.AwaitAccepted().Settled);
               Assert.IsFalse(tracker.AwaitSettlement().Settled);
               Assert.IsFalse(tracker.AwaitAccepted(TimeSpan.FromSeconds(1)).Settled);
               Assert.IsFalse(tracker.AwaitSettlement(TimeSpan.FromSeconds(1)).Settled);
               Assert.AreSame(sender, tracker.Sender);

               // These should no-op since message was never sent.
               tracker.Settle();
               tracker.Disposition(ClientAccepted.Instance, true);
            }

            try
            {
               session.CommitTransaction();
               Assert.Fail("Should not be able to commit as remote closed coordinator");
            }
            catch (ClientTransactionRolledBackException)
            {
               // Expected
            }

            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Stream sender not completed yet")]
      [Test]
      public void TestStreamSenderMessageCanOperatesWithinTransaction()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            message.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = message.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher dataMatcher = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.MessageContentMatcher = dataMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(5).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.ExpectTransfer().WithHandle(0)
                                 .WithMore(true)
                                 .WithPayload(payloadMatcher)
                                 .WithState().Transactional().WithTxnId(txnId).And()
                                 .Respond()
                                 .WithState().Transactional().WithTxnId(txnId).WithAccepted().And()
                                 .WithSettled(true);
            peer.ExpectTransfer().WithMore(false).WithNullPayload();
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            sender.Session.BeginTransaction();

            // Stream won't output until some body bytes are written since the buffer was not
            // filled by the header write.  Then the close will complete the stream message.
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Close();

            sender.Session.CommitTransaction();
            sender.CloseAsync().Wait();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAcceptAndRejectInSameTransaction()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.Start();

            byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ReceiverOptions options = new ReceiverOptions()
            {
               AutoAccept = false,
               AutoSettle = false
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(1)
                                 .WithDeliveryTag(new byte[] { 2 })
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.ExpectDisposition().WithSettled(true)
                                    .WithState().Transactional().WithTxnId(txnId).WithAccepted();
            peer.ExpectDisposition().WithSettled(true)
                                    .WithState().Transactional().WithTxnId(txnId).WithReleased();
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            session.BeginTransaction();

            IDelivery delivery1 = receiver.Receive(TimeSpan.FromSeconds(1));
            IDelivery delivery2 = receiver.Receive(TimeSpan.FromSeconds(1));

            Assert.IsNotNull(delivery1);
            Assert.IsFalse(delivery1.Settled);
            Assert.IsNull(delivery1.State);
            Assert.IsNotNull(delivery2);
            Assert.IsFalse(delivery2.Settled);
            Assert.IsNull(delivery2.State);

            delivery1.Accept();
            delivery2.Release();

            session.CommitTransaction();
            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}