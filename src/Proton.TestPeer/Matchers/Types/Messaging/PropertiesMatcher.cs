/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed With
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance With
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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public sealed class PropertiesMatcher : AbstractListSectionMatcher
   {
      public PropertiesMatcher(bool expectTrailingBytes) :
         base(Properties.DESCRIPTOR_CODE, Properties.DESCRIPTOR_SYMBOL, expectTrailingBytes)
      {
      }

      public PropertiesMatcher WithMessageId(object messageId)
      {
         return WithMessageId(Is.EqualTo(messageId));
      }

      public PropertiesMatcher WithUserId(byte[] userId)
      {
         return WithUserId(Is.EqualTo(new Binary(userId)));
      }

      public PropertiesMatcher WithUserId(Binary userId)
      {
         return WithUserId(Is.EqualTo(userId));
      }

      public PropertiesMatcher WithTo(string to)
      {
         return WithTo(Is.EqualTo(to));
      }

      public PropertiesMatcher WithSubject(string subject)
      {
         return WithSubject(Is.EqualTo(subject));
      }

      public PropertiesMatcher WithReplyTo(string replyTo)
      {
         return WithReplyTo(Is.EqualTo(replyTo));
      }

      public PropertiesMatcher WithCorrelationId(object correlationId)
      {
         return WithCorrelationId(Is.EqualTo(correlationId));
      }

      public PropertiesMatcher WithContentType(string contentType)
      {
         return WithContentType(Is.EqualTo(new Symbol(contentType)));
      }

      public PropertiesMatcher WithContentType(Symbol contentType)
      {
         return WithContentType(Is.EqualTo(contentType));
      }

      public PropertiesMatcher WithContentEncoding(string contentEncoding)
      {
         return WithContentEncoding(Is.EqualTo(new Symbol(contentEncoding)));
      }

      public PropertiesMatcher WithContentEncoding(Symbol contentEncoding)
      {
         return WithContentEncoding(Is.EqualTo(contentEncoding));
      }

      public PropertiesMatcher WithAbsoluteExpiryTime(ulong absoluteExpiryTime)
      {
         return WithAbsoluteExpiryTime(Is.EqualTo(absoluteExpiryTime));
      }

      public PropertiesMatcher WithCreationTime(ulong creationTime)
      {
         return WithCreationTime(Is.EqualTo(creationTime));
      }

      public PropertiesMatcher WithGroupId(string groupId)
      {
         return WithGroupId(Is.EqualTo(groupId));
      }

      public PropertiesMatcher WithGroupSequence(uint groupSequence)
      {
         return WithGroupSequence(Is.EqualTo(groupSequence));
      }

      public PropertiesMatcher WithReplyToGroupId(string replyToGroupId)
      {
         return WithReplyToGroupId(Is.EqualTo(replyToGroupId));
      }

      #region Matcher based With methods

      public PropertiesMatcher WithMessageId(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.MessageId, m);
         return this;
      }

      public PropertiesMatcher WithUserId(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.UserID, m);
         return this;
      }

      public PropertiesMatcher WithTo(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.To, m);
         return this;
      }

      public PropertiesMatcher WithSubject(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.Subject, m);
         return this;
      }

      public PropertiesMatcher WithReplyTo(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.ReplyTo, m);
         return this;
      }

      public PropertiesMatcher WithCorrelationId(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.CorrelationId, m);
         return this;
      }

      public PropertiesMatcher WithContentType(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.ContentType, m);
         return this;
      }

      public PropertiesMatcher WithContentEncoding(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.ContentEncoding, m);
         return this;
      }

      public PropertiesMatcher WithAbsoluteExpiryTime(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.AbsoluteExpiryTime, m);
         return this;
      }

      public PropertiesMatcher WithCreationTime(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.CreationTime, m);
         return this;
      }

      public PropertiesMatcher WithGroupId(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.GroupId, m);
         return this;
      }

      public PropertiesMatcher WithGroupSequence(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.GroupSequence, m);
         return this;
      }

      public PropertiesMatcher WithReplyToGroupId(IMatcher m)
      {
         FieldMatchers.Add(PropertiesField.ReplyToGroupId, m);
         return this;
      }

      #endregion

      protected override Enum GetFieldEnumByIndex(uint index)
      {
         return (PropertiesField)index;
      }
   }
}