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

namespace Apache.Qpid.Proton.Common.Logging
{
   /// <summary>
   /// Proton defined logging API used to abstract the ultimate source of
   /// the logging service and provide consistent API mechanics to the proton
   /// code.
   /// </summary>
   public interface IProtonLogger
   {
      /// <summary>
      /// Returns the name that was used to create this logger instance.
      /// </summary>
      string LoggerName { get; }

      /// <summary>
      /// Returns true if logging configuration has enabled trace level logs.
      /// </summary>
      bool IsTraceEnabled { get; }

      /// <summary>
      /// Returns true if logging configuration has enabled debug level logs.
      /// </summary>
      bool IsDebugEnabled { get; }

      /// <summary>
      /// Returns true if logging configuration has enabled information level logs.
      /// </summary>
      bool IsInfoEnabled { get; }

      /// <summary>
      /// Returns true if logging configuration has enabled warning level logs.
      /// </summary>
      bool IsWarnEnabled { get; }

      /// <summary>
      /// Returns true if logging configuration has enabled error level logs.
      /// </summary>
      bool IsErrorEnabled { get; }

      /// <summary>
      /// Logs a message at the trace level if enabled.
      /// </summary>
      /// <param name="message">the message to be logged</param>
      void Trace(string message);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given argument if trace level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value">the argument to provide for formatting</param>
      void Trace(string message, object value);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if trace level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value1">the 1st argument to provide for formatting</param>
      /// <param name="value2">the 2nd argument to provide for formatting</param>
      void Trace(string message, object value1, object value2);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if trace level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="values">the arguments to provide for formatting</param>
      void Trace(string message, params object[] values);

      /// <summary>
      /// Logs a message along with the given exception if trace logging is enabled.
      /// </summary>
      /// <param name="message">the description string to log with the exception</param>
      /// <param name="exception">the exception to log along with the message</param>
      void Trace(string message, Exception exception);

      /// <summary>
      /// Logs a message at the debug level if enabled.
      /// </summary>
      /// <param name="message">the message to be logged</param>
      void Debug(string message);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given argument if debug level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value">the argument to provide for formatting</param>
      void Debug(string message, object value);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if debug level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value1">the 1st argument to provide for formatting</param>
      /// <param name="value2">the 2nd argument to provide for formatting</param>
      void Debug(string message, object value1, object value2);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if debug level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="values">the arguments to provide for formatting</param>
      void Debug(string message, params object[] values);

      /// <summary>
      /// Logs a message along with the given exception if debug logging is enabled.
      /// </summary>
      /// <param name="message">the description string to log with the exception</param>
      /// <param name="exception">the exception to log along with the message</param>
      void Debug(string message, Exception exception);

      /// <summary>
      /// Logs a message at the information level if enabled.
      /// </summary>
      /// <param name="message">the message to be logged</param>
      void Info(string message);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given argument if information level logging is
      /// enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value">the argument to provide for formatting</param>
      void Info(string message, object value);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if information level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value1">the 1st argument to provide for formatting</param>
      /// <param name="value2">the 2nd argument to provide for formatting</param>
      void Info(string message, object value1, object value2);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if information level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="values">the arguments to provide for formatting</param>
      void Info(string message, params object[] values);

      /// <summary>
      /// Logs a message along with the given exception if information logging is enabled.
      /// </summary>
      /// <param name="message">the description string to log with the exception</param>
      /// <param name="exception">the exception to log along with the message</param>
      void Info(string message, Exception exception);

      /// <summary>
      /// Logs a message at the warn level if enabled.
      /// </summary>
      /// <param name="message">the message to be logged</param>
      void Warn(string message);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given argument if warn level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value">the argument to provide for formatting</param>
      void Warn(string message, object value);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if warn level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value1">the 1st argument to provide for formatting</param>
      /// <param name="value2">the 2nd argument to provide for formatting</param>
      void Warn(string message, object value1, object value2);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if warn level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="values">the arguments to provide for formatting</param>
      void Warn(string message, params object[] values);

      /// <summary>
      /// Logs a message along with the given exception if warn logging is enabled.
      /// </summary>
      /// <param name="message">the description string to log with the exception</param>
      /// <param name="exception">the exception to log along with the message</param>
      void Warn(string message, Exception exception);

      /// <summary>
      /// Logs a message at the error level if enabled.
      /// </summary>
      /// <param name="message">the message to be logged</param>
      void Error(string message);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given argument if error level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value">the argument to provide for formatting</param>
      void Error(string message, object value);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if error level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="value1">the 1st argument to provide for formatting</param>
      /// <param name="value2">the 2nd argument to provide for formatting</param>
      void Error(string message, object value1, object value2);

      /// <summary>
      /// Logs a message after applying a string format to the message
      /// and providing the given arguments if error level logging is enabled.
      /// </summary>
      /// <param name="message">the message format string</param>
      /// <param name="values">the arguments to provide for formatting</param>
      void Error(string message, params object[] values);

      /// <summary>
      /// Logs a message along with the given exception if error logging is enabled.
      /// </summary>
      /// <param name="message">the description string to log with the exception</param>
      /// <param name="exception">the exception to log along with the message</param>
      void Error(string message, Exception exception);

   }
}