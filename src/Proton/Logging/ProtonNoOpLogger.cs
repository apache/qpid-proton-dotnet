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

using System;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Logging
{
   /// <summary>
   /// Proton No-Op MS logger implementation that sink holes all logging and
   /// reports no levels enabled.
   /// </summary>
   internal class ProtonNoOpLogger : ILogger
   {
      private readonly string name;

      public string Name => name;

      internal ProtonNoOpLogger(string name)
      {
         this.name = name;
      }

      public IDisposable BeginScope<TState>(TState state)
      {
         return NoOpDisposable.Instance;
      }

      public bool IsEnabled(LogLevel logLevel)
      {
         return false;
      }

      public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
      {
      }

      private class NoOpDisposable : IDisposable
      {
         public static NoOpDisposable Instance = new();

         public void Dispose()
         {
         }
      }
   }
}