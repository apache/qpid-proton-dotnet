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

using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Driver.Test;
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture, Timeout(20000)]
   public class frameHandlerTests
   {
      [Test]
      public void TestScheduleJobWithZeroDelay()
      {
         List<Stream> actions = new List<Stream>();
         AMQPTestDriver driver = new AMQPTestDriver("test", (s) => actions.Add(s));
         DriverTaskScheduler scheduler = new DriverTaskScheduler(driver);

         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 0);

         Wait.AssertTrue(() => actions.Count == 1);
      }

      [Test]
      public void TestScheduleJobWithSmallDelay()
      {
         List<Stream> actions = new List<Stream>();
         AMQPTestDriver driver = new AMQPTestDriver("test", (s) => actions.Add(s));
         DriverTaskScheduler scheduler = new DriverTaskScheduler(driver);

         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 20);

         Wait.AssertTrue(() => actions.Count == 1);
      }

      [Test]
      public void TestScheduleJobWithLargeDelay()
      {
         List<Stream> actions = new List<Stream>();
         AMQPTestDriver driver = new AMQPTestDriver("test", (s) => actions.Add(s));
         DriverTaskScheduler scheduler = new DriverTaskScheduler(driver);

         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 1000);

         Wait.AssertTrue(() => actions.Count == 1);
      }

      [Test]
      public void TestScheduleMultipleJobs()
      {
         List<Stream> actions = new List<Stream>();
         AMQPTestDriver driver = new AMQPTestDriver("test", (s) => actions.Add(s));
         DriverTaskScheduler scheduler = new DriverTaskScheduler(driver);

         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 200);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 200);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 200);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 200);

         Wait.AssertTrue(() => actions.Count == 4);
      }

      [Test]
      public void TestScheduleMultipleJobsWithDifferentDelays()
      {
         List<Stream> actions = new List<Stream>();
         AMQPTestDriver driver = new AMQPTestDriver("test", (s) => actions.Add(s));
         DriverTaskScheduler scheduler = new DriverTaskScheduler(driver);

         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 100);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 250);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 300);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 500);

         Wait.AssertTrue(() => actions.Count == 4);
      }

      [Test]
      public void TestScheduleMultipleJobsWithHugeDelayNeverExecute()
      {
         List<Stream> actions = new List<Stream>();
         AMQPTestDriver driver = new AMQPTestDriver("test", (s) => actions.Add(s));
         DriverTaskScheduler scheduler = new DriverTaskScheduler(driver);

         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 50_000);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 50_000);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 50_000);
         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 50_000);

         scheduler.Schedule(new AMQPHeaderInjectAction(driver, AMQPHeader.Header), 2000);

         Wait.AssertTrue("Should only execute one event", () => actions.Count == 1, 10000);
      }
   }
}