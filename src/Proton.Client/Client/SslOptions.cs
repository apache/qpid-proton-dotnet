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
   /// <summary>
   /// Options that control the SSL level transport configuration.
   /// </summary>
   public class SslOptions
   {
      public static readonly bool DEFAULT_TRUST_ALL = false;
      public static readonly bool DEFAULT_VERIFY_HOST = true;
      public static readonly int DEFAULT_SSL_PORT = 5671;

      /// <summary>
      /// Creates a default SSL options instance.
      /// </summary>
      public SslOptions() : base()
      {
      }

      /// <summary>
      /// Create a target options instance that copies the configuration from the given instance.
      /// </summary>
      /// <param name="other">The target options instance to copy</param>
      public SslOptions(SslOptions other) : this()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new SslOptions());
      }

      internal SslOptions CopyInto(SslOptions other)
      {
         return this;
      }

      /// <summary>
      /// Controls if SSL is enabled for the connection these options are applied to.
      /// </summary>
      public bool SslEnabled { get; set; }

   }
}