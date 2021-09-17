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

using Apache.Qpid.Proton.Engine.Implementation;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Interface used to define the basic mechanisms for creating Engine instances.
   /// </summary>
   public interface IEngineFactory
   {
      public static readonly IEngineFactory Proton = ProtonEngineFactory.Instance;

      /// <summary>
      /// Create a new Engine instance with a SASL authentication layer added. The returned
      /// Engine can either be fully pre-configured for SASL or can require additional user
      /// configuration.
      /// </summary>
      /// <returns>a new Engine instance that can handle SASL authentication.</returns>
      IEngine CreateEngine();

      /// <summary>
      /// Create a new Engine instance that handles only raw AMQP with no SASL layer enabled.
      /// </summary>
      /// <returns>a new raw AMQP aware Engine implementation.</returns>
      IEngine CreateNonSaslEngine();

   }
}