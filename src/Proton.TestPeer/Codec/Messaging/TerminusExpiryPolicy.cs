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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Messaging
{
   public enum TerminusExpiryPolicy : uint
   {
      LinkDetach,
      SessionEnd,
      ConnectionClose,
      Never
   }

   public static class TerminusExpiryPolicyExtension
   {
      private static readonly Symbol LinkDetach = new Symbol("link-detach");
      private static readonly Symbol SessionEnd = new Symbol("session-end");
      private static readonly Symbol ConnectionClose = new Symbol("connection-close");
      private static readonly Symbol Never = new Symbol("never");

      public static Symbol ToSymbol(this TerminusExpiryPolicy mode)
      {
         return mode switch
         {
            TerminusExpiryPolicy.LinkDetach => LinkDetach,
            TerminusExpiryPolicy.SessionEnd => SessionEnd,
            TerminusExpiryPolicy.ConnectionClose => ConnectionClose,
            TerminusExpiryPolicy.Never => Never,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), "Unknown TerminusExpiryPolicy"),
         };
      }

      public static TerminusExpiryPolicy Lookup(Symbol policy)
      {
         if (policy == null)
         {
            return TerminusExpiryPolicy.SessionEnd;
         }

         return Lookup(policy.ToString());
      }

      public static TerminusExpiryPolicy Lookup(string policy)
      {
         if (policy == null)
         {
            return TerminusExpiryPolicy.SessionEnd;
         }

         return policy switch
         {
            "link-detach" => TerminusExpiryPolicy.LinkDetach,
            "session-end" => TerminusExpiryPolicy.SessionEnd,
            "connection-close" => TerminusExpiryPolicy.ConnectionClose,
            "never" => TerminusExpiryPolicy.Never,
            _ => throw new ArgumentOutOfRangeException(nameof(policy), "Terminus Expiry Policy value was invalid: " + policy),
         };
      }
   }
}