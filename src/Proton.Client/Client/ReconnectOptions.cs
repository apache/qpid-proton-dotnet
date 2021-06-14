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
   /// Reconnection options which will control how a connection deals will connection loss
   /// and or inability to connect to the host it was provided at create time.
   /// </summary>
   public class ReconnectOptions : ICloneable
   {
      /// <summary>
      /// Creates a default reconnect options instance.
      /// </summary>
      public ReconnectOptions() : base()
      {
      }

      /// <summary>
      /// Create a new reconnection options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The reconnect options instance to copy</param>
      public ReconnectOptions(ReconnectOptions other) : base()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public virtual object Clone()
      {
         return CopyInto(new ReconnectOptions());
      }

      internal ReconnectOptions CopyInto(ReconnectOptions other)
      {
         other.ReconnectEnabled = ReconnectEnabled;

         return other;
      }

      /// <summary>
      /// Configure if a connection will attempt reconnection if connection is lost or cannot
      /// be established to the primary host provided at create time.
      /// </summary>
      public bool ReconnectEnabled { get; set; }

   }
}