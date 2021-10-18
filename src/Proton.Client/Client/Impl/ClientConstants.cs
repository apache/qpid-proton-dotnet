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

using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Client.Impl
{
   public static class ClientConstants
   {
      // Symbols used to announce connection error information
      public static readonly Symbol CONNECTION_OPEN_FAILED = Symbol.Lookup("amqp:connection-establishment-failed");
      public static readonly Symbol INVALID_FIELD = Symbol.Lookup("invalid-field");
      public static readonly Symbol CONTAINER_ID = Symbol.Lookup("container-id");

      // Symbols used for connection capabilities
      public static readonly Symbol SOLE_CONNECTION_CAPABILITY = Symbol.Lookup("sole-connection-for-container");
      public static readonly Symbol ANONYMOUS_RELAY = Symbol.Lookup("ANONYMOUS-RELAY");
      public static readonly Symbol DELAYED_DELIVERY = Symbol.Lookup("DELAYED_DELIVERY");
      public static readonly Symbol SHARED_SUBS = Symbol.Lookup("SHARED-SUBS");

      // Symbols used to announce connection and link redirect ErrorCondition 'info'
      public static readonly Symbol ADDRESS = Symbol.Lookup("address");
      public static readonly Symbol PATH = Symbol.Lookup("path");
      public static readonly Symbol SCHEME = Symbol.Lookup("scheme");
      public static readonly Symbol PORT = Symbol.Lookup("port");
      public static readonly Symbol NETWORK_HOST = Symbol.Lookup("network-host");
      public static readonly Symbol OPEN_HOSTNAME = Symbol.Lookup("hostname");

      // Symbols used for receivers.
      public static readonly Symbol COPY = Symbol.Lookup("copy");
      public static readonly Symbol MOVE = Symbol.Lookup("move");
      public static readonly Symbol SHARED = Symbol.Lookup("shared");
      public static readonly Symbol GLOBAL = Symbol.Lookup("global");
   }
}