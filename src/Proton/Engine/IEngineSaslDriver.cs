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

using Apache.Qpid.Proton.Engine.Sasl;

namespace Apache.Qpid.Proton.Engine
{
   public interface IEngineSaslDriver
   {
      /// <summary>
      /// Configure this IEngineSaslDriver to operate in client mode and return the
      /// associated ISaslClientContext instance that should be used to complete the
      /// SASL negotiation with the server end.
      /// </summary>
      /// <returns>A SASL Client context instance</returns>
      /// <exception cref="InvalidOperationException">
      /// If the engine is in server mode or has not been configured with SASL support.
      /// </exception>
      ISaslClientContext Client();

      /// <summary>
      /// Configure this IEngineSaslDriver to operate in server mode and return the
      /// associated ISaslServerContext instance that should be used to complete the
      /// SASL negotiation with the client end.
      /// </summary>
      /// <returns>A SASL Server context instance</returns>
      /// <exception cref="InvalidOperationException">
      /// If the engine is in client mode or has not been configured with SASL support.
      /// </exception>
      ISaslServerContext Server();

      /// <summary>
      /// Returns a SaslState that indicates the current operating state of the SASL
      /// negotiation process or conversely if no SASL layer is configured this method
      /// should return the disabled state. This method must never return a null result.
      /// </summary>
      EngineSaslState SaslState { get; }

      /// <summary>
      /// Provides a low level outcome value for the SASL authentication process.
      /// <para/>
      /// If the SASL exchange is ongoing or the SASL layer was skipped because a
      /// particular engine configuration allows such behavior then this method
      /// should return null to indicate no SASL outcome is available.
      /// </summary>
      SaslAuthOutcome? SaslOutcome { get; }

      /// <summary>
      /// Provides access to the SASL drivers configured max frame size value, the
      /// max frame size can be updated before the engine has been started but is
      /// locked for updates following an engine start.
      /// </summary>
      uint MaxFrameSize { get; set; }

   }
}