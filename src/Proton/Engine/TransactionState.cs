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

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// A Transaction state enumeration that provides insight into what the
   /// current state of a transaction is.
   /// </summary>
   public enum TransactionState
   {
      /// <summary>
      /// A Transaction is considered IDLE until the transaction manager responds
      /// that it has been declared successfully and an transaction Id has been
      /// assigned.
      /// </summary>
      Idle,

      /// <summary>
      /// A Transaction is considered declaring once a Declare command has been sent to
      /// the remote but before any declared response has been received which assigns the
      /// transaction Id value.
      /// </summary>
      Declaring,

      /// <summary>
      /// A Transaction is considered declared once the TransactionManager has responded
      /// in the affirmative and assigned a transaction Id.
      /// </summary>
      Declared,

      /// <summary>
      /// A is considered to b discharging once a Discharge command has been sent to the
      /// remote but before any response has been received indicating the outcome of the
      /// attempted discharge.
      /// </summary>
      Discharging,

      /// <summary>
      /// A Transaction is considered discharged once a discharge has been requested and the
      /// transaction manager has responded in the affirmative that the request has been honored.
      /// </summary>
      Discharged,

      /// <summary>
      /// A Transaction is considered failed if the transaction manager responds with an error
      /// to the declaration action.
      /// </summary>
      DelcareFailed,

      /// <summary>
      /// A Transaction is considered failed in the transaction manager responds with an error
      /// to the discharge action.
      /// </summary>
      DischargeFailed

   }
}