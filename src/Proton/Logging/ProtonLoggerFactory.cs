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

using Apache.Qpid.Proton.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Apache.Qpid.Proton.Logging
{
   /// <summary>
   /// Proton Logging API factory which provides an single point of entry for
   /// users to configure proton wide logging sources. The user is encouraged
   /// to configure the logging abstraction that proton objects should use as
   /// early as possible as any updates to the logging abstraction will not
   /// be reflected in code that has already created its logger.
   /// </summary>
   public static class ProtonLoggerFactory
   {
      private static ILoggerFactory loggerFactory;

      /// <summary>
      /// Returns a logger created using the information of the generic type
      /// to generate a logger name.
      /// </summary>
      /// <typeparam name="T">The type requesting a logger</typeparam>
      /// <returns>A new proton logger instance</returns>/
      public static IProtonLogger GetLogger<T>() => GetLogger(typeof(T));

      /// <summary>
      /// Returns a logger created using the information of the type provided
      /// to generate a logger name.
      /// </summary>
      /// <param name="theType">The type for which a logger is being requested</param>
      /// <returns>A new proton logger instance</returns>/
      public static IProtonLogger GetLogger(Type theType) => GetLogger(theType.FullName);

      /// <summary>
      /// Returns a logger created using the name provided as the logger name.
      /// </summary>
      /// <param name="loggerName">The name of the logger is being requested</param>
      /// <returns>A new proton logger instance</returns>/
      public static IProtonLogger GetLogger(string loggerName)
      {
         return new ProtonLoggerWrapper(loggerName, Factory.CreateLogger(loggerName));
      }

      /// <summary>
      /// Gets or sets the logger factory that proton has been configured to use
      /// and if none was configured it creates a default proton factory that will
      /// produce logs in some manner.
      /// </summary>
      public static ILoggerFactory Factory
      {
         get
         {
            // TODO: Loggers already created will be using the old logger factory
            //       loggers which could cause issues in tests.
            ILoggerFactory current = Volatile.Read(ref loggerFactory);
            if (current == null)
            {
               // Create a new proton factory but allow race to set something else.
               current = CreateProtonDefaultFactory(typeof(ProtonLoggerFactory).FullName);
               _ = Interlocked.CompareExchange(ref loggerFactory, current, null);
            }

            return loggerFactory;
         }

         set
         {
            Statics.RequireNonNull(value, "Cannot configure the logging factory to be null");
            Volatile.Write(ref loggerFactory, value);
         }
      }

      private static ILoggerFactory CreateProtonDefaultFactory(string name)
      {
         ILoggerFactory factory = new LoggerFactory();
         factory.AddProvider(new ProtonDefaultLoggerProvider());

         ILogger logger = factory.CreateLogger(name);

         // Consider providing more information in the initial logs.
         logger.LogDebug("Using the internal Proton default logging API");

         return factory;
      }
   }
}