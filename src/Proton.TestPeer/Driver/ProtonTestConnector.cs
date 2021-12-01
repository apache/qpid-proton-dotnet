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
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// An in memory single threaded test driver used for testing Engine implementations
   /// where all test operations will take place in a single thread of control.
   /// <pr>
   /// This class in mainly intended for use in Unit tests of an Engine implementation
   /// and not for use by client implementations where a socket based test peer would be
   /// a more appropriate choice.
   /// </summary>
   public sealed class ProtonTestConnector : ProtonTestPeer
   {
      private readonly AMQPTestDriver driver;
      private Action<Stream> frameSink;

      public ProtonTestConnector(in ILoggerFactory loggerFactory = null)
      {
         this.driver = new AMQPTestDriver(PeerName, ProcessDriverOutput, null, loggerFactory);
      }

      public ProtonTestConnector(in Action<Stream> frameSink, in ILoggerFactory loggerFactory = null)
      {
         this.driver = new AMQPTestDriver(PeerName, ProcessDriverOutput, null, loggerFactory);

         this.frameSink = frameSink;
      }

      public void ConnectorFrameSink(Action<Stream> frameSink)
      {
         this.frameSink = frameSink;
      }

      public override AMQPTestDriver Driver => driver;

      public void Ingest(Stream frameBytes)
      {
         if (closed)
         {
            throw new InvalidOperationException("Closed driver is not accepting any new input", new IOException());
         }
         else
         {
            driver.Ingest(frameBytes);
         }
      }

      protected override string PeerName => "InMemoryConnector";

      protected override void ProcessCloseRequest()
      {
         // nothing to do in this peer implementation.
      }

      protected override void ProcessConnectionEstablished()
      {
         driver.HandleConnectedEstablished();
      }

      protected override void ProcessDriverOutput(Stream output)
      {
         if (frameSink == null)
         {
            throw new InvalidOperationException("Connector was not properly configured with a frame sink");
         }

         frameSink.Invoke(output);
      }
   }
}