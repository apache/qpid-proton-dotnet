/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed With
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance With
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

using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Performative into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public class DischargeInjectAction : TransferInjectAction
   {
      private readonly Discharge discharge = new();

      public DischargeInjectAction(AMQPTestDriver driver) : base(driver)
      {
         WithBody().WithDescribed(discharge);
      }

      public DischargeInjectAction WithFail(bool fail)
      {
         discharge.Fail = fail;
         return this;
      }

      public DischargeInjectAction WithTxnId(byte[] txnId)
      {
         discharge.TxnId = new Binary(txnId);
         return this;
      }

      public DischargeInjectAction WithTxnId(Binary txnId)
      {
         discharge.TxnId = txnId;
         return this;
      }

      public DischargeInjectAction WithDischarge(Discharge discharge)
      {
         WithBody().WithDescribed(discharge);
         return this;
      }

      public override DischargeInjectAction WithHandle(uint handle)
      {
         return (DischargeInjectAction)base.WithHandle(handle);
      }
   }
}