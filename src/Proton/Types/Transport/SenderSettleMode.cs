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

namespace Apache.Qpid.Proton.Types.Transport
{
   public enum SenderSettleMode : byte
   {
      Unsettled,
      Settled,
      Mixed
   }

   public static class SenderSettleModeExtension
   {
      public static byte ByteValue(this SenderSettleMode mode)
      {
         return (byte)mode;
      }

      public static SenderSettleMode Lookup(byte mode)
      {
         switch (mode)
         {
            case 0:
               return SenderSettleMode.Unsettled;
            case 1:
               return SenderSettleMode.Settled;
            case 2:
               return SenderSettleMode.Mixed;
            default:
               throw new ArgumentOutOfRangeException("Sender settlement role value out or range [0...2]");
         }
      }
   }
}