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
   /// Enumerates the possible states that the Proton AMQP link can occupy.
   /// </summary>
   public enum LinkState
   {
      /// <summary>
      /// Indicates that the targeted end of the Link (local or remote) has not yet been opened.
      /// </summary>
      Idle,

      /// <summary>
      /// Indicates that the targeted end of the Link (local or remote) is currently open.
      /// </summary>
      Active,

      /// <summary>
      /// Indicates that the targeted end of the Link (local or remote) has been detached.
      /// </summary>
      Detached,

      /// <summary>
      /// Indicates that the targeted end of the Link (local or remote) has been closed.
      /// </summary>
      Closed
   }

   public static class LinkStateExtension
   {
      public static bool IsClosedOrDetached(this LinkState state)
      {
         return state is LinkState.Closed or LinkState.Detached;
      }

      public static bool IsOpen(this LinkState state)
      {
         return state is LinkState.Active;
      }
   }
}
