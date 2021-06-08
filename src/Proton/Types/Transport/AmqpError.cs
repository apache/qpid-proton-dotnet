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

namespace Apache.Qpid.Proton.Types.Transport
{
   public static class AmqpError
   {
      /// <summary>
      /// An internal error occurred. Operator intervention might be necessary to resume normal operation.
      /// </summary>                 
      public static Symbol INTERNAL_ERROR = Symbol.Lookup("amqp:internal-error");

      /// <summary>
      /// A peer attempted to work with a remote entity that does not exist. 
      /// </summary>                 
      public static Symbol NOT_FOUND = Symbol.Lookup("amqp:not-found");

      /// <summary>
      /// A peer attempted to work with a remote entity to which it has no access due to security settings. 
      /// </summary>                 
      public static Symbol UNAUTHORIZED_ACCESS = Symbol.Lookup("amqp:unauthorized-access");

      /// <summary>
      /// Data could not be decoded. 
      /// </summary>                 
      public static Symbol DECODE_ERROR = Symbol.Lookup("amqp:decode-error");

      /// <summary>
      /// A peer exceeded its resource allocation. 
      /// </summary>                 
      public static Symbol RESOURCE_LIMIT_EXCEEDED = Symbol.Lookup("amqp:resource-limit-exceeded");

      /// <summary>
      /// The peer tried to use a frame in a manner that is inconsistent with the semantics defined in the specification. 
      /// </summary>                 
      public static Symbol NOT_ALLOWED = Symbol.Lookup("amqp:not-allowed");

      /// <summary>
      /// An invalid field was passed in a frame body, and the operation could not proceed. 
      /// </summary>                 
      public static Symbol INVALID_FIELD = Symbol.Lookup("amqp:invalid-field");

      /// <summary>
      /// The peer tried to use functionality that is not implemented in its partner. 
      /// </summary>                 
      public static Symbol NOT_IMPLEMENTED = Symbol.Lookup("amqp:not-implemented");

      /// <summary>
      /// The client attempted to work with a server entity to which it has no access because another client is working with it. 
      /// </summary>                 
      public static Symbol RESOURCE_LOCKED = Symbol.Lookup("amqp:resource-locked");

      /// <summary>
      /// The client made a request that was not allowed because some precondition failed. 
      /// </summary>                 
      public static Symbol PRECONDITION_FAILED = Symbol.Lookup("amqp:precondition-failed");

      /// <summary>
      /// A server entity the client is working with has been deleted. 
      /// </summary>                 
      public static Symbol RESOURCE_DELETED = Symbol.Lookup("amqp:resource-deleted");

      /// <summary>
      /// The peer sent a frame that is not permitted in the current state. 
      /// </summary>                 
      public static Symbol ILLEGAL_STATE = Symbol.Lookup("amqp:illegal-state");

      /// <summary>
      /// The peer cannot send a frame because the smallest encoding of the performative with the currently valid
      /// values would be too large to fit within a frame of the agreed maximum frame size. When transferring a message
      /// the message data can be sent in multiple transfer frames thereby avoiding this error. Similarly when attaching
      /// a link with a large unsettled map the endpoint MAY make use of the incomplete-unsettled flag to avoid the need
      /// for overly large frames.
      /// </summary>                 
     public static Symbol FRAME_SIZE_TOO_SMALL = Symbol.Lookup("amqp:frame-size-too-small");

   }
}