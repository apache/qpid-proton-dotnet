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

using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture]
   public class ProtonTestConnectorTests
   {
      [Test]
      public void TestCreateConnectorAndIngestFailsWhenNoExpectationsSet()
      {
         Stream frame = null; // Unused in this context as connector won't produce output

         ProtonTestConnector connector = new ProtonTestConnector((outputFrame) => frame = outputFrame);

         Assert.IsNotNull(connector.Driver);

         connector.Ingest(new MemoryStream(AMQPHeader.Header.ToArray()));

         Assert.IsNull(frame);
         Assert.Throws<AssertionError>(() => connector.WaitForScriptToComplete());
      }

      [Test]
      public void TestCreateConnectorAndIngestHeaderAsScripted()
      {
         Stream frame = null;

         ProtonTestConnector connector = new ProtonTestConnector((outputFrame) => frame = outputFrame);

         connector.ExpectAMQPHeader();

         connector.Ingest(new MemoryStream(AMQPHeader.Header.ToArray()));

         Assert.IsNull(frame);
         Assert.DoesNotThrow(() => connector.WaitForScriptToComplete());
      }

      [Test]
      public void TestConnectTwoTestConnectorsAndExchangeHeaders()
      {
         ProtonTestConnector server = new ProtonTestConnector();
         ProtonTestConnector client = new ProtonTestConnector(server.Ingest);
         server.ConnectorFrameSink(client.Ingest);

         server.ExpectAMQPHeader().RespondWithAMQPHeader();
         client.ExpectAMQPHeader();
         client.RemoteHeader(AMQPHeader.Header).Now();

         server.WaitForScriptToComplete();
         client.WaitForScriptToComplete();
      }

      [Test]
      public void TestServerProcessesHeaderAndEmptyOpenFrame()
      {
         // Frame data for: Open
         //   Open{ containerId="", hostname='null', maxFrameSize=4294967295, channelMax=65535,
         //         idleTimeOut=null, outgoingLocales=null, incomingLocales=null, offeredCapabilities=null,
         //         desiredCapabilities=null, properties=null}
         byte[] emptyOpen = new byte[] { 0, 0, 0, 16, 2, 0, 0, 0, 0, 83, 16, 192, 3, 1, 161, 0 };

         ProtonTestConnector server = new ProtonTestConnector();
         ProtonTestConnector client = new ProtonTestConnector(server.Ingest);
         server.ConnectorFrameSink(client.Ingest);

         server.ExpectAMQPHeader().RespondWithAMQPHeader();
         server.ExpectOpen().WithContainerId("");
         client.ExpectAMQPHeader();
         client.RemoteHeader(AMQPHeader.Header).Now();
         client.RemoteBytes().WithBytes(emptyOpen).Now();

         server.WaitForScriptToComplete();
         client.WaitForScriptToComplete();
      }

      [Test]
      public void TestServerProcessHeaderAndSimpleOpenFrame()
      {
        // Frame data for: Open
        //   Open{ containerId='container', hostname='localhost', maxFrameSize=16384, channelMax=65535,
        //         idleTimeOut=30000, outgoingLocales=null, incomingLocales=null, offeredCapabilities=null,
        //         desiredCapabilities=null, properties=null}
        byte[] basicOpen = new byte[] {0, 0, 0, 49, 2, 0, 0, 0, 0, 83, 16, 192, 36, 5, 161, 9, 99, 111,
                                       110, 116, 97, 105, 110, 101, 114, 161, 9, 108, 111, 99, 97, 108,
                                       104, 111, 115, 116, 112, 0, 0, 64, 0, 96, 255, 255, 112, 0, 0, 117, 48};

         ProtonTestConnector server = new ProtonTestConnector();
         ProtonTestConnector client = new ProtonTestConnector(server.Ingest);
         server.ConnectorFrameSink(client.Ingest);

         server.ExpectAMQPHeader().RespondWithAMQPHeader();
         server.ExpectOpen().WithContainerId("container")
                            .WithHostname("localhost")
                            .WithMaxFrameSize(16384u)
                            .WithChannelMax(65535)
                            .WithIdleTimeOut(30000u);
         client.ExpectAMQPHeader();
         client.RemoteHeader(AMQPHeader.Header).Now();
         client.RemoteBytes().WithBytes(basicOpen).Now();

         server.WaitForScriptToComplete();
         client.WaitForScriptToComplete();
      }
   }
}