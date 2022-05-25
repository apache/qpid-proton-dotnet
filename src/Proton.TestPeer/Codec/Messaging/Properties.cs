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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Messaging
{
   public enum PropertiesField : uint
   {
      MessageId,
      UserID,
      To,
      Subject,
      ReplyTo,
      CorrelationId,
      ContentType,
      ContentEncoding,
      AbsoluteExpiryTime,
      CreationTime,
      GroupId,
      GroupSequence,
      ReplyToGroupId,
   }

   public sealed class Properties : ListDescribedType
   {
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000073UL;
      public static readonly Symbol DESCRIPTOR_SYMBOL = new("amqp:properties:list");

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Properties() : base(Enum.GetNames(typeof(PropertiesField)).Length)
      {
      }

      public Properties(object described) : base(Enum.GetNames(typeof(PropertiesField)).Length, (IList)described)
      {
      }

      public Properties(IList described) : base(Enum.GetNames(typeof(PropertiesField)).Length, described)
      {
      }

      public object MessageId
      {
         get => List[((int)PropertiesField.MessageId)];
         set => List[((int)PropertiesField.MessageId)] = value;
      }

      public Binary UserId
      {
         get => (Binary)List[((int)PropertiesField.UserID)];
         set => List[((int)PropertiesField.UserID)] = value;
      }

      public string To
      {
         get => (string)List[((int)PropertiesField.To)];
         set => List[((int)PropertiesField.To)] = value;
      }

      public string Subject
      {
         get => (string)List[((int)PropertiesField.Subject)];
         set => List[((int)PropertiesField.Subject)] = value;
      }

      public string ReplyTo
      {
         get => (string)List[((int)PropertiesField.ReplyTo)];
         set => List[((int)PropertiesField.ReplyTo)] = value;
      }

      public object CorrelationId
      {
         get => List[((int)PropertiesField.CorrelationId)];
         set => List[((int)PropertiesField.CorrelationId)] = value;
      }

      public Symbol ContentType
      {
         get => (Symbol)List[((int)PropertiesField.ContentType)];
         set => List[((int)PropertiesField.ContentType)] = value;
      }

      public Symbol ContentEncoding
      {
         get => (Symbol)List[((int)PropertiesField.ContentEncoding)];
         set => List[((int)PropertiesField.ContentEncoding)] = value;
      }

      public ulong? AbsoluteExpiryTime
      {
         get => (ulong?)List[((int)PropertiesField.AbsoluteExpiryTime)];
         set => List[((int)PropertiesField.AbsoluteExpiryTime)] = value;
      }

      public ulong? CreationTime
      {
         get => (ulong?)List[((int)PropertiesField.CreationTime)];
         set => List[((int)PropertiesField.CreationTime)] = value;
      }

      public string GroupId
      {
         get => (string)List[((int)PropertiesField.GroupId)];
         set => List[((int)PropertiesField.GroupId)] = value;
      }

      public uint? GroupSequence
      {
         get => (uint?)List[((int)PropertiesField.GroupSequence)];
         set => List[((int)PropertiesField.GroupSequence)] = value;
      }

      public string ReplyToGroupId
      {
         get => (string)List[((int)PropertiesField.ReplyToGroupId)];
         set => List[((int)PropertiesField.ReplyToGroupId)] = value;
      }

      public override string ToString()
      {
         return "Properties{" +
                 "messageId=" + MessageId +
                 ", userId=" + UserId +
                 ", to='" + To + '\'' +
                 ", subject='" + Subject + '\'' +
                 ", replyTo='" + ReplyTo + '\'' +
                 ", correlationId=" + CorrelationId +
                 ", contentType=" + ContentType +
                 ", contentEncoding=" + ContentEncoding +
                 ", absoluteExpiryTime=" + AbsoluteExpiryTime +
                 ", creationTime=" + CreationTime +
                 ", groupId='" + GroupId + '\'' +
                 ", groupSequence=" + GroupSequence +
                 ", replyToGroupId='" + ReplyToGroupId + '\'' + " }";
      }
   }
}