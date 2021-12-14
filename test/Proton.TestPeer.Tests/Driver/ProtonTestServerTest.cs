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

using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture, Timeout(20000)]
   public class ProtonTestserverTest : ProtonBaseTestFixture
   {
      [Test]
      public void TestServerStart()
      {
         ProtonTestServer peer = new ProtonTestServer();

         Assert.IsFalse(peer.IsClosed);
         peer.Start();
         peer.Close();
         Assert.IsTrue(peer.IsClosed);
      }

      [Test]
      public void TestServerStartThenStopAndWaitForCompletion()
      {
         ProtonTestServer peer = new ProtonTestServer();

         Assert.IsFalse(peer.IsClosed);
         peer.Start();
         peer.Close();
         Assert.IsTrue(peer.IsClosed);
         peer.WaitForScriptToComplete();
      }

      [Test]
      public void TestServerFailsTestIfFrameSizeExpectationNotMet()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().WithFrameSize(4096);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient(loggerFactory);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();

            Assert.Throws<AssertionError>(() => peer.WaitForScriptToComplete());

            client.Close();
         }
      }
   }
}