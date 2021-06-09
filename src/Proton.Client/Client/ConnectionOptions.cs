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
   public class ConnectionOptions : ICloneable
   {
      /// <summary>
      /// Creates a default Connection options instance.
      /// </summary>
      public ConnectionOptions()
      {
      }

      /// <summary>
      /// Create a new Connection options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The connection options instance to copy</param>
      public ConnectionOptions(ConnectionOptions other)
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, chancges to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new ConnectionOptions());
      }

      protected ConnectionOptions CopyInto(ConnectionOptions other)
      {
         return other;
      }
   }
}