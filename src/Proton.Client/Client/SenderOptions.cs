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
   public class SenderOptions : ICloneable
   {
      /// <summary>
      /// Creates a default sender options instance.
      /// </summary>
      public SenderOptions() : base()
      {
      }

      /// <summary>
      /// Create a new sender options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The sender options instance to copy</param>
      public SenderOptions(SenderOptions other) : base()
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
         return CopyInto(new SenderOptions());
      }

      protected SenderOptions CopyInto(SenderOptions other)
      {
         other.LinkName = LinkName;
         other.AutoSettle = AutoSettle;
         other.SendTimeout = SendTimeout;
         other.RequestTimeout = RequestTimeout;
         other.OpenTimeout = OpenTimeout;
         other.CloseTimeout = CloseTimeout;
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

         return other;
      }

      /// <summary>
      /// Configures the link name that is assigned to the sender created from these options.
      /// </summary>
      public string LinkName { get; set; }

      /// <summary>
      /// Configures the delivery mode used by senders created using these options. By default
      /// the senders will use a delivery mode of at least once.
      /// </summary>
      public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.AtLeastOnce;

      /// <summary>
      /// Configures whether senders created using these options will automatically settle
      /// deliveries that were sent once the remote responds that the send was accepted and
      /// settled.
      /// </summary>
      public bool AutoSettle { get; set; } = true;

      /// <summary>
      /// Configures the send timeout for each sender created using these options. This
      /// timeout controls how long a sender will wait for a send to complete before giving
      /// up and signalling a send failure.
      /// </summary>
      public long SendTimeout { get; set; } = ConnectionOptions.DEFAULT_SEND_TIMEOUT;

      /// <summary>
      /// Configures the request timeout for each sender created using these options.
      /// </summary>
      public long RequestTimeout { get; set; } = ConnectionOptions.DEFAULT_REQUEST_TIMEOUT;

      /// <summary>
      /// Gets or sets the sender open timeout value which will be used as the for all senders
      /// created using these options. Controls how long a client will wait for a remote to
      /// respond to the open of a resource before signalling that the open has failed.
      /// </summary>
      public long OpenTimeout { get; set; } = ConnectionOptions.DEFAULT_OPEN_TIMEOUT;

      /// <summary>
      /// Gets or sets the sender close timeout value which will be used as the for all senders
      /// created using these options. Controls how long a client will wait for a remote to
      /// respond to the open of a resource before signalling that the close has failed.
      /// </summary>
      public long CloseTimeout { get; set; } = ConnectionOptions.DEFAULT_CLOSE_TIMEOUT;

      /// <summary>
      /// Configures the set of capabilities that a new sender will advertise to the remote.
      /// </summary>
      public string[] OfferedCapabilities { get; set; }

      /// <summary>
      /// Sets the collection of capabilities to request from the remote for a new sender.
      /// The desired capabilities inform the remote peer of the various capabilities the sender
      /// requires and the remote should return those that it supports in its offered capabilities.
      /// </summary>
      public string[] DesiredCapabilities { get; set; }

      /// <summary>
      /// Configures a collection of property values that are sent to the remote upon opening
      /// a new sender.
      /// </summary>
      public IDictionary<string, object> Properties { get; set; }

   }
}