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
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture, Timeout(20000)]
   public abstract class ProtonBaseTestFixture
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
         config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logconsole);
         config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);

         loggerFactory = NullLoggerFactory.Instance;
         // loggerFactory = LoggerFactory.Create(builder =>
         //    builder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNLog(config)
         // );

         logger = NullLogger.Instance;
            //loggerFactory.CreateLogger(GetType().Name);

         AppDomain currentDomain = AppDomain.CurrentDomain;
         currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UncaughtExceptionHander);
      }

      public static void UncaughtExceptionHander(object sender, UnhandledExceptionEventArgs args)
      {
         Console.WriteLine(string.Format("Unit test run threw unhandled exception: {}", args.ExceptionObject.ToString()));
      }

      [SetUp]
      public void SetUp()
      {
         // Just in case the one time setup routine is disabled create a null logger to prevent
         // NPEs from code in the tests.
         if (logger == null)
         {
            logger = NullLogger.Instance;
         }

         testName = TestContext.CurrentContext.Test.Name;
         logger?.LogInformation("--------- Begin test {0} ---------------------------------", testName);
      }

      [TearDown]
      public void TearDown()
      {
         logger?.LogInformation("--------- End test {0} ---------------------------------", testName);
      }
   }
}