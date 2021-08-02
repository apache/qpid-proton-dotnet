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

namespace Apache.Qpid.Proton.Types.Messaging
{
   public enum TerminusExpiryPolicy
   {
      LinkDetach,
      SessionEnd,
      ConnectionClose,
      Never
   }

   public static class TerminusExpiryPolicyExtension
   {
      private static readonly Symbol LinkDetach = Symbol.Lookup("link-detach");
      private static readonly Symbol SessionEnd = Symbol.Lookup("session-end");
      private static readonly Symbol ConnectionClose = Symbol.Lookup("connection-close");
      private static readonly Symbol Never = Symbol.Lookup("never");

      public static Symbol ToSymbol(this TerminusExpiryPolicy mode)
      {
         switch (mode)
         {
            case TerminusExpiryPolicy.LinkDetach:
               return LinkDetach;
            case TerminusExpiryPolicy.SessionEnd:
               return SessionEnd;
            case TerminusExpiryPolicy.ConnectionClose:
               return ConnectionClose;
            case TerminusExpiryPolicy.Never:
               return Never;
            default:
               throw new ArgumentOutOfRangeException("Terminus Expiry Policy value was invalid: " + mode);
         }
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

         switch (policy)
         {
            case "link-detach":
               return TerminusExpiryPolicy.LinkDetach;
            case "session-end":
               return TerminusExpiryPolicy.SessionEnd;
            case "connection-close":
               return TerminusExpiryPolicy.ConnectionClose;
            case "never":
               return TerminusExpiryPolicy.Never;
            default:
               throw new ArgumentOutOfRangeException("Terminus Expiry Policy value was invalid: " + policy);
         }
      }
   }
}