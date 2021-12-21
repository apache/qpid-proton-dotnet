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
using System;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Types.Transport;
using System.Collections.Generic;
using System.Linq;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientSessionTest : ClientBaseTestFixture
   {
      [Test]
      public void TestSessionOpenTimeoutWhenNoRemoteBeginArrivesTimeout()
      {
         DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(true);
      }

      [Test]
      public void TestSessionOpenTimeoutWhenNoRemoteBeginArrivesNoTimeout()
      {
         DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(false);
      }

      private void DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 75;
            ISession session = connection.OpenSession(options);

            try
            {
               if (timeout)
               {
                  session.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.OpenTask.Wait();
               }

               Assert.Fail("Session Open should timeout when no Begin response and complete future with error.");
            }
            catch (Exception error)
            {
               logger.LogInformation("Session open failed with error: ", error.Message);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionOpenWaitWithTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionOpenWaitCanceledWhenConnectionDrops(true);
      }

      [Test]
      public void TestSessionOpenWaitWithNoTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionOpenWaitCanceledWhenConnectionDrops(false);
      }

      private void DoTestSessionOpenWaitCanceledWhenConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();

            try
            {
               if (timeout)
               {
                  session.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.OpenTask.Wait();
               }

               Assert.Fail("Session Open should wait should abort when connection drops.");
            }
            catch (Exception error)
            {
               logger.LogInformation("Session open failed with error: ", error.Message);
               Assert.IsTrue(error.InnerException is ClientIOException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionCloseTimeoutWhenNoRemoteEndArrivesTimeout()
      {
         DoTestSessionCloseTimeoutWhenNoRemoteEndArrives(true);
      }

      [Test]
      public void TestSessionCloseTimeoutWhenNoRemoteEndArrivesNoTimeout()
      {
         DoTestSessionCloseTimeoutWhenNoRemoteEndArrives(false);
      }

      private void DoTestSessionCloseTimeoutWhenNoRemoteEndArrives(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.CloseTimeout = 75;
            ISession session = connection.OpenSession(options).OpenTask.GetAwaiter().GetResult();

            try
            {
               if (timeout)
               {
                  session.CloseAsync().Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.CloseAsync().Wait();
               }

               Assert.Fail("Close should throw an error if the Session end doesn't arrive in time");
            }
            catch (Exception error)
            {
               logger.LogInformation("Session close failed with error: ", error);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionCloseWaitWithTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionCloseWaitCanceledWhenConnectionDrops(true);
      }

      [Test]
      public void TestSessionCloseWaitWithNoTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionCloseWaitCanceledWhenConnectionDrops(false);
      }

      private void DoTestSessionCloseWaitCanceledWhenConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.CloseTimeout = 75;
            ISession session = connection.OpenSession(options).OpenTask.GetAwaiter().GetResult();

            try
            {
               if (timeout)
               {
                  session.CloseAsync().Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.CloseAsync().Wait();
               }
            }
            catch (Exception)
            {
               Assert.Fail("Session Close should complete when parent connection drops.");
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionCloseGetsResponseWithErrorDoesNotThrowTimedGet()
      {
         DoTestSessionCloseGetsResponseWithErrorThrows(true);
      }

      [Test]
      public void TestConnectionCloseGetsResponseWithErrorDoesNotThrowUntimedGet()
      {
         DoTestSessionCloseGetsResponseWithErrorThrows(false);
      }

      protected void DoTestSessionCloseGetsResponseWithErrorThrows(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd().Respond().WithErrorCondition(AmqpError.INTERNAL_ERROR.ToString(), "Something odd happened.");
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.GetAwaiter().GetResult();

            if (timeout)
            {
               session.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            }
            else
            {
               session.CloseAsync().Wait();
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionGetRemotePropertiesWaitsForRemoteBegin()
      {
         TryReadSessionRemoteProperties(true);
      }

      [Test]
      public void TestSessionGetRemotePropertiesFailsAfterOpenTimeout()
      {
         TryReadSessionRemoteProperties(false);
      }

      private void TryReadSessionRemoteProperties(bool beginResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 120;
            ISession session = connection.OpenSession(options);

            peer.WaitForScriptToComplete();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            expectedProperties.Add("TEST", "test-property");

            if (beginResponse)
            {
               peer.ExpectEnd().Respond();
               peer.RespondToLastBegin().WithProperties(expectedProperties).Later(10);
            }
            else
            {
               peer.ExpectEnd();
            }

            if (beginResponse)
            {
               Assert.IsNotNull(session.Properties, "Remote should have responded with a remote properties value");
               Assert.AreEqual(expectedProperties, session.Properties);
            }
            else
            {
               try
               {
                  _ = session.Properties;
                  Assert.Fail("Should failed to get remote state due to no begin response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex.Message);
               }
            }

            try
            {
               session.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and end was sent");
            }

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionGetRemoteOfferedCapabilitiesWaitsForRemoteBegin()
      {
         TryReadSessionRemoteOfferedCapabilities(true);
      }

      [Test]
      public void TestSessionGetRemoteOfferedCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadSessionRemoteOfferedCapabilities(false);
      }

      private void TryReadSessionRemoteOfferedCapabilities(bool beginResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            _ = connection.OpenTask.Result;
            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 125;
            ISession session = connection.OpenSession(options);

            peer.WaitForScriptToComplete();

            if (beginResponse)
            {
               peer.ExpectEnd().Respond();
               peer.RespondToLastBegin().WithOfferedCapabilities("transactions").Later(10);
            }
            else
            {
               peer.ExpectEnd();
            }

            if (beginResponse)
            {
               Assert.IsNotNull(session.OfferedCapabilities, "Remote should have responded with a remote offered Capabilities value");
               Assert.AreEqual(1, session.OfferedCapabilities.Count);
               Assert.AreEqual("transactions", session.OfferedCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = session.OfferedCapabilities;
                  Assert.Fail("Should failed to get remote state due to no begin response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               session.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and end was sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionGetRemoteDesiredCapabilitiesWaitsForRemoteBegin()
      {
         tryReadSessionRemoteDesiredCapabilities(true);
      }

      [Test]
      public void TestSessionGetRemoteDesiredCapabilitiesFailsAfterOpenTimeout()
      {
         tryReadSessionRemoteDesiredCapabilities(false);
      }

      private void tryReadSessionRemoteDesiredCapabilities(bool beginResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 125;
            ISession session = connection.OpenSession(options);

            peer.WaitForScriptToComplete();

            if (beginResponse)
            {
               peer.ExpectEnd().Respond();
               peer.RespondToLastBegin().WithDesiredCapabilities("Error-Free").Later(10);
            }
            else
            {
               peer.ExpectEnd();
            }

            if (beginResponse)
            {
               Assert.IsNotNull(session.DesiredCapabilities, "Remote should have responded with a remote desired Capabilities value");
               Assert.AreEqual(1, session.DesiredCapabilities.Count);
               Assert.AreEqual("Error-Free", session.DesiredCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = session.DesiredCapabilities;
                  Assert.Fail("Should failed to get remote state due to no begin response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               session.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and end sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestQuickOpenCloseWhenNoBeginResponseFailsFastOnOpenTimeout()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ConnectionOptions options = new ConnectionOptions();
            options.CloseTimeout = (long)TimeSpan.FromHours(1).TotalMilliseconds;

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            connection.OpenTask.Wait();

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.OpenTimeout = 125;

            try
            {
               connection.OpenSession(sessionOptions).CloseAsync().Wait();
            }
            catch (Exception)
            {
               Assert.Fail("Should not fail when waiting on close with quick open timeout");
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseWithErrorConditionSync()
      {
         DoTestCloseWithErrorCondition(true);
      }

      [Test]
      public void TestCloseWithErrorConditionAsync()
      {
         DoTestCloseWithErrorCondition(false);
      }

      private void DoTestCloseWithErrorCondition(bool sync)
      {
         string condition = "amqp:precondition-failed";
         string description = "something bad happened.";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd().WithError(condition, description).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();

            session.OpenTask.Wait();

            Assert.AreEqual(session.Client, container);

            if (sync)
            {
               session.Close(IErrorCondition.Create(condition, description, null));
            }
            else
            {
               session.CloseAsync(IErrorCondition.Create(condition, description, null));
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotCreateResourcesFromClosedSession()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();

            session.OpenTask.Result.Close();

            Assert.Throws<ClientIllegalStateException>(() => session.OpenReceiver("test"));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenReceiver("test", new ReceiverOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDurableReceiver("test", "test"));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDurableReceiver("test", "test", new ReceiverOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDynamicReceiver());
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDynamicReceiver(new ReceiverOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDynamicReceiver(new ReceiverOptions(), new Dictionary<string, object>()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenSender("test"));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenSender("test", new SenderOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenAnonymousSender());
            Assert.Throws<ClientIllegalStateException>(() => session.OpenAnonymousSender(new SenderOptions()));

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}