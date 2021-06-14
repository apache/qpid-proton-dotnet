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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Base options type for the terminus configuration for Source and Target types
   /// that configure the nodes for Sender and Receiver links.
   /// </summary>
   public abstract class TerminusOptions
   {
      /// <summary>
      /// Configures the Terminus durability mode.
      /// </summary>
      public DurabilityMode? DurabilityMode { get; set; }

      /// <summary>
      /// Terminus timeout configuration.
      /// </summary>
      public uint? Timeout { get; set; }

      /// <summary>
      /// Configures the expiry policy for the Terminus.
      /// </summary>
      public ExpiryPolicy? ExpiryPolicy { get; set; }

      /// <summary>
      /// Capabilities that are assigned to the created Terminus
      /// </summary>
      public string[] Capabilities { get; set; }

      protected void CopyInto(TerminusOptions other)
      {
         other.DurabilityMode = DurabilityMode;
         other.ExpiryPolicy = ExpiryPolicy;
         other.Timeout = Timeout;

         if (Capabilities != null)
         {
            string[] copyOf = new string[Capabilities.Length];
            Array.Copy(Capabilities, copyOf, Capabilities.Length);
            other.Capabilities = copyOf;
         }
      }
   }
}