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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Reconnection options which will control how a connection deals will connection loss
   /// and or inability to connect to the host it was provided at create time.
   /// </summary>
   public class ReconnectOptions : ICloneable
   {
      public static readonly bool DEFAULT_RECONNECT_ENABLED = false;
      public static readonly int INFINITE = -1;
      public static readonly int DEFAULT_WARN_AFTER_RECONNECT_ATTEMPTS = 10;
      public static readonly int DEFAULT_RECONNECT_DELAY = 10;
      public static readonly int DEFAULT_MAX_RECONNECT_DELAY = 30_000;
      public static readonly bool DEFAULT_USE_RECONNECT_BACKOFF = true;
      public static readonly double DEFAULT_RECONNECT_BACKOFF_MULTIPLIER = 2.0d;

      private readonly List<ReconnectLocation> reconnectLocations = new();

      /// <summary>
      /// Creates a default reconnect options instance.
      /// </summary>
      public ReconnectOptions() : base()
      {
      }

      /// <summary>
      /// Create a new reconnection options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The reconnect options instance to copy</param>
      public ReconnectOptions(ReconnectOptions other) : this()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public virtual object Clone()
      {
         return CopyInto(new ReconnectOptions());
      }

      internal ReconnectOptions CopyInto(ReconnectOptions other)
      {
         other.ReconnectLocations = ReconnectLocations;
         other.ReconnectEnabled = ReconnectEnabled;
         other.WarnAfterReconnectAttempts = WarnAfterReconnectAttempts;
         other.MaxInitialConnectionAttempts = MaxInitialConnectionAttempts;
         other.MaxReconnectAttempts = MaxReconnectAttempts;
         other.ReconnectDelay = ReconnectDelay;
         other.MaxReconnectDelay = MaxReconnectDelay;
         other.UseReconnectBackOff = UseReconnectBackOff;
         other.ReconnectBackOffMultiplier = ReconnectBackOffMultiplier;

         return other;
      }

      /// <summary>
      /// Configure if a connection will attempt reconnection if connection is lost or cannot
      /// be established to the primary host provided at create time.
      /// </summary>
      public bool ReconnectEnabled { get; set; }

      /// <summary>
      /// Adds the given host and port to the collection of locations where a connection
      /// reconnect attempt can be made.
      /// </summary>
      /// <param name="host">The host where the connection is made</param>
      /// <param name="port">The port on the host where the connection is made</param>
      /// <returns>This reconnection options instance.</returns>
      public ReconnectOptions AddReconnectLocation(string host, int port)
      {
         this.reconnectLocations.Add(new ReconnectLocation(host, port));
         return this;
      }

      /// <summary>
      /// Provides access to get or set the list of reconnection locations that are
      /// available from these options.  The returned collection is read-only amd the
      /// provided collection is copied in this options own collection.
      /// </summary>
      public IReadOnlyCollection<ReconnectLocation> ReconnectLocations
      {
         get { return reconnectLocations.AsReadOnly(); }
         set
         {
            this.reconnectLocations.Clear();
            this.reconnectLocations.AddRange(value);
         }
      }

      /// <summary>
      /// Controls how often the client will log a message indicating that a reconnection is
      /// being attempted.  The default is to log every 10 connection attempts.
      /// </summary>
      public int WarnAfterReconnectAttempts { get; set; } = DEFAULT_WARN_AFTER_RECONNECT_ATTEMPTS;

      /// <summary>
      /// For a client that has never connected to a remote peer before this option controls
      /// how many attempts are made to connect before reporting the connection as failed.
      /// The default behavior is to use the value of maxReconnectAttempts.
      /// </summary>
      public int MaxInitialConnectionAttempts { get; set; } = INFINITE;

      /// <summary>
      /// The number of reconnection attempts allowed before reporting the connection as failed
      /// to the client.  The default is no limit or (-1).
      /// </summary>
      public int MaxReconnectAttempts { get; set; } = INFINITE;

      /// <summary>
      /// Controls the delay between successive reconnection attempts, defaults to 10 milliseconds.
      /// If the back off option is not enabled this value remains constant.
      /// </summary>
      public int ReconnectDelay { get; set; } = DEFAULT_RECONNECT_DELAY;

      /// <summary>
      /// The maximum time that the client will wait before attempting a reconnect. This value is
      /// only used when the back off feature is enabled to ensure that the delay does not grow too
      /// large. Defaults to 30 seconds as the max time between successive connection attempts.
      /// </summary>
      public int MaxReconnectDelay { get; set; } = DEFAULT_MAX_RECONNECT_DELAY;

      /// <summary>
      /// Controls whether the time between reconnection attempts should grow based on a configured
      /// multiplier. This option defaults to true.
      /// </summary>
      public bool UseReconnectBackOff { get; set; } = DEFAULT_USE_RECONNECT_BACKOFF;

      /// <summary>
      /// The multiplier used to grow the reconnection delay value, defaults to 2.0d.
      /// </summary>
      public double ReconnectBackOffMultiplier { get; set; } = DEFAULT_RECONNECT_BACKOFF_MULTIPLIER;

   }
}