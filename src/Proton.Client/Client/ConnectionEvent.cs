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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// An event object that accompanies events fired to handlers configured in the
   /// Connection options which are signaled during specific Connection event points.
   /// </summary>
   public class ConnectionEvent : EventArgs
   {
      /// <summary>
      /// Creates the immutable connection event object
      /// </summary>
      /// <param name="host">The host where the connection occurred</param>
      /// <param name="port">The port on the host where the connection occurred</param>
      public ConnectionEvent(string host, int port) : base()
      {
         Host = host;
         Port = port;
      }

      /// <summary>
      /// The host that the connection was established on.
      /// </summary>
      public string Host { get; }

      /// <summary>
      /// The port on the remote host where the connection was established
      /// </summary>
      public int Port { get; }

   }
}