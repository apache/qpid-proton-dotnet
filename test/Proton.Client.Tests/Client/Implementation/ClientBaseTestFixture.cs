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

using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NLog.Extensions.Logging;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public abstract class ClientBaseTestFixture
   {
      protected ILoggerFactory loggerFactory;
      protected ILogger logger;
      protected string testName;

      [OneTimeSetUp]
      public void OneTimeSetup()
      {
         var config = new NLog.Config.LoggingConfiguration();

         // Targets where to log to: File and Console
         NLog.Targets.FileTarget logfile = new NLog.Targets.FileTarget("logfile")
         {
            FileName = "./target/" + GetType().Name + ".txt",
            DeleteOldFileOnStartup = true
         };
         NLog.Targets.Target logconsole = new NLog.Targets.ConsoleTarget("logconsole");

         // Rules for mapping loggers to targets
         config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logconsole);
         config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);

         loggerFactory = LoggerFactory.Create(builder =>
            builder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNLog(config)
         );

         logger = loggerFactory.CreateLogger(GetType().Name);

         // Configure the proton logger facility such that it uses the configured logger factory.
         ProtonLoggerFactory.Factory = loggerFactory;
      }

      [SetUp]
      public void SetUp()
      {
         testName = TestContext.CurrentContext.Test.Name;
         logger.LogInformation("--------- Begin test {0} ---------------------------------", testName);
      }

      [TearDown]
      public void TearDown()
      {
         logger.LogInformation("--------- End test {0} ---------------------------------", testName);
      }

      protected byte[] CreateEncodedMessage(ISection body)
      {
         IEncoder encoder = CodecFactory.Encoder;
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         encoder.WriteObject(buffer, encoder.NewEncoderState(), body);
         byte[] result = new byte[buffer.ReadableBytes];
         buffer.CopyInto(0, result, 0, result.Length);
         return result;
      }

      protected byte[] CreateEncodedMessage(params Data[] body)
      {
         IEncoder encoder = CodecFactory.Encoder;
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         foreach (Data data in body)
         {
            encoder.WriteObject(buffer, encoder.NewEncoderState(), data);
         }
         byte[] result = new byte[buffer.ReadableBytes];
         buffer.CopyInto(0, result, 0, result.Length);
         return result;
      }
   }
}