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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// The distribution-mode of a source determines how messages from a distribution node
   /// are distributed among its associated links.
   /// </summary>
   public enum DistributionMode
   {
      /// <summary>
      /// The copy distribution-mode leaves the state of the message unchanged at the distribution node.
      /// </summary>
      Copy,

      /// <summary>
      /// Causes messages transferred from the distribution node to transition to the ACQUIRED state
      /// prior to transfer over the link, and subsequently to the ARCHIVED state when the transfer
      /// is settled with a successful outcome.
      /// </summary>
      Move

   }
}