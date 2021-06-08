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

namespace Apache.Qpid.Proton.Types.Transport
{
   public static class TransactionError
   {
      /// <summary>
      /// The remote sent a transactional request with an unknown transaction id.
      /// </summary>                 
      public static Symbol UNKNOWN_ID = Symbol.Lookup("amqp:transaction:unknown-id");

      /// <summary>
      /// The transaction has been rolled back by the remote and cannot be operated upon.
      /// </summary>                 
      public static Symbol TRANSACTION_ROLLBACK = Symbol.Lookup("amqp:transaction:rollback");

      /// <summary>
      /// The transaction has timed out by the remote and cannot be operated upon.
      /// </summary>                 
      public static Symbol TRANSACTION_TIMEOUT = Symbol.Lookup("amqp:transaction:timeout");

   }
}