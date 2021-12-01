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
using System.IO;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// A TCP based test driver client that will attempt one connection and then
   /// proceeds to run the configured test script actions and apply scripted
   /// expectations to incoming AMQP frames.
   /// </summary>
   public sealed class ProtonTestClient : ProtonTestPeer
   {
      private readonly AMQPTestDriver driver;

      public override AMQPTestDriver Driver => driver;

      protected override string PeerName => "Client";

      protected override void ProcessCloseRequest()
      {
         throw new NotImplementedException();
      }

      protected override void ProcessConnectionEstablished()
      {
         throw new NotImplementedException();
      }

      protected override void ProcessDriverOutput(Stream output)
      {
         throw new NotImplementedException();
      }
   }
}