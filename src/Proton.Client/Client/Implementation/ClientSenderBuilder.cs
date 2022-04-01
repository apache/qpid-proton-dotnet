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
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Engine.Implementation;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// The sender builder is used by client session instances to create
   /// AMQP senders and wrap those within a client sender instance.
   /// </summary>
   internal class ClientSenderBuilder
   {
      private readonly ClientSession session;
      private readonly SessionOptions sessionOptions;
      private readonly AtomicInteger senderCounter = new AtomicInteger();

      private volatile SenderOptions defaultSenderOptions;
      private volatile StreamSenderOptions defaultStreamSenderOptions;

      public ClientSenderBuilder(ClientSession session)
      {
         this.session = session;
         this.sessionOptions = session.Options;
      }

      public ClientSender Sender(string address, SenderOptions senderOptions)
      {
         SenderOptions options = senderOptions ?? GetDefaultSenderOptions();
         string senderId = NextSenderId();
         Engine.ISender protonSender = CreateSender(session.ProtonSession, address, options, senderId);

         return new ClientSender(session, options, senderId, protonSender);
      }

      public ClientSender AnonymousSender(SenderOptions senderOptions)
      {
         SenderOptions options = senderOptions ?? GetDefaultSenderOptions();
         string senderId = NextSenderId();
         Engine.ISender protonSender = CreateSender(session.ProtonSession, null, options, senderId);

         return new ClientSender(session, options, senderId, protonSender);
      }

      public ClientStreamSender StreamSender(String address, StreamSenderOptions senderOptions)
      {
         StreamSenderOptions options = senderOptions ?? GetDefaultStreamSenderOptions();
         string senderId = NextSenderId();
         Engine.ISender protonSender = CreateSender(session.ProtonSession, address, options, senderId);

         return new ClientStreamSender(session, options, senderId, protonSender);
      }

      public static Engine.ISender RecreateSender(ClientSession session, Engine.ISender previousSender, SenderOptions options)
      {
         Engine.ISender protonSender = session.ProtonSession.Sender(previousSender.Name);

         protonSender.Source = previousSender.Source;
         if (previousSender.Terminus is Coordinator coordinator)
         {
            protonSender.Coordinator = coordinator;
         }
         else
         {
            protonSender.Target = previousSender.Target;
         }

         protonSender.DeliveryTagGenerator = previousSender.DeliveryTagGenerator;
         protonSender.SenderSettleMode = previousSender.SenderSettleMode;
         protonSender.ReceiverSettleMode = previousSender.ReceiverSettleMode;
         protonSender.OfferedCapabilities = ClientConversionSupport.ToSymbolArray(options.OfferedCapabilities);
         protonSender.DesiredCapabilities = ClientConversionSupport.ToSymbolArray(options.DesiredCapabilities);
         protonSender.Properties = ClientConversionSupport.ToSymbolKeyedMap(options.Properties);

         return protonSender;
      }

      #region Private sender builder APIs

      private string NextSenderId()
      {
         return session.SessionId + ":" + senderCounter.IncrementAndGet();
      }

      private static Engine.ISender CreateSender(Engine.ISession protonSession, string address, SenderOptions options, string senderId)
      {
         string linkName = options?.LinkName ?? "sender-" + senderId;

         Engine.ISender protonSender = protonSession.Sender(linkName);

         switch (options.DeliveryMode)
         {
            case DeliveryMode.AtMostOnce:
               protonSender.SenderSettleMode = SenderSettleMode.Settled;
               protonSender.ReceiverSettleMode = ReceiverSettleMode.First;
               break;
            case DeliveryMode.AtLeastOnce:
               protonSender.SenderSettleMode = SenderSettleMode.Unsettled;
               protonSender.ReceiverSettleMode = ReceiverSettleMode.First;
               break;
         }

         protonSender.OfferedCapabilities = ClientConversionSupport.ToSymbolArray(options.OfferedCapabilities);
         protonSender.DesiredCapabilities = ClientConversionSupport.ToSymbolArray(options.DesiredCapabilities);
         protonSender.Properties = ClientConversionSupport.ToSymbolKeyedMap(options.Properties);
         protonSender.Target = CreateTarget(address, options);
         protonSender.Source = CreateSource(senderId, options);

         // Use a tag generator that will reuse old tags.  Later we might make this configurable.
         if (protonSender.SenderSettleMode == SenderSettleMode.Settled)
         {
            protonSender.DeliveryTagGenerator = ProtonDeliveryTagTypes.Empty.NewTagGenerator();
         }
         else
         {
            protonSender.DeliveryTagGenerator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();
         }

         return protonSender;
      }

      private static Source CreateSource(string address, SenderOptions options)
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

         source.Outcomes = ClientConversionSupport.ToSymbolArray(sourceOptions.Outcomes);
         source.Capabilities = ClientConversionSupport.ToSymbolArray(sourceOptions.Capabilities);

         return source;
      }

      private static Target CreateTarget(string address, SenderOptions options)
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

      private SenderOptions GetDefaultSenderOptions()
      {
         SenderOptions senderOptions = defaultSenderOptions;
         if (senderOptions == null)
         {
            lock (sessionOptions)
            {
               senderOptions = defaultSenderOptions;
               if (senderOptions == null)
               {
                  senderOptions = new SenderOptions();
                  senderOptions.OpenTimeout = sessionOptions.OpenTimeout;
                  senderOptions.CloseTimeout = sessionOptions.CloseTimeout;
                  senderOptions.RequestTimeout = sessionOptions.RequestTimeout;
                  senderOptions.SendTimeout = sessionOptions.SendTimeout;
               }

               defaultSenderOptions = senderOptions;
            }
         }

         return senderOptions;
      }

      private StreamSenderOptions GetDefaultStreamSenderOptions()
      {
         StreamSenderOptions senderOptions = defaultStreamSenderOptions;
         if (senderOptions == null)
         {
            lock (sessionOptions)
            {
               senderOptions = defaultStreamSenderOptions;
               if (senderOptions == null)
               {
                  senderOptions = new StreamSenderOptions();
                  senderOptions.OpenTimeout = sessionOptions.OpenTimeout;
                  senderOptions.CloseTimeout = sessionOptions.CloseTimeout;
                  senderOptions.RequestTimeout = sessionOptions.RequestTimeout;
                  senderOptions.SendTimeout = sessionOptions.SendTimeout;
               }

               defaultStreamSenderOptions = senderOptions;
            }
         }

         return senderOptions;
      }

      #endregion
   }
}