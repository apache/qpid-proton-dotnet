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
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Tracks all sessions opened by the remote or initiated from the driver.
   /// </summary>
   public sealed class DriverSessions
   {
      public static readonly int DRIVER_DEFAULT_CHANNEL_MAX = 65535;

      private readonly IDictionary<ushort, SessionTracker> localSessions = new Dictionary<ushort, SessionTracker>();
      private readonly IDictionary<ushort, SessionTracker> remoteSessions = new Dictionary<ushort, SessionTracker>();

      private readonly AMQPTestDriver driver;

      private ushort? lastRemotelyOpenedSession = null;
      private ushort? lastLocallyOpenedSession = null;
      private LinkTracker lastCoordinator;

      public DriverSessions(AMQPTestDriver driver)
      {
         this.driver = driver;
      }

      public SessionTracker LastRemotelyOpenedSession =>
         lastRemotelyOpenedSession != null ? localSessions[(ushort)lastRemotelyOpenedSession] : null;

      public SessionTracker LastLocallyOpenedSession =>
         lastLocallyOpenedSession != null ? localSessions[(ushort)lastLocallyOpenedSession] : null;

      public LinkTracker LastOpenedCoordinator
      {
         get => lastCoordinator;
         set => lastCoordinator = value;
      }

      public AMQPTestDriver Driver => driver;

      public SessionTracker SessionFromLocalChannel(ushort localChannel) => localSessions[localChannel];

      public SessionTracker SessionFromRemoteChannel(ushort remoteChannel) => remoteSessions[remoteChannel];

      internal ushort FindFreeLocalChannel()
      {
         // TODO: Respect local channel max if one was set on open.
         for (ushort i = 0; i <= DRIVER_DEFAULT_CHANNEL_MAX; ++i)
         {
            if (!localSessions.ContainsKey(i))
            {
               return i;
            }
         }

         throw new InvalidOperationException("no local channel available for allocation");
      }

      internal void FreeLocalChannel(ushort localChannel)
      {
         localSessions.Remove(localChannel);
      }

      #region Handlers for AMQP Performatives

      public SessionTracker HandleBegin(Begin remoteBegin, ushort remoteChannel)
      {
         if (remoteSessions.ContainsKey(remoteChannel))
         {
            throw new AssertionError("Received duplicate Begin for already opened session on channel: " + remoteChannel);
         }

         ushort localChannelMax = driver.LocalOpen?.ChannelMax ?? ushort.MaxValue;

         if (remoteChannel > localChannelMax)
         {
            throw new AssertionError("Channel Max [" + localChannelMax + "] Exceeded for session Begin: " + remoteChannel);
         }

         SessionTracker sessionTracker;  // Result that we need to update here once validation is complete.

         if (remoteBegin.RemoteChannel != null)
         {
            // This should be a response to previous Begin that this test driver sent if there
            // is a remote channel set in which case a local session should already have been
            // created and if not that is an error
            if (!localSessions.TryGetValue((ushort)remoteBegin.RemoteChannel, out sessionTracker))
            {
               throw new AssertionError(string.Format(
                   "Received Begin on channel [{0}] that indicated it was a response to a Begin this driver never sent to channel [{1}]: ",
                   remoteChannel, remoteBegin.RemoteChannel));
            }
         }
         else
         {
            // Remote has requested that the driver create a new session which will require a scripted
            // response in order to complete the begin cycle.  Start tracking now for future
            sessionTracker = new SessionTracker(driver);

            localSessions.Add((ushort)sessionTracker.LocalChannel, sessionTracker);
         }

         sessionTracker.HandleBegin(remoteBegin, remoteChannel);

         remoteSessions.Add(remoteChannel, sessionTracker);
         lastRemotelyOpenedSession = sessionTracker.LocalChannel;

         return sessionTracker;
      }

      public SessionTracker HandleEnd(End remoteEnd, ushort remoteChannel)
      {
         SessionTracker sessionTracker = null;

         if (!remoteSessions.TryGetValue(remoteChannel, out sessionTracker))
         {
            throw new AssertionError(string.Format(
                "Received End on channel [{0}] that has no matching Session for that remote channel. ", remoteChannel));
         }
         else
         {
            sessionTracker.HandleEnd(remoteEnd);
            remoteSessions.Remove(remoteChannel);

            return sessionTracker;
         }
      }

      //----- Process Session Begin and End from their injection actions and update state

      public SessionTracker HandleLocalBegin(Begin localBegin, ushort localChannel)
      {
         // Are we responding to a remote Begin?  If so then we already have a SessionTracker
         // that should be correlated with the local tracker stored now that we are responding
         // to, although a test might be fiddling with unexpected Begin commands so we don't
         // assume there absolutely must be a remote session in the tracking map.
         if (localBegin.RemoteChannel != null && remoteSessions.ContainsKey((ushort)localBegin.RemoteChannel))
         {
            localSessions.Add(localChannel, remoteSessions[(ushort)localBegin.RemoteChannel]);
         }

         if (!localSessions.ContainsKey(localChannel))
         {
            localSessions.Add(localChannel, new SessionTracker(driver));
         }

         lastLocallyOpenedSession = localChannel;

         return localSessions[localChannel].HandleLocalBegin(localBegin, localChannel);
      }

      public SessionTracker HandleLocalEnd(End localEnd, ushort localChannel)
      {
         // A test script might trigger multiple end calls or otherwise mess with normal
         // AMQP processing no in case we can't find it, just return a dummy that the
         // script can use.
         if (localSessions.ContainsKey(localChannel))
         {
            return localSessions[localChannel].HandleLocalEnd(localEnd);
         }
         else
         {
            return new SessionTracker(driver).HandleLocalEnd(localEnd);
         }
      }

      #endregion
   }
}