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

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Defines an AMQP Protocol Engine interface that should be used to implement
   /// an AMQP Engine.
   /// </summary>
   public interface IEngine
   {
      /// <summary>
      /// Checks if the engine is in the running state and has not failed or been
      /// shutdown yet. Will return false until start is called on the engine.
      /// </summary>
      /// <returns>true if the engine is currently running.</returns>
      bool IsRunning();

      /// <summary>
      /// Checks if the engine has been shutdown which is a terminal state
      /// after which no future engine state changes can occur.
      /// </summary>
      /// <returns></returns>
      bool IsShutdown();

      /// <summary>
      /// Checks if the engine has entered a failed state either by a call to the
      /// engine failed method or by an exception having been thrown and caught.
      /// An engine that reports failed will stop after a call to shutdown.
      /// </summary>
      /// <returns>true if the engine is in a failed state</returns>
      bool IsFailed();

      /// <summary>
      /// Provides an Exception that has information regarding the cause of an
      /// engine entering the failed state.
      /// </summary>
      Exception FailureCause { get; }

      /// <summary>
      /// Provides the current engine operating state.
      /// </summary>
      EngineState EngineState { get; }

   }
}