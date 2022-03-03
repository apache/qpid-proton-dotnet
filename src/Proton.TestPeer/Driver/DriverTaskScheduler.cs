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
using System.Collections.Generic;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Used to schedule delayed actions from the test driver and ensure that they
   /// execute in a timely but ordered fashion
   ///
   /// This implementation intends to ensure that elements that are scheduled to execute
   /// at or about the same time do so in the order submitted.  This implementation is
   /// not intended to be performant or memory efficient as the test driver should not
   /// be used to schedule large numbers of tasks for any given test script.
   /// </summary>
   public sealed class DriverTaskScheduler
   {
      private static readonly int DEFAULT_TIMER_INTERVAL = 10;

      private readonly SortedDictionary<long, Queue<ScriptedAction>> eventHeap =
         new SortedDictionary<long, Queue<ScriptedAction>>();

      private readonly AMQPTestDriver driver;
      private readonly Timer timer = new Timer();

      private readonly ILogger<DriverTaskScheduler> logger;

      public DriverTaskScheduler(AMQPTestDriver driver)
      {
         this.driver = driver;

         // Timer starts immediately
         this.timer.Elapsed += RunPendingTasks;
         this.timer.Interval = DEFAULT_TIMER_INTERVAL;
         this.timer.AutoReset = false;
         this.timer.Enabled = true;

         this.logger = driver.LoggerFactory.CreateLogger<DriverTaskScheduler>();
      }

      public void Schedule(ScriptedAction action, long delay)
      {
         long timeNow = Environment.TickCount64;
         long timeToRun = timeNow + delay;

         Queue<ScriptedAction> timeToRunQueue = eventHeap.GetValueOrDefault(timeToRun);
         if (timeToRunQueue == null)
         {
            eventHeap[timeToRun] = timeToRunQueue = new Queue<ScriptedAction>();
         }

         timeToRunQueue.Enqueue(action);
      }

      private void RunPendingTasks(object sender, ElapsedEventArgs args)
      {
         long timeNow = Environment.TickCount64;
         List<long> toRemove = new List<long>();

         foreach (KeyValuePair<long, Queue<ScriptedAction>> entry in eventHeap)
         {
            if (timeNow >= entry.Key)
            {
               toRemove.Add(entry.Key);
            }
         }

         foreach (long timeSlot in toRemove)
         {
            Queue<ScriptedAction> events = eventHeap[timeSlot];

            foreach (ScriptedAction action in events)
            {
               try
               {
                  logger.LogTrace("{} running delayed action: {}", driver.Name, action);
                  action.Perform(driver);
               }
               catch (Exception)
               {
                  eventHeap.Clear();
                  return;
               }
            }

            eventHeap.Remove(timeSlot);
         }

         timer.Start();
      }
   }
}