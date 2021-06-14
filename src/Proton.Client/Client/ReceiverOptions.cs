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
   public class ReceiverOptions : ICloneable
   {
      /// <summary>
      /// Creates a default receiver options instance.
      /// </summary>
      public ReceiverOptions() : base()
      {
      }

      /// <summary>
      /// Create a new receiver options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The sender options instance to copy</param>
      public ReceiverOptions(ReceiverOptions other) : base()
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
         return CopyInto(new ReceiverOptions());
      }

      protected ReceiverOptions CopyInto(ReceiverOptions other)
      {
         other.LinkName = LinkName;
         other.AutoAccept = AutoAccept;
         other.CreditWindow = CreditWindow;
         other.AutoSettle = AutoSettle;
         other.DeliveryMode = DeliveryMode;
         other.DrainTimeout = DrainTimeout;
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
      /// Configures the link name that is assigned to the receiver created from these options.
      /// </summary>
      public string LinkName { get; set; }

      /// <summary>
      /// Controls if receivers created with these options will automatically accept deliveries after
      /// they have been delivered to an applications.
      /// </summary>
      public bool AutoAccept { get; set; } = true;

      /// <summary>
      /// Controls if receivers created with these options will automatically settle deliveries after
      /// they have been delivered to an applications.
      /// </summary>
      public bool AutoSettle { get; set; } = true;

      /// <summary>
      /// A credit window value that will be used to maintain an window of credit for Receiver instances
      /// that are created from these options.  The receiver will allow up to the credit window amount of
      /// incoming deliveries to be queued and as they are read from the receiver the window will be extended
      /// to maintain a consistent backlog of deliveries.  The default is to configure a credit window of 10.
      ///
      /// To disable credit windowing and allow the client application to control the credit on the receiver
      /// link the credit window value should be set to zero.
      /// </summary>
      public int CreditWindow { get; set; } = 10;

      /// <summary>
      /// Configures the delivery mode used by receivers created using these options. By default
      /// the receivers will use a delivery mode of at least once.
      /// </summary>
      public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.AtLeastOnce;

      /// <summary>
      /// Configures the drain timeout that is used by all receivers created from these options.
      /// This value controls how long a receiver waits for the remote to answer a drain request
      /// before considering the drain operation to have failed.
      /// </summary>
      public long DrainTimeout { get; set; } = ConnectionOptions.DEFAULT_DRAIN_TIMEOUT;

      /// <summary>
      /// Configures the request timeout for each receiver created using these options.
      /// </summary>
      public long RequestTimeout { get; set; } = ConnectionOptions.DEFAULT_REQUEST_TIMEOUT;

      /// <summary>
      /// Gets or sets the receiver open timeout value which will be used as the for all senders
      /// created using these options. Controls how long a client will wait for a remote to
      /// respond to the open of a resource before signalling that the open has failed.
      /// </summary>
      public long OpenTimeout { get; set; } = ConnectionOptions.DEFAULT_OPEN_TIMEOUT;

      /// <summary>
      /// Gets or sets the receiver close timeout value which will be used as the for all senders
      /// created using these options. Controls how long a client will wait for a remote to
      /// respond to the open of a resource before signalling that the close has failed.
      /// </summary>
      public long CloseTimeout { get; set; } = ConnectionOptions.DEFAULT_CLOSE_TIMEOUT;

      /// <summary>
      /// Configures the set of capabilities that a new receiver will advertise to the remote.
      /// </summary>
      public string[] OfferedCapabilities { get; set; }

      /// <summary>
      /// Sets the collection of capabilities to request from the remote for a new receiver.
      /// The desired capabilities inform the remote peer of the various capabilities the sender
      /// requires and the remote should return those that it supports in its offered capabilities.
      /// </summary>
      public string[] DesiredCapabilities { get; set; }

      /// <summary>
      /// Configures a collection of property values that are sent to the remote upon opening
      /// a new receiver.
      /// </summary>
      public IDictionary<string, object> Properties { get; set; }

   }
}