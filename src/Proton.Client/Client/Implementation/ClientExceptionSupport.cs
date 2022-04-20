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
using System.Collections.Generic;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Exception support API for creating or passing through client
   /// exception types and or wrapping when a fatal vs non-fatal type
   /// needs to be presented.
   /// </summary>
   public static class ClientExceptionSupport
   {
      /// <summary>
      /// Checks the given cause to determine if it's already an ClientIOException type and
      /// if not creates a new ClientIOException to wrap it.
      /// </summary>
      /// <param name="cause">The exception to operate on</param>
      /// <returns>A client exception that is produced using the input exception instance</returns>
      public static ClientIOException CreateOrPassthroughFatal(Exception cause)
      {
         if (cause is ClientIOException exception)
         {
            return exception;
         }

         if (cause.InnerException is ClientIOException exception1)
         {
            return exception1;
         }

         string message = cause.Message;
         if (string.IsNullOrEmpty(message))
         {
            message = cause.ToString();
         }

         return new ClientIOException(message, cause);
      }

      /// <summary>
      /// Checks the given cause to determine if it's already an ClientException type and
      /// if not creates a new ClientException to wrap it. If the inbound exception is a
      /// fatal type then it will pass through this method untouched to preserve the fatal
      /// status of the error.
      /// </summary>
      /// <param name="cause">The exception to operate on</param>
      /// <returns>A client exception that is produced using the input exception instance</returns>
      public static ClientException CreateNonFatalOrPassthrough(Exception cause)
      {
         if (cause is ClientException exception)
         {
            return exception;
         }

         if (cause.InnerException is ClientException exception1)
         {
            return exception1;
         }

         string message = cause.Message;
         if (string.IsNullOrEmpty(message))
         {
            message = cause.ToString();
         }

         // TODO interrogate task exceptions for any client exception

         if (cause is TimeoutException)
         {
            return new ClientOperationTimedOutException(message, cause);
         }
         else if (cause is InvalidOperationException)
         {
            return new ClientIllegalStateException(message, cause);
         }
         else
         {
            return new ClientException(message, cause);
         }
      }

      /// <summary>
      /// Given an ErrorCondition instance create a new Exception that best matches
      /// the error type that indicates the connection creation failed for some reason.
      /// </summary>
      /// <param name="errorCondition">The proton error condition to wrap</param>
      /// <returns>A connection remotely closed variant based on the proton error</returns>
      public static ClientConnectionRemotelyClosedException ConvertToConnectionClosedException(ErrorCondition errorCondition)
      {
         ClientConnectionRemotelyClosedException remoteError;

         if (errorCondition?.Condition != null)
         {
            Symbol error = errorCondition.Condition;
            string message = ExtractErrorMessage(errorCondition);

            if (error.Equals(AmqpError.UNAUTHORIZED_ACCESS))
            {
               remoteError = new ClientConnectionSecurityException(message, new ClientErrorCondition(errorCondition));
            }
            else if (error.Equals(ConnectionError.REDIRECT))
            {
               remoteError = CreateConnectionRedirectException(error, message, errorCondition);
            }
            else
            {
               remoteError = new ClientConnectionRemotelyClosedException(message, new ClientErrorCondition(errorCondition));
            }
         }
         else
         {
            remoteError = new ClientConnectionRemotelyClosedException("Unknown error from remote peer");
         }

         return remoteError;
      }

      /// <summary>
      /// Given an Exception instance create a new Exception that best matches
      /// the error type that indicates the connection creation failed for some reason.
      /// </summary>
      /// <param name="cause">The exception that indicates why the connection closed</param>
      /// <returns>A connection remotely closed that indicates the reason.</returns>
      public static ClientConnectionRemotelyClosedException ConvertToConnectionClosedException(Exception cause)
      {
         ClientConnectionRemotelyClosedException remoteError;

         if (cause is ClientConnectionRemotelyClosedException exception)
         {
            remoteError = exception;
         }
         else if (cause is SaslSystemException saslError)
         {
            remoteError = new ClientConnectionSecuritySaslException(
                cause.Message, cause, !saslError.Permanent);
         }
         else if (cause is SaslException)
         {
            remoteError = new ClientConnectionSecuritySaslException(cause.Message, cause);
         }
         else
         {
            remoteError = new ClientConnectionRemotelyClosedException(cause.Message, cause);
         }

         return remoteError;
      }

      /// <summary>
      /// Given an ErrorCondition instance create a new Exception that best matches
      /// the error type that indicates the session creation failed for some reason.
      /// </summary>
      /// <param name="errorCondition">The proton error condition to wrap</param>
      /// <returns>A session remotely closed variant based on the proton error</returns>
      public static ClientSessionRemotelyClosedException ConvertToSessionClosedException(ErrorCondition errorCondition)
      {
         ClientSessionRemotelyClosedException remoteError;

         if (errorCondition?.Condition != null)
         {
            string message = ExtractErrorMessage(errorCondition);
            if (message == null)
            {
               message = "Session remotely closed without explanation";
            }

            remoteError = new ClientSessionRemotelyClosedException(message, new ClientErrorCondition(errorCondition));
         }
         else
         {
            remoteError = new ClientSessionRemotelyClosedException("Session remotely closed without explanation");
         }

         return remoteError;
      }

      /// <summary>
      /// Given an ErrorCondition instance create a new Exception that best matches
      /// the error type that indicates the link creation failed for some reason.
      /// </summary>
      /// <param name="errorCondition">The proton error condition to wrap</param>
      /// <param name="defaultMessage">An error message to use if no more specific message is available</param>
      /// <returns>A session remotely closed variant based on the proton error</returns>
      public static ClientLinkRemotelyClosedException ConvertToLinkClosedException(ErrorCondition errorCondition, string defaultMessage)
      {
         ClientLinkRemotelyClosedException remoteError;

         if (errorCondition?.Condition != null)
         {
            string message = ExtractErrorMessage(errorCondition);
            Symbol error = errorCondition.Condition;

            if (message == null)
            {
               message = defaultMessage;
            }

            if (error.Equals(LinkError.REDIRECT))
            {
               remoteError = CreateLinkRedirectException(error, message, errorCondition);
            }
            else
            {
               remoteError = new ClientLinkRemotelyClosedException(message, new ClientErrorCondition(errorCondition));
            }
         }
         else
         {
            remoteError = new ClientLinkRemotelyClosedException(defaultMessage);
         }

         return remoteError;
      }

      /// <summary>
      /// Given an ErrorCondition instance create a new Exception that best matches
      /// the error type that indicates a non-fatal error usually at the link level
      /// such as link closed remotely or link create failed due to security access
      /// issues.
      /// </summary>
      /// <param name="errorCondition">The proton error condition to wrap</param>
      /// <returns>A non-fatal client exception based on the proton error</returns>
      public static ClientException ConvertToNonFatalException(ErrorCondition errorCondition)
      {
         ClientException remoteError;

         if (errorCondition?.Condition != null)
         {
            Symbol error = errorCondition.Condition;
            string message = ExtractErrorMessage(errorCondition);

            if (error.Equals(AmqpError.RESOURCE_LIMIT_EXCEEDED))
            {
               remoteError = new ClientResourceRemotelyClosedException(message, new ClientErrorCondition(errorCondition));
            }
            else if (error.Equals(AmqpError.NOT_FOUND))
            {
               remoteError = new ClientResourceRemotelyClosedException(message, new ClientErrorCondition(errorCondition));
            }
            else if (error.Equals(LinkError.DETACH_FORCED))
            {
               remoteError = new ClientResourceRemotelyClosedException(message, new ClientErrorCondition(errorCondition));
            }
            else if (error.Equals(LinkError.REDIRECT))
            {
               remoteError = CreateLinkRedirectException(error, message, errorCondition);
            }
            else if (error.Equals(AmqpError.RESOURCE_DELETED))
            {
               remoteError = new ClientResourceRemotelyClosedException(message, new ClientErrorCondition(errorCondition));
            }
            else if (error.Equals(TransactionError.TRANSACTION_ROLLBACK))
            {
               remoteError = new ClientTransactionRolledBackException(message);
            }
            else
            {
               remoteError = new ClientException(message);
            }
         }
         else
         {
            remoteError = new ClientException("Unknown error from remote peer");
         }

         return remoteError;
      }

      /// <summary>
      /// Attempt to read and return the embedded error message in the given ErrorCondition.
      /// </summary>
      /// <param name="errorCondition">The error condition to inspect</param>
      /// <returns>The extracted error message</returns>
      public static string ExtractErrorMessage(ErrorCondition errorCondition)
      {
         string message = "Received error from remote peer without description";
         if (errorCondition != null)
         {
            if (!string.IsNullOrEmpty(errorCondition.Description))
            {
               message = errorCondition.Description;
            }

            Symbol condition = errorCondition.Condition;
            if (condition != null)
            {
               message = message + " [condition = " + condition + "]";
            }
         }

         return message;
      }

      /// <summary>
      /// When a connection redirect type exception is received this method is called to
      /// create the appropriate redirect exception type containing the error details needed.
      /// </summary>
      /// <param name="error">The symbolic error to use when creating the exception</param>
      /// <param name="message">The message to use for the created exception</param>
      /// <param name="condition">The remote ErrorCondition to use when creating the exception</param>
      /// <returns>A new connection remotely closed exception with redirect data if available</returns>
      public static ClientConnectionRemotelyClosedException CreateConnectionRedirectException(Symbol error, string message, ErrorCondition condition)
      {
         ClientConnectionRemotelyClosedException result;
         IReadOnlyDictionary<Symbol, object> info = condition.Info;

         if (info == null)
         {
            result = new ClientConnectionRemotelyClosedException(
                message + " : Redirection information not set.", new ClientErrorCondition(condition));
         }
         else
         {
            ClientRedirect redirect = new ClientRedirect(info);

            try
            {
               result = new ClientConnectionRedirectedException(
                   message, redirect.Validate(), new ClientErrorCondition(condition));
            }
            catch (Exception ex)
            {
               result = new ClientConnectionRemotelyClosedException(
                   message + " : " + ex.Message, new ClientErrorCondition(condition));
            }
         }

         return result;
      }

      /// <summary>
      /// When a link redirect type exception is received this method is called to create the
      /// appropriate redirect exception type containing the error details needed.
      /// </summary>
      /// <param name="error">The symbolic error to use when creating the exception</param>
      /// <param name="message">The message to use for the created exception</param>
      /// <param name="condition">The remote ErrorCondition to use when creating the exception</param>
      /// <returns>A new link remotely closed exception with redirect data if available</returns>
      public static ClientLinkRemotelyClosedException CreateLinkRedirectException(Symbol error, string message, ErrorCondition condition)
      {
         ClientLinkRemotelyClosedException result;
         IReadOnlyDictionary<Symbol, object> info = condition.Info;

         if (info == null)
         {
            result = new ClientLinkRemotelyClosedException(
                message + " : Redirection information not set.", new ClientErrorCondition(condition));
         }
         else
         {
            ClientRedirect redirect = new ClientRedirect(info);

            try
            {
               result = new ClientLinkRedirectedException(
                   message, redirect.Validate(), new ClientErrorCondition(condition));
            }
            catch (Exception ex)
            {
               result = new ClientLinkRemotelyClosedException(
                   message + " : " + ex.Message, new ClientErrorCondition(condition));
            }
         }

         return result;
      }
   }
}