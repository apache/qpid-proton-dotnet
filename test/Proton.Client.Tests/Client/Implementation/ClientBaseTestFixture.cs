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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec;
using System;
using System.Collections.Generic;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types;

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
         // config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logconsole);
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

      protected static byte[] CreateEncodedMessage(ISection body)
      {
         IEncoder encoder = CodecFactory.Encoder;
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         encoder.WriteObject(buffer, encoder.NewEncoderState(), body);
         byte[] result = new byte[buffer.ReadableBytes];
         buffer.CopyInto(0, result, 0, result.Length);
         return result;
      }

      protected static byte[] CreateEncodedMessage(params Data[] body)
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

      protected static byte[] CreateEncodedMessage(params ISection[] body)
      {
         IEncoder encoder = CodecFactory.Encoder;
         IProtonBuffer buffer = new ProtonByteBufferAllocator().Allocate();
         foreach (ISection section in body)
         {
            encoder.WriteObject(buffer, encoder.NewEncoderState(), section);
         }
         byte[] result = new byte[buffer.ReadableBytes];
         buffer.CopyInto(0, result, 0, result.Length);
         return result;
      }

      protected static byte[] CreateInvalidHeaderEncoding()
      {
         byte[] buffer = new byte[12];

         buffer[0] = 0; // Described Type Indicator
         buffer[1] = (byte)EncodingCodes.SmallULong;
         buffer[2] = (byte)Header.DescriptorCode;
         buffer[3] = (byte)EncodingCodes.Map32; // Should be list based

         return buffer;
      }

      protected static byte[] CreateInvalidDeliveryAnnotationsEncoding()
      {
         byte[] buffer = new byte[12];

         buffer[0] = 0; // Described Type Indicator
         buffer[1] = (byte)EncodingCodes.SmallULong;
         buffer[2] = (byte)DeliveryAnnotations.DescriptorCode;
         buffer[3] = (byte)EncodingCodes.List32; // Should be Map based

         return buffer;
      }

      protected static byte[] CreateInvalidMessageAnnotationsEncoding()
      {
         byte[] buffer = new byte[12];

         buffer[0] = 0; // Described Type Indicator
         buffer[1] = (byte)EncodingCodes.SmallULong;
         buffer[2] = (byte)MessageAnnotations.DescriptorCode;
         buffer[3] = (byte)EncodingCodes.List32; // Should be Map based

         return buffer;
      }

      protected static byte[] CreateInvalidPropertiesEncoding()
      {
         byte[] buffer = new byte[12];

         buffer[0] = 0; // Described Type Indicator
         buffer[1] = (byte)EncodingCodes.SmallULong;
         buffer[2] = (byte)Properties.DescriptorCode;
         buffer[3] = (byte)EncodingCodes.Map32; // Should be list based

         return buffer;
      }

      protected static byte[] CreateInvalidApplicationPropertiesEncoding()
      {
         byte[] buffer = new byte[12];

         buffer[0] = 0; // Described Type Indicator
         buffer[1] = (byte)EncodingCodes.SmallULong;
         buffer[2] = (byte)ApplicationProperties.DescriptorCode;
         buffer[3] = (byte)EncodingCodes.List32; // Should be map based

         return buffer;
      }

      protected byte[] CreateNestedEncodedMessage(int depth)
      {
         IEncoder encoder = CodecFactory.Encoder;
         IProtonBuffer buffer = new ProtonByteBufferAllocator().Allocate();
         encoder.WriteObject(buffer, encoder.NewEncoderState(), new AmqpValue(CreateNode(depth, 0)));
         byte[] result = new byte[buffer.ReadableBytes];
         buffer.CopyInto(buffer.ReadOffset, result, 0, result.Length);
         return result;
      }

      private UnknownDescribedType CreateNode(int limit, int depth)
      {
         ulong DESCRIPTOR_CODE = 0xAA00468C00000003UL;

         if (++depth > limit)
         {
            return new UnknownDescribedType(DESCRIPTOR_CODE, null);
         }
         else
         {
            List<UnknownDescribedType> list = new List<UnknownDescribedType>();
            list.Add(CreateNode(limit, depth));

            return new UnknownDescribedType(DESCRIPTOR_CODE, list);
         }
      }

      protected byte[] CreateEncodedMessageWithZeroWidthArray(int length)
      {
         if (length > byte.MaxValue)
         {
            throw new ArgumentException("Length must be within the range of an unsigned byte");
         }

         IProtonBuffer buffer = new ProtonByteBufferAllocator().Allocate();

         buffer.WriteUnsignedByte(0); // Described Type Indicator - 1
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
         buffer.WriteUnsignedByte(2);
         buffer.WriteUnsignedByte((byte)length);
         buffer.WriteUnsignedByte((byte)EncodingCodes.BooleanTrue);

         byte[] result = new byte[buffer.ReadableBytes];

         buffer.CopyInto(buffer.ReadOffset, result, 0, result.Length);

         return result;
      }
   }
}