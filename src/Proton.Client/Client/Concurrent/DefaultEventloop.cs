/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client.Concurrent
{
   /// <summary>
   /// Default event loop implementation.
   /// </summary>
   public sealed class DefaultEventLoop : IEventLoop
   {
      private readonly Channel<Action> eventChannel;
      private readonly ChannelReader<Action> eventReader;
      private readonly ChannelWriter<Action> eventWriter;

      private readonly Task eventLoop;
      private AtomicBoolean shutdown = new AtomicBoolean();
      private CountdownEvent hasShutdown = new CountdownEvent(1);

      private AtomicReference<Thread> eventLoopThread = new AtomicReference<Thread>();

      public DefaultEventLoop()
      {
         eventChannel = Channel.CreateUnbounded<Action>(
            new UnboundedChannelOptions
            {
               AllowSynchronousContinuations = false,
               SingleReader = true,
               SingleWriter = false
            });

         eventReader = eventChannel.Reader;
         eventWriter = eventChannel.Writer;

         // Long running event loop task which will process the submitted Actions
         // for this IEventLoop implementation.
         eventLoop = Task.Factory.StartNew(EventLoop, TaskCreationOptions.LongRunning);
      }

      public bool InEventLoop => Thread.CurrentThread.Equals(eventLoopThread.Get());

      public bool IsShutdown => shutdown;

      public bool IsTerminated => hasShutdown.CurrentCount == 0;

      public void Shutdown()
      {
         if (shutdown.CompareAndSet(false, true))
         {
            eventWriter.Complete();
         }
      }

      public bool WaitForTermination(TimeSpan waitTime)
      {
         return hasShutdown.Wait(waitTime);
      }

      public void Execute(Action action)
      {
         if (shutdown)
         {
            throw new RejectedExecutionException("Could not submit to action as the execution has been shut down");
         }

         if (!eventWriter.TryWrite(action))
         {
            throw new RejectedExecutionException("Failed to submit action for execution");
         }
      }

      private async void EventLoop()
      {
         eventLoopThread.Set(Thread.CurrentThread);

         try
         {
            while (await eventReader.WaitToReadAsync().ConfigureAwait(false) && !shutdown)
            {
               while (eventReader.TryRead(out Action loopAction) && !shutdown)
               {
                  TriggerLoopEvent(loopAction);
               }
            }
         }
         finally
         {
            hasShutdown.Signal();
         }
      }

      private void TriggerLoopEvent(Action loopAction)
      {
         try
         {
            loopAction();
         }
         catch (Exception)
         {
            // TODO Fire uncaught exception handler
         }
      }
   }
}