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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Util
{
   public class FifoMessageQueueTests
   {
      [Test, Timeout(20000)]
      public void TestCreate()
      {
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();

         Assert.False(queue.IsClosed);
         Assert.True(queue.IsEmpty);
         Assert.False(queue.IsRunning);

         Assert.AreEqual(0, queue.Count);
      }

      [Test, Timeout(20000)]
      public void TestClose()
      {
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();

         queue.Start();
         Assert.False(queue.IsClosed);
         Assert.True(queue.IsRunning);
         queue.Close();
         Assert.True(queue.IsClosed);
         Assert.False(queue.IsRunning);
         queue.Close();
      }

      [Test, Timeout(20000)]
      public void TestCloseQueueDoesNotAccumulateValues()
      {
         const string message1 = "test1";
         const string message2 = "test2";
         const string message3 = "test3";

         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();

         queue.Start();
         queue.EnqueueFront(message1);
         Assert.AreEqual(1, queue.Count);
         Assert.False(queue.IsClosed);
         Assert.True(queue.IsRunning);

         queue.Close();
         Assert.AreEqual(0, queue.Count);
         Assert.True(queue.IsEmpty);
         Assert.True(queue.IsClosed);
         Assert.False(queue.IsRunning);

         queue.Enqueue(message2);
         queue.EnqueueFront(message3);

         Assert.AreEqual(0, queue.Count);
         Assert.True(queue.IsEmpty);
      }

      [Test, Timeout(20000)]
      public void TestDequeueNoWaitWhenQueueIsClosed()
      {
         const string message = "test";
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();
         queue.EnqueueFront(message);

         Assert.False(queue.IsEmpty);
         queue.Close();
         Assert.Null(queue.DequeueNoWait());
      }

      [Test, Timeout(20000)]
      public void TestDequeueWhenQueueIsClosed()
      {
         const string message = "test";
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();
         queue.EnqueueFront(message);

         Assert.False(queue.IsEmpty);
         queue.Close();
         Assert.Null(queue.Dequeue(TimeSpan.FromDays(1)));
      }

      [Test, Timeout(20000)]
      public void TestDequeueWhenQueueIsStopped()
      {
         const string message = "test";
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();
         queue.EnqueueFront(message);

         Assert.False(queue.IsEmpty);
         queue.Stop();
         Assert.False(queue.IsRunning);
         Assert.Null(queue.Dequeue(TimeSpan.FromDays(1)));
         queue.Start();
         Assert.True(queue.IsRunning);
         Assert.AreEqual(message, queue.Dequeue(TimeSpan.FromDays(1)));
      }

      [Test, Timeout(20000)]
      public void TestDequeueNoWaitWhenQueueIsStopped()
      {
         const string message = "test";
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();
         queue.EnqueueFront(message);

         Assert.False(queue.IsEmpty);
         queue.Stop();
         Assert.False(queue.IsRunning);
         Assert.Null(queue.DequeueNoWait());
         queue.Start();
         Assert.True(queue.IsRunning);
         Assert.AreEqual(message, queue.DequeueNoWait());
      }

      [Test, Timeout(20000)]
      public void TestEnqueueFirst()
      {
         const string message1 = "test1";
         const string message2 = "test2";
         const string message3 = "test3";

         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();

         queue.EnqueueFront(message1);
         queue.EnqueueFront(message2);
         queue.EnqueueFront(message3);

         Assert.AreSame(message3, queue.DequeueNoWait());
         Assert.AreSame(message2, queue.DequeueNoWait());
         Assert.AreSame(message1, queue.DequeueNoWait());
      }

      [Test, Timeout(20000)]
      public void TestClear()
      {
         const string message1 = "test1";
         const string message2 = "test2";
         const string message3 = "test3";

         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();
         queue.EnqueueFront(message1);
         queue.EnqueueFront(message2);
         queue.EnqueueFront(message3);

         Assert.False(queue.IsEmpty);
         queue.Clear();
         Assert.That(queue.IsEmpty);
      }

      [Test, Timeout(20000)]
      public void TestRemoveFirstOnEmptyQueue()
      {
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();

         Assert.Null(queue.DequeueNoWait());
      }

      [Test, Timeout(20000)]
      public void TestRemoveFirst()
      {
         const string message1 = "test1";
         const string message2 = "test2";
         const string message3 = "test3";

         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();
         queue.Enqueue(message1);
         queue.Enqueue(message2);
         queue.Enqueue(message3);

         Assert.AreSame(message1, queue.DequeueNoWait());
         Assert.AreSame(message2, queue.DequeueNoWait());
         Assert.AreSame(message3, queue.DequeueNoWait());

         Assert.True(queue.IsEmpty);
      }

      [Test, Timeout(20000)]
      public void TestDequeueWaitsUntilMessageArrives()
      {
         const string message = "test1";

         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();

         Task runner = Task.Run(() =>
            {
               Thread.Sleep(100);
               queue.EnqueueFront(message);
            }
         );

         Assert.AreSame(message, queue.Dequeue(TimeSpan.FromDays(1)));
      }

      [Test, Timeout(20000)]
      public void TestDequeueReturnsWhenQueueIsStopped()
      {
         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();

         Task runner = Task.Run(() =>
            {
               Thread.Sleep(100);
               queue.Stop();
            }
         );

         Assert.Null(queue.Dequeue(TimeSpan.FromDays(1)));
      }

      [Test, Timeout(20000)]
      public void TestRestartingClosedQueueHasNoEffect()
      {
         const string message = "test1";

         FifoDeliveryQueue<string> queue = new FifoDeliveryQueue<string>();
         queue.Start();
         queue.EnqueueFront(message);

         Assert.True(queue.IsRunning);
         Assert.False(queue.IsClosed);

         queue.Stop();

         Assert.False(queue.IsRunning);
         Assert.False(queue.IsClosed);
         Assert.Null(queue.Dequeue(TimeSpan.FromDays(1)));

         queue.Close();

         Assert.True(queue.IsClosed);
         Assert.False(queue.IsRunning);

         queue.Start();

         Assert.True(queue.IsClosed);
         Assert.False(queue.IsRunning);
         Assert.Null(queue.Dequeue(TimeSpan.FromDays(1)));
      }
   }
}