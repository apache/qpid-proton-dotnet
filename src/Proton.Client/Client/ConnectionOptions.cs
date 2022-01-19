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
   public class ConnectionOptions : ICloneable
   {
      private static readonly string[] DEFAULT_DESIRED_CAPABILITIES_ARRAY = new string[] { "ANONYMOUS-RELAY" };

      public static readonly long INFINITE = -1;
      public static readonly long DEFAULT_OPEN_TIMEOUT = 15000;
      public static readonly long DEFAULT_CLOSE_TIMEOUT = 60000;
      public static readonly long DEFAULT_SEND_TIMEOUT = INFINITE;
      public static readonly long DEFAULT_REQUEST_TIMEOUT = INFINITE;
      public static readonly long DEFAULT_IDLE_TIMEOUT = 60000;
      public static readonly long DEFAULT_DRAIN_TIMEOUT = 60000;
      public static readonly ushort DEFAULT_CHANNEL_MAX = 65535;
      public static readonly uint DEFAULT_MAX_FRAME_SIZE = 65536;

      /// <summary>
      /// Creates a default Connection options instance.
      /// </summary>
      public ConnectionOptions() : base()
      {
      }

      /// <summary>
      /// Create a new Connection options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The connection options instance to copy</param>
      public ConnectionOptions(ConnectionOptions other) : base()
      {
         other?.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new ConnectionOptions());
      }

      internal ConnectionOptions CopyInto(ConnectionOptions other)
      {
         other.User = User;
         other.Password = Password;
         other.VirtualHost = VirtualHost;
         other.SendTimeout = SendTimeout;
         other.RequestTimeout = RequestTimeout;
         other.OpenTimeout = OpenTimeout;
         other.CloseTimeout = CloseTimeout;
         other.IdleTimeout = IdleTimeout;
         other.DrainTimeout = DrainTimeout;
         other.ChannelMax = ChannelMax;
         other.MaxFrameSize = MaxFrameSize;
         other.TraceFrames = TraceFrames;
         if (OfferedCapabilities != null && OfferedCapabilities.Length > 0)
         {
            string[] copyOf = new string[OfferedCapabilities.Length];
            Array.Copy(OfferedCapabilities, copyOf, OfferedCapabilities.Length);
         }
         if (DesiredCapabilities != null && DesiredCapabilities.Length > 0)
         {
            string[] copyOf = new string[DesiredCapabilities.Length];
            Array.Copy(DesiredCapabilities, copyOf, DesiredCapabilities.Length);
         }
         if (Properties != null)
         {
            other.Properties = new Dictionary<string, object>(Properties);
         }

         ReconnectOptions.CopyInto(other.ReconnectOptions);
         TransportOptions.CopyInto(other.TransportOptions);
         SslOptions.CopyInto(other.SslOptions);
         SaslOptions.CopyInto(other.SaslOptions);

         // Copy event sets as currently set and any changes to the source will
         // not be reflected in the target as the lists are copy on write.
         other.ConnectedHandler = ConnectedHandler;
         other.DisconnectedHandler = DisconnectedHandler;
         other.InterruptedHandler = InterruptedHandler;
         other.ReconnectedHandler = ReconnectedHandler;

         return other;
      }

      /// <summary>
      /// Configure the user name that is conveyed to the remote peer when a new connection
      /// is being opened.
      /// </summary>
      public string User { get; set; }

      /// <summary>
      /// Configure the password that is conveyed to the remote peer when a new connection
      /// is being opened.
      /// </summary>
      public string Password { get; set; }

      /// <summary>
      /// Configure the virtual host value that is conveyed to the remote peer when a new
      /// connection is being opened.
      /// </summary>
      public string VirtualHost { get; set; }

      /// <summary>
      /// Gets or sets the connection level send timeout value which will be used as the
      /// defaults if child resources are not created with their own options type. This
      /// timeout controls how long a sender will wait for a send to complete before giving
      /// up and signalling a send failure.
      /// </summary>
      public long SendTimeout { get; set; } = DEFAULT_SEND_TIMEOUT;

      /// <summary>
      /// Gets or sets the connection level request timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  The request
      /// timeout applies to client specific actions that require a remote response such as the
      /// begin or end of a transaction.
      /// </summary>
      public long RequestTimeout { get; set; } = DEFAULT_REQUEST_TIMEOUT;

      /// <summary>
      /// Gets or sets the connection level open timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  Controls
      /// how long a client will wait for a remote to respond to the open of a resource before
      /// signalling that the open has failed.
      /// </summary>
      public long OpenTimeout { get; set; } = DEFAULT_OPEN_TIMEOUT;

      /// <summary>
      /// Gets or sets the connection level close timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  Controls
      /// how long the remote will wait for the remote to respond to a close request for a
      /// resource before giving up and signalling an error has occurred.
      /// </summary>
      public long CloseTimeout { get; set; } = DEFAULT_CLOSE_TIMEOUT;

      /// <summary>
      /// Gets or sets the connection level idle timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  Controls
      /// the idle processing timeout that is sent to the remote informing it up the clients
      /// expectation for how long it can remain idle before needing to send a heart beat.
      /// </summary>
      public long IdleTimeout { get; set; } = DEFAULT_IDLE_TIMEOUT;

      /// <summary>
      /// Gets or sets the connection level drain timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  Controls
      /// how long the drain of a receiver link and remain outstanding before it is considered
      /// failed and an error is signalled indicating that the drain attempt failed.
      /// </summary>
      public long DrainTimeout { get; set; } = DEFAULT_DRAIN_TIMEOUT;

      /// <summary>
      /// Configure the channel maximum value for the new connection created with these options.
      /// The channel max value controls how many session instances can be created by a given
      /// Connection, the default value is <c>65535</c>.
      /// </summary>
      public ushort ChannelMax { get; set; } = DEFAULT_CHANNEL_MAX;

      /// <summary>
      /// Configures the max frame size that will be communicated to the remote peer instructing it
      /// that this client cannot receive frames larger than this value.  This also controls the
      /// size of the frames this peer will output unless the remote has requested a smaller value
      /// than what is set here.
      /// </summary>
      public uint MaxFrameSize { get; set; } = DEFAULT_MAX_FRAME_SIZE;

      /// <summary>
      /// Configures the set of capabilities that a new connection will advertise to the remote.
      /// </summary>
      public string[] OfferedCapabilities { get; set; }

      /// <summary>
      /// Sets the collection of capabilities to request from the remote for a new connection.
      /// The desired capabilities inform the remote peer of the various capabilities the connection
      /// requires and the remote should return those that it supports in its offered capabilities.
      /// </summary>
      public string[] DesiredCapabilities { get; set; } = DEFAULT_DESIRED_CAPABILITIES_ARRAY;

      /// <summary>
      /// Configures a collection of property values that are sent to the remote upon opening
      /// a new connection.
      /// </summary>
      public IDictionary<string, object> Properties { get; set; }

      /// <summary>
      /// Gets the SASL options instance associated with these connection options.
      /// </summary>
      public SaslOptions SaslOptions { get; } = new SaslOptions();

      /// <summary>
      /// Quick access to enable and disable reconnection for newly created connections that
      /// use these options.
      /// </summary>
      public bool ReconnectEnabled
      {
         get { return ReconnectOptions.ReconnectEnabled; }
         set { ReconnectOptions.ReconnectEnabled = value; }
      }

      /// <summary>
      /// Gets the Reconnection options that control client reconnection behavior.
      /// </summary>
      public ReconnectOptions ReconnectOptions { get; } = new ReconnectOptions();

      /// <summary>
      /// Controls if the client will attempt to trigger the AMQP engine to trace
      /// all incoming and outgoing frames via the logger.
      /// </summary>
      public bool TraceFrames { get; set; }

      /// <summary>
      /// Configuration of the I/O layer options.
      /// </summary>
      public TransportOptions TransportOptions { get; } = new TransportOptions();

      /// <summary>
      /// Quick access to enable and disable SSL for newly created connections that
      /// use these options.
      /// </summary>
      public bool SslEnabled
      {
         get { return SslOptions.SslEnabled; }
         set { SslOptions.SslEnabled = value; }
      }

      /// <summary>
      /// Configuration that controls the SSL I/O layer if enabled.
      /// </summary>
      public SslOptions SslOptions { get; } = new SslOptions();

      /// <summary>
      /// Register and action that will be fired asynchronously to signal that
      /// the client has connected to a remote peer. It is a programming error
      /// for the signaled handler to throw an exception and the outcome of such
      /// an error is unspecified.
      /// </summary>
      public Action<IConnection, ConnectionEvent> ConnectedHandler { get; set; }

      /// <summary>
      /// Register and action that will be fired asynchronously to signal that
      /// the client has reconnected to a remote peer. It is a programming error
      /// for the signaled handler to throw an exception and the outcome of such
      /// an error is unspecified.
      /// </summary>
      public Action<IConnection, ConnectionEvent> ReconnectedHandler { get; set; }

      /// <summary>
      /// Register and action that will be fired asynchronously to signal that
      /// the client has been disconnected from a remote peer but will attempt to
      /// reconnect using configured reconnection options. It is a programming
      /// error for the signaled handler to throw an exception and the outcome of
      /// such an error is unspecified.
      /// </summary>
      public Action<IConnection, DisconnectionEvent> InterruptedHandler { get; set; }

      /// <summary>
      /// Register and action that will be fired asynchronously to signal that
      /// the client has been disconnected from a remote peer. It is a programming
      /// error for the signaled handler to throw an exception and the outcome of
      /// such an error is unspecified.
      /// </summary>
      public Action<IConnection, DisconnectionEvent> DisconnectedHandler { get; set; }

   }
}