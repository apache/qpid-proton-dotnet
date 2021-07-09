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
using Apache.Qpid.Proton.Types.Security;

namespace Apache.Qpid.Proton.Engine.Sasl
{
   /// <summary>
   /// Represents the outcome of a SASL exchange.
   /// </summary>
   public enum SaslOutcome
   {
      /// <summary>
      /// SASL Authentication succeeded.
      /// </summary>
      SaslOk,

      /// <summary>
      /// SASL Authentication failed due to bad credentials
      /// </summary>
      SaslAuth,

      /// <summary>
      /// SASL Authentication failed due to a system error.
      /// </summary>
      SaslSys,

      /// <summary>
      /// SASL Authentication failed due to an unrecoverable error.
      /// </summary>
      SaslPerm,

      /// <summary>
      /// SASL Authentication failed due to a temporary failure.
      /// </summary>
      SaslTemp
   }

   public static class TerminusDurabilityExtension
   {
      public static SaslCode ToSaslCode(this SaslOutcome outcome)
      {
         switch (outcome)
         {
            case SaslOutcome.SaslOk:
               return SaslCode.Ok;
            case SaslOutcome.SaslAuth:
               return SaslCode.Auth;
            case SaslOutcome.SaslSys:
               return SaslCode.Sys;
            case SaslOutcome.SaslPerm:
               return SaslCode.SysPerm;
            case SaslOutcome.SaslTemp:
               return SaslCode.SysTemp;
            default:
               throw new ArgumentException("Unknown SASL outcome type");
         }
      }
   }
}