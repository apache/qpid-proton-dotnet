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

using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture]
   public class ProtonTestConnectorTests
   {
      private ILoggerFactory loggerFactory;

      [OneTimeSetUp]
      public void OneTimeSetup()
      {
         loggerFactory = LoggerFactory.Create(builder =>
            builder.ClearProviders()
                   .SetMinimumLevel(LogLevel.Trace)
                   .AddSimpleConsole(options =>
                   {
                      options.IncludeScopes = true;
                      options.SingleLine = true;
                      options.TimestampFormat = "hh:mm:ss ";
                   })
         );
      }

      [Test]
      public void TestCreateConnectorAndIngestFailsWhenNoExpectationsSet()
      {
         Stream frame = null; // Unused in this context as connector won't produce output

         ProtonTestConnector connector =
            new ProtonTestConnector((outputFrame) => frame = outputFrame, loggerFactory);

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

      [Test]
      public void TestServerProcessHeaderAnComplexOpenFrame()
      {
         // Frame data for: Open
         //   Open{ containerId='container', hostname='localhost', maxFrameSize=16384, channelMax=65535,
         //         idleTimeOut=36000, outgoingLocales=null, incomingLocales=null, offeredCapabilities=[SOMETHING],
         //         desiredCapabilities=[ANONYMOUS-RELAY, DELAYED-DELIVERY], properties={queue-prefix=queue://}}
         byte[] completeOpen = new byte[] {0, 0, 0, 129, 2, 0, 0, 0, 0, 83, 16, 192, 116, 10, 161, 9, 99, 111,
                                           110, 116, 97, 105, 110, 101, 114, 161, 9, 108, 111, 99, 97, 108, 104,
                                           111, 115, 116, 112, 0, 0, 64, 0, 96, 255, 255, 112, 0, 0, 140, 160,
                                           64, 64, 224, 12, 1, 163, 9, 83, 79, 77, 69, 84, 72, 73, 78, 71, 224,
                                           35, 2, 163, 15, 65, 78, 79, 78, 89, 77, 79, 85, 83, 45, 82, 69, 76,
                                           65, 89, 16, 68, 69, 76, 65, 89, 69, 68, 45, 68, 69, 76, 73, 86, 69,
                                           82, 89, 193, 25, 2, 163, 12, 113, 117, 101, 117, 101, 45, 112, 114,
                                           101, 102, 105, 120, 161, 8, 113, 117, 101, 117, 101, 58, 47, 47};

         ProtonTestConnector server = new ProtonTestConnector();
         ProtonTestConnector client = new ProtonTestConnector(server.Ingest);
         server.ConnectorFrameSink(client.Ingest);

         // Expectation should convert this to symbol keyed map expectation along with the
         // offered and desired capabilities values passed below which should be converted
         // to symbol array expectations.
         IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
         expectedProperties.Add("queue-prefix", "queue://");

         server.ExpectAMQPHeader().RespondWithAMQPHeader();
         server.ExpectOpen().WithContainerId("container")
                            .WithHostname("localhost")
                            .WithMaxFrameSize(16384u)
                            .WithChannelMax(65535)
                            .WithIdleTimeOut(36000u)
                            .WithOfferedCapabilities("SOMETHING")
                            .WithDesiredCapabilities("ANONYMOUS-RELAY", "DELAYED-DELIVERY");
         // TODO Matchers must be smarter
         //  .WithProperties(expectedProperties);
         client.ExpectAMQPHeader();
         client.RemoteHeader(AMQPHeader.Header).Now();
         client.RemoteBytes().WithBytes(completeOpen).Now();

         server.WaitForScriptToComplete();
         client.WaitForScriptToComplete();
      }
   }
}