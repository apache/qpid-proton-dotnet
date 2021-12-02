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
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Utilities;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Abstract base class that is implemented by all the AMQP v1.0 test peer
   /// implementations to provide a consistent interface for the test driver
   /// classes to interact with.
   /// </summary>
   public abstract class ProtonTestPeer : ScriptWriter, IDisposable
   {
      protected readonly AtomicBoolean closed = new AtomicBoolean();

      public void Dispose()
      {
         if (closed.CompareAndSet(false, true))
         {
            ProcessCloseRequest();
         }
      }

      public void WaitForScriptToCompleteIgnoreErrors()
      {
         Driver.WaitForScriptToCompleteIgnoreErrors();
      }

      public void WaitForScriptToComplete()
      {
         Driver.WaitForScriptToComplete();
      }

      public void WaitForScriptToComplete(long timeout)
      {
         Driver.WaitForScriptToComplete(timeout);
      }

      public void WaitForScriptToComplete(TimeSpan timeout)
      {
         Driver.WaitForScriptToComplete(timeout);
      }

      public uint EmptyFrameCount => Driver.EmptyFrameCount;

      public uint PerformativeCount => Driver.PerformativeCount;

      public uint SaslPerformativeCount => Driver.SaslPerformativeCount;

      /// <summary>
      /// Drops the connection to the connected client immediately after the last handler that was
      /// registered before this scripted action is queued.  Adding any additional test scripting to
      /// the test driver will either not be acted on or could cause the wait methods to not return
      /// as they will never be invoked.
      /// </summary>
      /// <returns></returns>
      public ProtonTestPeer DropAfterLastHandler()
      {
         Driver.AddScriptedElement(new ConnectionDropAction(this));
         return this;
      }

      /// <summary>
      /// Drops the connection to the connected client immediately after the last handler that was
      /// registered before this scripted action is queued.  Adding any additional test scripting to
      /// the test driver will either not be acted on or could cause the wait methods to not return
      /// as they will never be invoked.
      /// </summary>
      /// <param name="delay"></param>
      /// <returns></returns>
      public ProtonTestPeer DropAfterLastHandler(int delay)
      {
         Driver.AddScriptedElement(new ConnectionDropAction(this).AfterDelay(delay));
         return this;
      }

      protected abstract String PeerName { get; }

      protected abstract void ProcessCloseRequest();

      protected abstract void ProcessDriverOutput(Stream output);

      protected abstract void ProcessConnectionEstablished();

      protected void CheckClosed()
      {
         if (closed)
         {
            throw new InvalidOperationException("The test peer is closed");
         }
      }
   }
}