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

using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl
{
   /// <summary>
   /// Root context of a SASL authentication API which provides common elements
   /// used in both clients and servers.
   /// </summary>
   public interface ISaslContext
   {
      /// <summary>
      /// Returns the role this context plays either client or server
      /// </summary>
      SaslContextRole Role { get; }

      /// <summary>
      /// Access to the linked attachments instance where properties can be attached
      /// to this context for later application use.
      /// </summary>
      IAttachments Attachments { get; }

      /// <summary>
      /// Checks if SASL authentication has completed and an outcome is available.
      /// </summary>
      bool IsDone { get; }

      /// <summary>
      /// Gets the outcome of the SASL authentication process.
      /// <para/>
      /// If the SASL exchange is ongoing or the SASL layer was skipped because a particular
      /// engine configuration allows such behavior then this method should return null to
      /// indicate no SASL outcome is available.
      /// </summary>
      SaslOutcome Outcome { get; }

      /// <summary>
      /// Returns a state enum that indicates the current operating state of the SASL
      /// negotiation process or conversely if no SASL layer is configured this method
      /// should return the no-SASL state.  This method must never return a null result.
      /// </summary>
      EngineSaslState State { get; }

      /// <summary>
      /// After the server has sent its supported mechanisms this method will return a
      /// copy of that list for review by the server event handler.  If called before
      /// the server has sent the mechanisms list this method will return null.
      /// </summary>
      Symbol[] ServerMechanisms { get; }

      /// <summary>
      /// Returns the mechanism that was sent to the server to select the SASL mechanism
      /// to use for negotiations. If called before the client has sent its chosen mechanism
      /// this method returns null.
      /// </summary>
      Symbol[] ChosenMechanisms { get; }

      /// <summary>
      /// The DNS name of the host (either fully qualified or relative) that was sent to the
      /// server which define the host the sending peer is connecting to. If called before
      /// the client sent the host name information to the server this method returns null.
      /// </summary>
      string Hostname { get; }

   }
}