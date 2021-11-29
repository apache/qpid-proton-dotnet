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
using System.Threading;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Concurrent.Default
{
   [TestFixture, Timeout(20000)]
   public class DefaultEventLoopTest
   {
      [Test]
      public void TestCreateAndShutdown()
      {
         IEventLoop loop = new DefaultEventLoop();

         Assert.IsFalse(loop.IsShutdown);
         Assert.IsFalse(loop.IsTerminated);
         Assert.IsFalse(loop.InEventLoop);

         loop.Shutdown();

         Assert.IsTrue(loop.WaitForTermination(System.TimeSpan.FromHours(1)));
         Assert.IsTrue(loop.IsShutdown);
         Assert.IsTrue(loop.IsTerminated);
      }

      [Test]
      public void TestExecuteSomeAction()
      {
         IEventLoop loop = new DefaultEventLoop();

         CountdownEvent waitEvent = new CountdownEvent(1);

         loop.Execute(() => waitEvent.Signal());

         Assert.IsTrue(waitEvent.Wait(TimeSpan.FromHours(1)));
      }

      [Test]
      public void TestExecuteManyActions()
      {
         IEventLoop loop = new DefaultEventLoop();

         CountdownEvent waitEvent = new CountdownEvent(10);

         for (int i = 0; i < 9; ++i)
         {
            loop.Execute(() => waitEvent.Signal());
         }

         Assert.AreNotEqual(0, waitEvent.CurrentCount);
         Assert.IsFalse(waitEvent.Wait(TimeSpan.FromMilliseconds(1)));

         loop.Execute(() => waitEvent.Signal());

         Assert.IsTrue(waitEvent.Wait(TimeSpan.FromHours(1)));
      }
   }
}