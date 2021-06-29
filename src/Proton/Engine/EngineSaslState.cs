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

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// The SASL driver state used to determine at what point the current SASL negotiation
   /// process is currently in.  If the state is 'none' then no SASL negotiations will be
   /// performed.
   /// </summary>
   public enum EngineSaslState
   {
      /// <summary>
      /// Engine not started, SASL context can be configured
      /// </summary>
      Idle,

      /// <summary>
      /// Engine started and set configuration in use
      /// </summary>
      Authenticating,

      /// <summary>
      /// Authentication succeeded
      /// </summary>
      Authenticated,

      /// <summary>
      /// Authentication failed
      /// </summary>
      AuthenticationFailed,

      /// <summary>
      /// No authentication layer configured.
      /// </summary>
      None

   }
}