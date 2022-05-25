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

using System.Threading;
using NUnit.Framework;
using System;
using System.Diagnostics;

namespace Apache.Qpid.Proton.Client.TestSupport
{
   public static class Wait
   {
      public static readonly int MAX_WAIT_MILLISECONDS = 30 * 1000;
      public static readonly int SLEEP_MILLISECONDS = 200;
      public static readonly string DEFAULT_FAILURE_MESSAGE = "Expected condition was not met";

      public static void AssertTrue(Func<bool> condition)
      {
         AssertTrue(DEFAULT_FAILURE_MESSAGE, condition);
      }

      public static void AssertFalse(Func<bool> condition)
      {
         AssertTrue(() => !condition());
      }

      public static void AssertFalse(String failureMessage, Func<bool> condition)
      {
         AssertTrue(failureMessage, () => !condition());
      }

      public static void AssertFalse(String failureMessage, Func<bool> condition, int duration)
      {
         AssertTrue(failureMessage, () => !condition(), duration, SLEEP_MILLISECONDS);
      }

      public static void AssertFalse(Func<bool> condition, int duration, int sleep)
      {
         AssertTrue(DEFAULT_FAILURE_MESSAGE, () => !condition(), duration, sleep);
      }

      public static void AssertTrue(Func<bool> condition, int duration)
      {
         AssertTrue(DEFAULT_FAILURE_MESSAGE, condition, duration, SLEEP_MILLISECONDS);
      }

      public static void AssertTrue(String failureMessage, Func<bool> condition)
      {
         AssertTrue(failureMessage, condition, MAX_WAIT_MILLISECONDS);
      }

      public static void AssertTrue(String failureMessage, Func<bool> condition, int duration)
      {
         AssertTrue(failureMessage, condition, duration, SLEEP_MILLISECONDS);
      }

      public static void AssertTrue(Func<bool> condition, int duration, int sleep)
      {
         AssertTrue(DEFAULT_FAILURE_MESSAGE, condition, duration, sleep);
      }

      public static void AssertTrue(String failureMessage, Func<bool> condition, int duration, int sleep)
      {
         bool result = WaitFor(condition, duration, sleep);

         if (!result)
         {
            Assert.Fail(failureMessage);
         }
      }

      public static bool WaitFor(Func<bool> condition)
      {
         return WaitFor(condition, MAX_WAIT_MILLISECONDS);
      }

      public static bool WaitFor(Func<bool> condition, int duration)
      {
         return WaitFor(condition, duration, SLEEP_MILLISECONDS);
      }

      public static bool WaitFor(Func<bool> condition, long durationMilliseconds, int sleepMilliseconds)
      {
         try
         {
            Stopwatch watch = Stopwatch.StartNew();
            bool conditionSatisfied = condition();

            while (!conditionSatisfied && watch.ElapsedMilliseconds < durationMilliseconds)
            {
               if (sleepMilliseconds == 0)
               {
                  Thread.Yield();
               }
               else
               {
                  Thread.Sleep(sleepMilliseconds);
               }

               conditionSatisfied = condition();
            }

            return conditionSatisfied;
         }
         catch (Exception e)
         {
            throw new InvalidOperationException("Wait for condition failed", e);
         }
      }
   }
}