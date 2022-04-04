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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   /// <summary>
   /// Defines the enumeration values for the role that an AMQP link
   /// is performing either sender or receiver.
   /// </summary>
   public enum Role
   {
      Sender,
      Receiver
   }

   public static class RoleExtension
   {
      public static EncodingCodes ToBooleanEncoding(this Role role)
      {
         return role == Role.Sender ? EncodingCodes.BooleanFalse : EncodingCodes.BooleanTrue;
      }

      public static bool ToBoolean(this Role role)
      {
         return role != Role.Sender;
      }

      public static bool IsSender(this Role role)
      {
         return role == Role.Sender;
      }

      public static bool IsReceiver(this Role role)
      {
         return role == Role.Receiver;
      }

      public static Role ReverseOf(this Role role)
      {
         return role.IsSender() ? Role.Receiver : Role.Sender;
      }

      public static Role Lookup(bool role)
      {
         return role ? Role.Receiver : Role.Sender;
      }
   }
}