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
using System.Collections;
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   public enum AttachField
   {
      Name,
      Handle,
      Role,
      SenderSettleMode,
      ReceiverSettleMode,
      Source,
      Target,
      Unsettled,
      IncompleteUnsettled,
      InitialDeliveryCount,
      MaxMessageSize,
      OfferedCapabilities,
      DesiredCapabilities,
      Properties
   }

   public sealed class Attach : PerformativeDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:attach:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000012ul;

      public Attach() : base(Enum.GetNames(typeof(AttachField)).Length)
      {
      }

      public Attach(object described) : base(Enum.GetNames(typeof(AttachField)).Length, (IList)described)
      {
      }

      public Attach(IList described) : base(Enum.GetNames(typeof(AttachField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Attach;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public bool IsSender => Role?.IsSender() ?? false;

      public bool IsReceiver => Role?.IsReceiver() ?? false;

      public string Name
      {
         get => (string)List[((int)AttachField.Name)];
         set => List[((int)AttachField.Name)] = value;
      }

      public uint? Handle
      {
         get => (uint?)List[((int)AttachField.Handle)];
         set => List[((int)AttachField.Handle)] = value;
      }

      public Role? Role
      {
         get => (Role)List[((int)AttachField.Role)];
         set => List[((int)AttachField.Role)] = value == null ? null : value == Transport.Role.Receiver ? true : false;
      }

      public SenderSettleMode? SenderSettleMode
      {
         get => (SenderSettleMode?)List[((int)AttachField.SenderSettleMode)];
         set => List[((int)AttachField.SenderSettleMode)] = ((byte?)value);
      }

      public ReceiverSettleMode? ReceiverSettleMode
      {
         get => (ReceiverSettleMode?)List[((int)AttachField.ReceiverSettleMode)];
         set => List[((int)AttachField.ReceiverSettleMode)] = ((byte?)value);
      }

      public Source Source
      {
         get => (Source)List[((int)AttachField.Source)];
         set => List[((int)AttachField.Source)] = value;
      }

      public object Target
      {
         get => List[((int)AttachField.Target)];
         set => List[((int)AttachField.Target)] = value;
      }

      public IDictionary Unsettled
      {
         get => (IDictionary)List[((int)AttachField.Unsettled)];
         set => List[((int)AttachField.Unsettled)] = value;
      }

      public bool? IncompleteUnsettled
      {
         get => (bool?)List[((int)AttachField.IncompleteUnsettled)];
         set => List[((int)AttachField.IncompleteUnsettled)] = value;
      }

      public uint? InitialDeliveryCount
      {
         get => (uint?)List[((int)AttachField.InitialDeliveryCount)];
         set => List[((int)AttachField.InitialDeliveryCount)] = value;
      }

      public ulong? MaxMessageSize
      {
         get => (ulong?)List[((int)AttachField.MaxMessageSize)];
         set => List[((int)AttachField.MaxMessageSize)] = value;
      }

      public Symbol[] OfferedCapabilities
      {
         get => (Symbol[])List[((int)AttachField.OfferedCapabilities)];
         set => List[((int)AttachField.OfferedCapabilities)] = value;
      }

      public Symbol[] DesiredCapabilities
      {
         get => (Symbol[])List[((int)AttachField.DesiredCapabilities)];
         set => List[((int)AttachField.DesiredCapabilities)] = value;
      }

      public IDictionary<Symbol, object> Properties
      {
         get => (IDictionary<Symbol, object>)List[((int)AttachField.Properties)];
         set => List[((int)AttachField.Properties)] = value;
      }

      public override string ToString()
      {
         return "Attach{" +
             "name='" + Name + '\'' +
             ", handle=" + Handle +
             ", role=" + Role +
             ", sndSettleMode=" + SenderSettleMode +
             ", rcvSettleMode=" + ReceiverSettleMode +
             ", source=" + Source +
             ", target=" + Target +
             ", unsettled=" + Unsettled +
             ", incompleteUnsettled=" + IncompleteUnsettled +
             ", initialDeliveryCount=" + InitialDeliveryCount +
             ", maxMessageSize=" + MaxMessageSize +
             ", offeredCapabilities=" + OfferedCapabilities +
             ", desiredCapabilities=" + DesiredCapabilities +
             ", properties=" + Properties + '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, Span<byte> payload, ushort channel, T context)
      {
         handler.HandleAttach(frameSize, this, payload, channel, context);
      }
   }
}