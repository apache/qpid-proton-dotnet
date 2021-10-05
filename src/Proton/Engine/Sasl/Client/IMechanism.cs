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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Interface that is implemented by all SASL mechanisms provided in this
   /// package.
   /// </summary>
   public interface IMechanism
   {
      /// <summary>
      /// Returns the proper name of the SASL mechanism
      /// </summary>
      Symbol Name { get; }

      /// <summary>
      /// Based on the functionality of the implemented mechanism, formulate the
      /// initial response packet that will be sent back to the remote, if no
      /// response is required for this mechanism this method may return null.
      /// </summary>
      /// <param name="credentialsProvider">The provider of the credentials</param>
      /// <returns>The initial response encoded into a proton buffer.</returns>
      IProtonBuffer GetInitialResponse(ISaslCredentialsProvider credentialsProvider);

      /// <summary>
      /// Based on the functionality of the implemented mechanism, formulate the
      /// challenge response packet that will be sent back to the remote.  If the
      /// mechanism is not expecting a challenge this method my throw an exception.
      /// </summary>
      /// <param name="credentialsProvider">The provider of the credentials</param>
      /// <param name="challenge">The encoded challenge received from the remote</param>
      /// <returns>The challenge response encoded into a proton buffer.</returns>
      /// <exception cref="SaslException"></exception>
      IProtonBuffer GetChallengeResponse(ISaslCredentialsProvider credentials, IProtonBuffer challenge);

      /// <summary>
      /// Verifies that the SASL exchange has completed successfully. This is an
      /// opportunity for the mechanism to ensure that all mandatory steps have been
      /// completed successfully and to cleanup and resources that are held by this
      /// Mechanism. When verification fails this method throw a SaslException.
      /// </summary>
      /// <exception cref="SaslException"></exception>
      void VerifyCompletion();

      /// <summary>
      /// Allows the Mechanism to determine if it is a valid choice based on the configured
      /// credentials at the time of selection. The sasl processing layer must query each
      /// mechanism to determine its applicability and must not attempt to utilize a given
      /// mechanism if it reports it cannot be applied using the provided credentials.
      /// </summary>
      /// <param name="credentialsProvider">The provider of the credentials</param>
      /// <returns>true if the mechanism can operate given the provided credentials</returns>
      bool IsApplicable(ISaslCredentialsProvider credentials);

      /// <summary>
      /// Allows the mechanism to indicate if it is enabled by default, or only when explicitly enabled
      /// through configuring the permitted SASL mechanisms.  Any mechanism selection logic should examine
      /// this value along with the configured allowed mechanism and decide if this one should be used.
      /// </summary>
      /// <remarks>
      /// Typically most mechanisms can be enabled by default but some require explicit configuration
      /// in order to operate which implies that selecting them by default would always cause an
      /// authentication error if that mechanism matches the highest priority value offered by the remote
      /// peer.
      /// </remarks>
      /// <returns>true if the mechanim should used without it having been requested</returns>
      bool IsEnabledByDefault();

   }
}