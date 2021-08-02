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
using System.Numerics;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class Properties : ISection
   {
      public static readonly ulong DescriptorCode = 0x0000000000000073UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:properties:list");

      private static readonly uint MESSAGE_ID = 1;
      private static readonly uint USER_ID = 2;
      private static readonly uint TO = 4;
      private static readonly uint SUBJECT = 8;
      private static readonly uint REPLY_TO = 16;
      private static readonly uint CORRELATION_ID = 32;
      private static readonly uint CONTENT_TYPE = 64;
      private static readonly uint CONTENT_ENCODING = 128;
      private static readonly uint ABSOLUTE_EXPIRY = 256;
      private static readonly uint CREATION_TIME = 512;
      private static readonly uint GROUP_ID = 1024;
      private static readonly uint GROUP_SEQUENCE = 2048;
      private static readonly uint REPLY_TO_GROUP_ID = 4096;

      private uint modified = 0;

      private object messageId;
      private IProtonBuffer userId;
      private string to;
      private string subject;
      private string replyTo;
      private object correlationId;
      private string contentType;
      private string contentEncoding;
      private ulong absoluteExpiryTime;
      private ulong creationTime;
      private string groupId;
      private uint groupSequence;
      private string replyToGroupId;

      public Properties() : base()
      {
      }

      public Properties(Properties other) : this()
      {
         messageId = other.messageId;
         userId = other.userId;
         to = other.to;
         subject = other.subject;
         replyTo = other.replyTo;
         correlationId = other.correlationId;
         contentType = other.contentType;
         contentEncoding = other.contentEncoding;
         absoluteExpiryTime = other.absoluteExpiryTime;
         creationTime = other.creationTime;
         groupId = other.groupId;
         groupSequence = other.groupSequence;
         replyToGroupId = other.replyToGroupId;

         modified = other.modified;
      }

      public object Clone()
      {
         return new Properties(this);
      }

      public Properties Copy()
      {
         return new Properties(this);
      }

      #region Element access

      public object MessageId
      {
         get { return messageId; }
         set
         {
            if (value == null)
            {
               modified &= ~MESSAGE_ID;
            }
            else
            {
               modified |= MESSAGE_ID;
            }

            messageId = ValidateIsMessageIdType(value);
         }
      }

      public IProtonBuffer UserId
      {
         get { return userId; }
         set
         {
            if (value == null)
            {
               modified &= ~USER_ID;
            }
            else
            {
               modified |= USER_ID;
            }

            userId = value;
         }
      }

      public string To
      {
         get { return to; }
         set
         {
            if (value == null)
            {
               modified &= ~TO;
            }
            else
            {
               modified |= TO;
            }

            to = value;
         }
      }

      public string Subject
      {
         get { return subject; }
         set
         {
            if (value == null)
            {
               modified &= ~SUBJECT;
            }
            else
            {
               modified |= SUBJECT;
            }

            subject = value;
         }
      }

      public string ReplyTo
      {
         get { return replyTo; }
         set
         {
            if (value == null)
            {
               modified &= ~REPLY_TO;
            }
            else
            {
               modified |= REPLY_TO;
            }

            replyTo = value;
         }
      }

      public object CorrelationId
      {
         get { return correlationId; }
         set
         {
            if (value == null)
            {
               modified &= ~CORRELATION_ID;
            }
            else
            {
               modified |= CORRELATION_ID;
            }

            correlationId = ValidateIsMessageIdType(value);
         }
      }

      public string ContentType
      {
         get { return contentType; }
         set
         {
            if (value == null)
            {
               modified &= ~CONTENT_TYPE;
            }
            else
            {
               modified |= CONTENT_TYPE;
            }

            contentType = value;
         }
      }

      public string ContentEncoding
      {
         get { return contentEncoding; }
         set
         {
            if (value == null)
            {
               modified &= ~CONTENT_ENCODING;
            }
            else
            {
               modified |= CONTENT_ENCODING;
            }

            contentEncoding = value;
         }
      }

      public ulong AbsoluteExpiryTime
      {
         get { return absoluteExpiryTime; }
         set
         {
            modified |= ABSOLUTE_EXPIRY;
            absoluteExpiryTime = value;
         }
      }

      public void ClearAbsoluteExpiryTime()
      {
         modified &= ~ABSOLUTE_EXPIRY;
         absoluteExpiryTime = 0;
      }

      public ulong CreationTime
      {
         get { return creationTime; }
         set
         {
            modified |= CREATION_TIME;
            creationTime = value;
         }
      }

      public void ClearCreationTime()
      {
         modified &= ~CREATION_TIME;
         creationTime = 0;
      }

      public string GroupId
      {
         get { return groupId; }
         set
         {
            if (value == null)
            {
               modified &= ~GROUP_ID;
            }
            else
            {
               modified |= GROUP_ID;
            }

            groupId = value;
         }
      }

      public uint GroupSequence
      {
         get { return groupSequence; }
         set
         {
            modified |= GROUP_SEQUENCE;
            groupSequence = value;
         }
      }

      public void ClearGroupSequence()
      {
         modified &= ~GROUP_SEQUENCE;
         groupSequence = 0u;
      }

      public string ReplyToGroupId
      {
         get { return replyToGroupId; }
         set
         {
            if (value == null)
            {
               modified &= ~REPLY_TO_GROUP_ID;
            }
            else
            {
               modified |= REPLY_TO_GROUP_ID;
            }

            replyToGroupId = value;
         }
      }

      #endregion

      #region Element count and value presence utility

      public bool IsEmpty() => modified == 0;

      public int GetElementCount() => 32 - BitOperations.LeadingZeroCount(modified);

      public bool HasMessageId() => (modified & MESSAGE_ID) == MESSAGE_ID;

      public bool HasUserId() => (modified & USER_ID) == USER_ID;

      public bool HasTo() => (modified & TO) == TO;

      public bool HasSubject() => (modified & SUBJECT) == SUBJECT;

      public bool HasReplyTo() => (modified & REPLY_TO) == REPLY_TO;

      public bool HasCorrelationId() => (modified & CORRELATION_ID) == CORRELATION_ID;

      public bool HasContentType() => (modified & CONTENT_TYPE) == CONTENT_TYPE;

      public bool HasContentEncoding() => (modified & CONTENT_ENCODING) == CONTENT_ENCODING;

      public bool HasAbsoluteExpiryTime() => (modified & ABSOLUTE_EXPIRY) == ABSOLUTE_EXPIRY;

      public bool HasCreationTime() => (modified & CREATION_TIME) == CREATION_TIME;

      public bool HasGroupId() => (modified & GROUP_ID) == GROUP_ID;

      public bool HasGroupSequence() => (modified & GROUP_SEQUENCE) == GROUP_SEQUENCE;

      public bool HasReplyToGroupId() => (modified & REPLY_TO_GROUP_ID) == REPLY_TO_GROUP_ID;

      #endregion

      public SectionType Type => SectionType.Properties;

      public object Value => this;

      public override string ToString()
      {
         return "Properties{" +
                 "messageId=" + messageId +
                 ", userId=" + userId +
                 ", to='" + to + '\'' +
                 ", subject='" + subject + '\'' +
                 ", replyTo='" + replyTo + '\'' +
                 ", correlationId=" + correlationId +
                 ", contentType=" + contentType +
                 ", contentEncoding=" + contentEncoding +
                 ", absoluteExpiryTime=" + (HasAbsoluteExpiryTime() ? absoluteExpiryTime : "null") +
                 ", creationTime=" + (HasCreationTime() ? creationTime : null) +
                 ", groupId='" + groupId + '\'' +
                 ", groupSequence=" + (HasGroupSequence() ? groupSequence : null) +
                 ", replyToGroupId='" + replyToGroupId + '\'' + " }";
      }

      private static object ValidateIsMessageIdType(object messageId)
      {
         // Allowed types of message IDs are listed here
         if (messageId == null ||
             messageId is string ||
             messageId is Guid ||
             messageId is ulong ||
             messageId is IProtonBuffer)
         {
            return messageId;
         }

         throw new ArgumentException(
             "AMQP Message ID type restriction violated, cannot assign type: " + messageId.GetType().Name);
      }
   }
}