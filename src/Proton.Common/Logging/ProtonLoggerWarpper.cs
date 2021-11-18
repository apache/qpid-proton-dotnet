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

namespace Apache.Qpid.Proton.Common.Logging
{
   /// <summary>
   /// Proton external .NET logging extensions wrapper used to provide
   /// a common API for proton code to log.
   /// </summary>
   internal class ProtonLoggerWrapper : IProtonLogger
   {
      private readonly string name;
      private readonly ILogger wrapped;

      internal ProtonLoggerWrapper(string name, ILogger wrapped)
      {
         this.name = name;
         this.wrapped = wrapped;
      }

      public string LoggerName => name;

      public bool IsTraceEnabled => wrapped.IsEnabled(LogLevel.Trace);

      public bool IsDebugEnabled => wrapped.IsEnabled(LogLevel.Debug);

      public bool IsInfoEnabled => wrapped.IsEnabled(LogLevel.Information);

      public bool IsWarnEnabled => wrapped.IsEnabled(LogLevel.Warning);

      public bool IsErrorEnabled => wrapped.IsEnabled(LogLevel.Error);

      // At the moment these are logging using the extensions and not doing
      // any custom formatting especially in the case of exception which may
      // not be ideal. This should be revisited when time permits.

      public void Debug(string message)
      {
         wrapped.LogDebug(message);
      }

      public void Debug(string message, object value)
      {
         wrapped.LogDebug(message, value);
      }

      public void Debug(string message, object value1, object value2)
      {
         wrapped.LogDebug(message, value1, value2);
      }

      public void Debug(string message, params object[] values)
      {
         wrapped.LogDebug(message, values);
      }

      public void Debug(string message, Exception exception)
      {
         wrapped.Log(LogLevel.Debug, 0, exception, message, null);
      }

      public void Error(string message)
      {
         wrapped.LogError(message);
      }

      public void Error(string message, object value)
      {
         wrapped.LogError(message, value);
      }

      public void Error(string message, object value1, object value2)
      {
         wrapped.LogError(message, value1, value2);
      }

      public void Error(string message, params object[] values)
      {
         wrapped.LogError(message, values);
      }

      public void Error(string message, Exception exception)
      {
         wrapped.Log(LogLevel.Error, 0, exception, message, null);
      }

      public void Info(string message)
      {
         wrapped.LogInformation(message);
      }

      public void Info(string message, object value)
      {
         wrapped.LogInformation(message, value);
      }

      public void Info(string message, object value1, object value2)
      {
         wrapped.LogInformation(message, value1, value2);
      }

      public void Info(string message, params object[] values)
      {
         wrapped.LogInformation(message, values);
      }

      public void Info(string message, Exception exception)
      {
         wrapped.Log(LogLevel.Information, 0, exception, message, null);
      }

      public void Trace(string message)
      {
         wrapped.LogTrace(message);
      }

      public void Trace(string message, object value)
      {
         wrapped.LogTrace(message, value);
      }

      public void Trace(string message, object value1, object value2)
      {
         wrapped.LogTrace(message, value1, value2);
      }

      public void Trace(string message, params object[] values)
      {
         wrapped.LogTrace(message, values);
      }

      public void Trace(string message, Exception exception)
      {
         wrapped.Log(LogLevel.Trace, 0, exception, message, null);
      }

      public void Warn(string message)
      {
         wrapped.LogWarning(message);
      }

      public void Warn(string message, object value)
      {
         wrapped.LogWarning(message, value);
      }

      public void Warn(string message, object value1, object value2)
      {
         wrapped.LogWarning(message, value1, value2);
      }

      public void Warn(string message, params object[] values)
      {
         wrapped.LogWarning(message, values);
      }

      public void Warn(string message, Exception exception)
      {
         wrapped.Log(LogLevel.Warning, 0, exception, message, null);
      }
   }
}