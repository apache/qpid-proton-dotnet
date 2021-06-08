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
   public static class LinkError
   {
      /// <summary>
      /// An operator intervened to detach for some reason. 
      /// </summary>                 
      public static Symbol DETACH_FORCED = Symbol.Lookup("amqp:link:detach-forced");

      /// <summary>
      /// The peer sent more message transfers than currently allowed on the link. 
      /// </summary>                 
      public static Symbol TRANSFER_LIMIT_EXCEEDED = Symbol.Lookup("amqp:link:transfer-limit-exceeded");

      /// <summary>
      /// The peer sent a larger message than is supported on the link.
      /// </summary>                 
      public static Symbol MESSAGE_SIZE_EXCEEDED = Symbol.Lookup("amqp:link:message-size-exceeded");

      /// <summary>
      /// The address provided cannot be resolved to a terminus at the current container. The info map
      /// MAY contain the following information to allow the client to locate the attach to the terminus.
      /// 
      /// hostname: the hostname of the container hosting the terminus. This is the value that SHOULD be
      ///           supplied in the hostname field of the open frame, and during SASL and TLS negotiation
      ///           (if used).
      /// network-host: the DNS hostname or IP address of the machine hosting the container.
      /// port: the port number on the machine hosting the container.
      /// </summary>                 
      public static Symbol REDIRECT = Symbol.Lookup("amqp:link:redirect");

      /// <summary>
      /// The link has been attached elsewhere, causing the existing attachment to be forcibly closed. 
      /// </summary>                 
      public static Symbol STOLEN = Symbol.Lookup("amqp:link:stolen");

   }
}