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
   public static class ConnectionError
   {
      /// <summary>
      /// An operator intervened to close the connection for some reason. The client could retry at some later date.
      /// </summary>
      public static readonly Symbol CONNECTION_FORCED = Symbol.Lookup("amqp:connection:forced");

      /// <summary>
      /// A valid frame header cannot be formed from the incoming byte stream.
      /// </summary>
      public static readonly Symbol FRAMING_ERROR = Symbol.Lookup("amqp:connection:framing-error");

      /// <summary>
      /// The container is no longer available on the current connection. The peer SHOULD
      /// attempt reconnection to the container using the details provided in the info map.
      ///
      /// hostname: the hostname of the container hosting the terminus. This is the value that SHOULD be
      ///           supplied in the hostname field of the open frame, and during SASL and TLS negotiation
      ///           (if used).
      /// network-host: the DNS hostname or IP address of the machine hosting the container.
      /// port: the port number on the machine hosting the container.
      /// </summary>
      public static readonly Symbol REDIRECT = Symbol.Lookup("amqp:connection:redirect");

   }
}