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
using System.Text;
using System.Threading;

namespace Apache.Qpid.Proton.Client.Utilities
{
   public sealed class IdGenerator
   {
      public static readonly String DefaultPrefix = "ID:";

      private int sequenceId;

      public IdGenerator()
      {
      }

      public IdGenerator(string prefix) : this()
      {
         Prefix = prefix;
      }

      /// <summary>
      /// Access the Prefix value that is used when creating new Ids from this generator
      /// </summary>
      public string Prefix { get; set; } = DefaultPrefix;

      /// <summary>
      /// Create and returns a new unique Id value from this generator using the
      /// configure prefix value for the returned Id.
      /// </summary>
      /// <returns></returns>
      public string GenerateId()
      {
         StringBuilder sb = new StringBuilder(64);

         sb.Append(Prefix);
         sb.Append(Guid.NewGuid());
         sb.Append(":");
         sb.Append(Interlocked.Increment(ref sequenceId));

         return sb.ToString();
      }
   }
}
