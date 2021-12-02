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

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Base options class for common options that can be configured for test
   /// peer implementations that operate over a network connection.
   /// </summary>
   public abstract class ProtonNetworkPeerOptions : ICloneable
   {
      public static readonly int DEFAULT_SEND_BUFFER_SIZE = 64 * 1024;
      public static readonly int DEFAULT_RECEIVE_BUFFER_SIZE = DEFAULT_SEND_BUFFER_SIZE;
      public static readonly int DEFAULT_TRAFFIC_CLASS = 0;
      public static readonly bool DEFAULT_TCP_NO_DELAY = true;
      public static readonly bool DEFAULT_TCP_KEEP_ALIVE = false;
      public static readonly int DEFAULT_SO_LINGER = int.MinValue;
      public static readonly int DEFAULT_SO_TIMEOUT = -1;
      public static readonly bool DEFAULT_TRACE_BYTES = false;
      public static readonly string DEFAULT_CONTEXT_PROTOCOL = "TLS";
      public static readonly bool DEFAULT_TRUST_ALL = false;
      public static readonly bool DEFAULT_VERIFY_HOST = true;
      public static readonly int DEFAULT_LOCAL_PORT = 0;
      public static readonly bool DEFAULT_USE_WEBSOCKETS = false;
      public static readonly bool DEFAULT_FRAGMENT_WEBSOCKET_WRITES = false;
      public static readonly bool DEFAULT_SECURE_SERVER = false;
      public static readonly bool DEFAULT_NEEDS_CLIENT_AUTH = false;

      public int SendBufferSize { get; set; } = DEFAULT_SEND_BUFFER_SIZE;

      public int ReceiveBufferSize { get; set; } = DEFAULT_RECEIVE_BUFFER_SIZE;

      public int TrafficClass { get; set; } = DEFAULT_TRAFFIC_CLASS;

      public int SoTimeout { get; set; } = DEFAULT_SO_TIMEOUT;

      public int SoLinger { get; set; } = DEFAULT_SO_LINGER;

      public bool TcpKeepAlive { get; set; } = DEFAULT_TCP_KEEP_ALIVE;

      public bool TcpNoDelay { get; set; } = DEFAULT_TCP_NO_DELAY;

      public string LocalAddress { get; set; }

      public int LocalPort { get; set; } = DEFAULT_LOCAL_PORT;

      public bool TraceBytes { get; set; } = DEFAULT_TRACE_BYTES;

      public bool UseWebSockets { get; set; } = DEFAULT_USE_WEBSOCKETS;

      public bool FragmentWebSocketWrites { get; set; } = DEFAULT_FRAGMENT_WEBSOCKET_WRITES;

      public virtual object Clone()
      {
         return base.MemberwiseClone();
      }
   }
}