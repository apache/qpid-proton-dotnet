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
   public class SessionOptions : ICloneable
   {
      /// <summary>
      /// The default Session configured incoming capacity limit to provide to the remote.
      /// </summary>
      public static readonly uint DEFAULT_SESSION_INCOMING_CAPACITY = 100 * 1024 * 1024;

      /// <summary>
      /// The default Session configured outgoing capacity limit to use to limit pending writes.
      /// </summary>
      public static readonly uint DEFAULT_SESSION_OUTGOING_CAPACITY = 100 * 1024 * 1024;

      /// <summary>
      /// Creates a default session options instance.
      /// </summary>
      public SessionOptions() : base()
      {
      }

      /// <summary>
      /// Create a new session options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The session options instance to copy</param>
      public SessionOptions(SessionOptions other) : base()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new SessionOptions());
      }

      protected SessionOptions CopyInto(SessionOptions other)
      {
         other.SendTimeout = SendTimeout;
         other.RequestTimeout = RequestTimeout;
         other.OpenTimeout = OpenTimeout;
         other.CloseTimeout = CloseTimeout;
         other.DrainTimeout = DrainTimeout;
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
         other.OutgoingCapacity = OutgoingCapacity;
         other.IncomingCapacity = IncomingCapacity;

         return other;
      }

      /// <summary>
      /// Gets or sets the session level send timeout value which will be used as the
      /// defaults if child resources are not created with their own options type. This
      /// timeout controls how long a sender will wait for a send to complete before giving
      /// up and signalling a send failure.
      /// </summary>
      public long SendTimeout { get; set; } = ConnectionOptions.DEFAULT_SEND_TIMEOUT;

      /// <summary>
      /// Gets or sets the session level request timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  The request
      /// timeout applies to client specific actions that require a remote response such as the
      /// begin or end of a transaction.
      /// </summary>
      public long RequestTimeout { get; set; } = ConnectionOptions.DEFAULT_REQUEST_TIMEOUT;

      /// <summary>
      /// Gets or sets the session level open timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  Controls
      /// how long a client will wait for a remote to respond to the open of a resource before
      /// signalling that the open has failed.
      /// </summary>
      public long OpenTimeout { get; set; } = ConnectionOptions.DEFAULT_OPEN_TIMEOUT;

      /// <summary>
      /// Gets or sets the session level close timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  Controls
      /// how long the remote will wait for the remote to respond to a close request for a
      /// resource before giving up and signalling an error has occurred.
      /// </summary>
      public long CloseTimeout { get; set; } = ConnectionOptions.DEFAULT_CLOSE_TIMEOUT;

      /// <summary>
      /// Gets or sets the session level drain timeout value which will be used as the
      /// defaults if child resources are not created with their own options type.  Controls
      /// how long the drain of a receiver link and remain outstanding before it is considered
      /// failed and an error is signalled indicating that the drain attempt failed.
      /// </summary>
      public long DrainTimeout { get; set; } = ConnectionOptions.DEFAULT_DRAIN_TIMEOUT;

      /// <summary>
      /// Configures the set of capabilities that a new session will advertise to the remote.
      /// </summary>
      public string[] OfferedCapabilities { get; set; }

      /// <summary>
      /// Sets the collection of capabilities to request from the remote for a new session.
      /// The desired capabilities inform the remote peer of the various capabilities the session
      /// requires and the remote should return those that it supports in its offered capabilities.
      /// </summary>
      public string[] DesiredCapabilities { get; set; }

      /// <summary>
      /// Configures a collection of property values that are sent to the remote upon opening
      /// a new session.
      /// </summary>
      public IDictionary<string, object> Properties { get; set; }

      /// <summary>
      /// Configures the incoming capacity for each session created with these options.  The incoming
      /// capacity controls how much buffering a session will allow before applying back pressure to
      /// the remote thereby preventing excessive memory overhead.
      ///
      /// This is an advanced option and in most cases the client defaults should be left in place unless
      /// a specific issue needs to be addressed.
      /// </summary>
      public uint IncomingCapacity { get; set; } = DEFAULT_SESSION_INCOMING_CAPACITY;

      /// <summary>
      /// Configures the outgoing capacity for a session created with these options.  The outgoing
      /// capacity controls how much buffering a session will allow before applying back pressure to
      /// the local thereby preventing excessive memory overhead while writing large amounts of data
      /// and the client is experiencing back-pressure due to the remote not keeping pace.
      ///
      /// This is an advanced option and in most cases the client defaults should be left in place unless
      /// a specific issue needs to be addressed.  Setting this value incorrectly can lead to senders that
      /// either block frequently or experience very poor overall performance.
      /// </summary>
      public uint OutgoingCapacity { get; set; } = DEFAULT_SESSION_OUTGOING_CAPACITY;

   }
}