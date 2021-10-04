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

using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Declare into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public sealed class DeclareInjectAction : TransferInjectAction
   {
      private readonly Declare declare = new Declare();

      public DeclareInjectAction(AMQPTestDriver driver) : base(driver)
      {
         WithBody().WithDescribed(new Declare());
      }

      public DeclareInjectAction WithDeclare(Declare declare)
      {
         WithBody().WithDescribed(declare);
         return this;
      }
   }
}