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

namespace Apache.Qpid.Proton.Client
{
   public class ClientOptions : ICloneable
   {
      /// <summary>
      /// Creates a default Client options instance.
      /// </summary>
      public ClientOptions()
      {
      }

      /// <summary>
      /// Create a new Client options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The Client options instance to copy</param>
      public ClientOptions(ClientOptions other)
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Configure the Container Id that is set on new Connections created from a
      /// Client instance that was created with these options.
      /// </summary>
      public string Id { get; set; }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new ClientOptions());
      }

      protected ClientOptions CopyInto(ClientOptions other)
      {
         other.Id = this.Id;

         return other;
      }
   }
}