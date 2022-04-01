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
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// The receiver builder is used by client session instances to create
   /// AMQP receivers and wrap those within a client receiver instance.
   /// </summary>
   internal class ClientReceiverBuilder
   {
      private readonly ClientSession session;
      private readonly SessionOptions sessionOptions;
      private readonly AtomicInteger receiverCounter = new AtomicInteger();

      private volatile ReceiverOptions defaultReceiverOptions;
      private volatile StreamReceiverOptions defaultStreamReceiverOptions;

      public ClientReceiverBuilder(ClientSession session)
      {
         this.session = session;
         this.sessionOptions = session.Options;
      }

      public ClientReceiver Receiver(string address, ReceiverOptions receiverOptions)
      {
         ReceiverOptions rcvOptions = receiverOptions ?? GetDefaultReceiverOptions();
         string receiverId = NextReceiverId();
         Engine.IReceiver protonReceiver = CreateReceiver(rcvOptions, receiverId);

         protonReceiver.Source = CreateSource(address, rcvOptions);
         protonReceiver.Target = CreateTarget(address, rcvOptions);

         return new ClientReceiver(session, rcvOptions, receiverId, protonReceiver);
      }

      public ClientReceiver DurableReceiver(string address, string subscriptionName, ReceiverOptions receiverOptions)
      {
         ReceiverOptions options = receiverOptions ?? GetDefaultReceiverOptions();
         string receiverId = NextReceiverId();

         options.LinkName = subscriptionName;

         Engine.IReceiver protonReceiver = CreateReceiver(options, receiverId);

         protonReceiver.Source = CreateDurableSource(address, options);
         protonReceiver.Target = CreateTarget(address, options);

         return new ClientReceiver(session, options, receiverId, protonReceiver);
      }

      public ClientReceiver DynamicReceiver(IDictionary<string, object> dynamicNodeProperties, ReceiverOptions receiverOptions)
      {
         ReceiverOptions options = receiverOptions ?? GetDefaultReceiverOptions();
         string receiverId = NextReceiverId();
         Engine.IReceiver protonReceiver = CreateReceiver(options, receiverId);

         protonReceiver.Source = CreateSource(null, options);
         protonReceiver.Target = CreateTarget(null, options);

         // Configure the dynamic nature of the source now.
         protonReceiver.Source.Dynamic = true;
         protonReceiver.Source.DynamicNodeProperties = ClientConversionSupport.ToSymbolKeyedMap(dynamicNodeProperties);

         return new ClientReceiver(session, options, receiverId, protonReceiver);
      }

      public ClientStreamReceiver StreamReceiver(string address, StreamReceiverOptions receiverOptions)
      {
         StreamReceiverOptions options = receiverOptions ?? GetDefaultStreamReceiverOptions();
         string receiverId = NextReceiverId();
         Engine.IReceiver protonReceiver = CreateReceiver(options, receiverId);

         protonReceiver.Source = CreateSource(address, options);
         protonReceiver.Target = CreateTarget(address, options);

         return new ClientStreamReceiver(session, options, receiverId, protonReceiver);
      }

      public static Engine.IReceiver RecreateReceiver(ClientSession session, Engine.IReceiver previousReceiver, ReceiverOptions options)
      {
         Engine.IReceiver protonReceiver = session.ProtonSession.Receiver(previousReceiver.Name);

         protonReceiver.Source = previousReceiver.Source;
         if (previousReceiver.Terminus is Coordinator)
         {
            protonReceiver.Coordinator = protonReceiver.Coordinator;
         }
         else
         {
            protonReceiver.Target = protonReceiver.Target;
         }

         protonReceiver.SenderSettleMode = previousReceiver.SenderSettleMode;
         protonReceiver.ReceiverSettleMode = previousReceiver.ReceiverSettleMode;
         protonReceiver.OfferedCapabilities = ClientConversionSupport.ToSymbolArray(options.OfferedCapabilities);
         protonReceiver.DesiredCapabilities = ClientConversionSupport.ToSymbolArray(options.DesiredCapabilities);
         protonReceiver.Properties = ClientConversionSupport.ToSymbolKeyedMap(options.Properties);
         protonReceiver.DefaultDeliveryState = Released.Instance;

         return protonReceiver;
      }

      #region Private build helper methods

      private string NextReceiverId()
      {
         return session.SessionId + ":" + receiverCounter.IncrementAndGet();
      }

      private Engine.IReceiver CreateReceiver(ReceiverOptions options, String receiverId)
      {
         string linkName = options?.LinkName ?? "receiver-" + receiverId;

         Engine.IReceiver protonReceiver = session.ProtonSession.Receiver(linkName);

         switch (options.DeliveryMode)
         {
            case DeliveryMode.AtMostOnce:
               protonReceiver.SenderSettleMode = SenderSettleMode.Settled;
               protonReceiver.ReceiverSettleMode = ReceiverSettleMode.First;
               break;
            case DeliveryMode.AtLeastOnce:
               protonReceiver.SenderSettleMode = SenderSettleMode.Unsettled;
               protonReceiver.ReceiverSettleMode = ReceiverSettleMode.First;
               break;
         }

         protonReceiver.OfferedCapabilities = ClientConversionSupport.ToSymbolArray(options.OfferedCapabilities);
         protonReceiver.DesiredCapabilities = ClientConversionSupport.ToSymbolArray(options.DesiredCapabilities);
         protonReceiver.Properties = ClientConversionSupport.ToSymbolKeyedMap(options.Properties);
         protonReceiver.DefaultDeliveryState = Released.Instance;

         return protonReceiver;
      }

      private static Source CreateSource(string address, ReceiverOptions options)
      {
         SourceOptions sourceOptions = options.SourceOptions;

         Source source = new Source();
         source.Address = address;

         if (sourceOptions.DurabilityMode.HasValue)
         {
            source.Durable = sourceOptions.DurabilityMode.Value.AsProtonType();
         }
         else
         {
            source.Durable = TerminusDurability.None;
         }

         if (sourceOptions.ExpiryPolicy.HasValue)
         {
            source.ExpiryPolicy = sourceOptions.ExpiryPolicy.Value.AsProtonType();
         }
         else
         {
            source.ExpiryPolicy = TerminusExpiryPolicy.LinkDetach;
         }

         if (sourceOptions.DistributionMode.HasValue)
         {
            source.DistributionMode = sourceOptions.DistributionMode.Value.AsProtonType();
         }

         if (sourceOptions.Timeout.HasValue)
         {
            source.Timeout = sourceOptions.Timeout.Value;
         }

         if (sourceOptions.Filters != null)
         {
            source.Filter = ClientConversionSupport.ToSymbolKeyedMap(sourceOptions.Filters);
         }

         if (sourceOptions.DefaultOutcome != null)
         {
            source.DefaultOutcome = (IOutcome)sourceOptions.DefaultOutcome.AsProtonType();
         }
         else
         {
            source.DefaultOutcome = (IOutcome)SourceOptions.DefaultReceiverOutcome.AsProtonType();
         }

         source.Outcomes = ClientConversionSupport.ToSymbolArray(sourceOptions.Outcomes);
         source.Capabilities = ClientConversionSupport.ToSymbolArray(sourceOptions.Capabilities);

         return source;
      }

      private static Source CreateDurableSource(string address, ReceiverOptions options)
      {
         SourceOptions sourceOptions = options.SourceOptions;
         Source source = new Source();

         source.Address = address;
         source.Durable = TerminusDurability.UnsettledState;
         source.ExpiryPolicy = TerminusExpiryPolicy.Never;
         source.DistributionMode = ClientConstants.COPY;
         source.Outcomes = ClientConversionSupport.ToSymbolArray(sourceOptions.Outcomes);
         source.DefaultOutcome = (IOutcome)(sourceOptions.DefaultOutcome?.AsProtonType());
         source.Capabilities = ClientConversionSupport.ToSymbolArray(sourceOptions.Capabilities);

         if (sourceOptions.Timeout.HasValue)
         {
            source.Timeout = sourceOptions.Timeout.Value;
         }
         if (sourceOptions.Filters != null)
         {
            source.Filter = ClientConversionSupport.ToSymbolKeyedMap(sourceOptions.Filters);
         }

         return source;
      }

      private static Target CreateTarget(String address, ReceiverOptions options)
      {
         TargetOptions targetOptions = options.TargetOptions;
         Target target = new Target();

         target.Address = address;
         target.Capabilities = ClientConversionSupport.ToSymbolArray(targetOptions.Capabilities);

         if (targetOptions.DurabilityMode.HasValue)
         {
            target.Durable = targetOptions.DurabilityMode.Value.AsProtonType();
         }
         if (targetOptions.ExpiryPolicy.HasValue)
         {
            target.ExpiryPolicy = targetOptions.ExpiryPolicy.Value.AsProtonType();
         }
         if (targetOptions.Timeout.HasValue)
         {
            target.Timeout = targetOptions.Timeout.Value;
         }

         return target;
      }

      private ReceiverOptions GetDefaultReceiverOptions()
      {
         ReceiverOptions receiverOptions = defaultReceiverOptions;
         if (receiverOptions == null)
         {
            lock (sessionOptions)
            {
               receiverOptions = defaultReceiverOptions;
               if (receiverOptions == null)
               {
                  receiverOptions = new ReceiverOptions();
                  receiverOptions.OpenTimeout = sessionOptions.OpenTimeout;
                  receiverOptions.CloseTimeout = sessionOptions.CloseTimeout;
                  receiverOptions.RequestTimeout = sessionOptions.RequestTimeout;
                  receiverOptions.DrainTimeout = sessionOptions.DrainTimeout;
               }

               defaultReceiverOptions = receiverOptions;
            }
         }

         return receiverOptions;
      }

      private StreamReceiverOptions GetDefaultStreamReceiverOptions()
      {
         StreamReceiverOptions receiverOptions = defaultStreamReceiverOptions;
         if (receiverOptions == null)
         {
            lock (sessionOptions)
            {
               receiverOptions = defaultStreamReceiverOptions;
               if (receiverOptions == null)
               {
                  receiverOptions = new StreamReceiverOptions();
                  receiverOptions.OpenTimeout = sessionOptions.OpenTimeout;
                  receiverOptions.CloseTimeout = sessionOptions.CloseTimeout;
                  receiverOptions.RequestTimeout = sessionOptions.RequestTimeout;
                  receiverOptions.DrainTimeout = sessionOptions.DrainTimeout;
               }

               defaultStreamReceiverOptions = receiverOptions;
            }
         }

         return receiverOptions;
      }

      #endregion
   }
}