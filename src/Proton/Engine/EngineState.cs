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
   /// Enumerates the possible states that the Proton AMQM engine can occupy.
   /// </summary>
   public enum EngineState
   {
      /// <summary>
      /// The engine has not yet been started and is safet to configure.
      /// </summary>
      Idle,

      /// <summary>
      /// Indicates the engine is in the starting phase and configuration is safe to use now. 
      /// </summary>
      Starting,

      /// <summary>
      /// The engine has been started and no changes to configuration are permissible. 
      /// </summary>
      Started,

      /// <summary>
      /// The engine has encountered an error and is no longer usable.
      /// </summary>
      Failed,

      /// <summary>
      /// Engine is shutting down and all pending work should be completed.
      /// </summary>
      ShuttingDown,

      /// <summary>
      /// The engine has been shutdown and can no longer be used.
      /// </summary>
      Shutdown
   }
}