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

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Provides options for the proton TCP test client
   /// </summary>
   public sealed class ProtonTestServerOptions : ProtonNetworkPeerOptions
   {
      private static readonly int SERVER_CHOOSES_PORT = 0;

      public static readonly int DEFAULT_SERVER_PORT = SERVER_CHOOSES_PORT;

      /// <summary>
      /// The port that the test peer server will listen on for an incoming
      /// connection from a client. If a value of zero is given the server
      /// will find a free port and listen there, to get the active port
      /// the user would need to start the server and then request the port
      /// from the running server.
      /// </summary>
      public int ServerPort { get; set; } = DEFAULT_SERVER_PORT;

   }
}