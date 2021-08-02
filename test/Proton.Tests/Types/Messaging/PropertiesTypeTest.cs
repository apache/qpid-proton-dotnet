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
using System.Text;
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class PropertiesTypeTest
   {
      private static string TEST_MESSAGE_ID = "test";
      private static IProtonBuffer TEST_USER_ID = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
      private static string TEST_TO_ADDRESS = "to";
      private static string TEST_TO_SUBJECT = "subject";
      private static string TEST_REPLYTO_ADDRESS = "reply-to";
      private static string TEST_CORRELATION_ID = "correlation";
      private static string TEST_CONTENT_TYPE = "text/test";
      private static string TEST_CONTENT_ENCODING = "UTF-8";
      private static uint TEST_ABSOLUTE_EXPIRY_TIME = 100u;
      private static uint TEST_CREATION_TIME = 200u;
      private static string TEST_GROUP_ID = "group-test";
      private static uint TEST_GROUP_SEQUENCE = 300u;
      private static string TEST_REPLYTO_GROUPID = "reply-to-group";

      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new Properties().ToString());
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.Properties, new Properties().Type);
      }

      [Test]
      public void TestCreate()
      {
         Properties properties = new Properties();

         Assert.IsNull(properties.MessageId);
         Assert.IsNull(properties.UserId);
         Assert.IsNull(properties.To);
         Assert.IsNull(properties.Subject);
         Assert.IsNull(properties.ReplyTo);
         Assert.IsNull(properties.CorrelationId);
         Assert.IsNull(properties.ContentType);
         Assert.IsNull(properties.ContentEncoding);
         Assert.AreEqual(0, properties.AbsoluteExpiryTime);
         Assert.AreEqual(0, properties.CreationTime);
         Assert.IsNull(properties.GroupId);
         Assert.AreEqual(0, properties.GroupSequence);
         Assert.IsNull(properties.ReplyToGroupId);

         Assert.IsTrue(properties.IsEmpty());
         Assert.AreEqual(0, properties.GetElementCount());

         Assert.AreSame(properties, properties.Value);
      }

      [Test]
      public void TestCopyFromDefault()
      {
         Properties properties = new Properties();

         Assert.IsNull(properties.MessageId);
         Assert.IsNull(properties.UserId);
         Assert.IsNull(properties.To);
         Assert.IsNull(properties.Subject);
         Assert.IsNull(properties.ReplyTo);
         Assert.IsNull(properties.CorrelationId);
         Assert.IsNull(properties.ContentType);
         Assert.IsNull(properties.ContentEncoding);
         Assert.AreEqual(0, properties.AbsoluteExpiryTime);
         Assert.AreEqual(0, properties.CreationTime);
         Assert.IsNull(properties.GroupId);
         Assert.AreEqual(0, properties.GroupSequence);
         Assert.IsNull(properties.ReplyToGroupId);
         Assert.IsTrue(properties.IsEmpty());
         Assert.AreEqual(0, properties.GetElementCount());

         Properties copy = properties.Copy();

         Assert.IsNull(copy.MessageId);
         Assert.IsNull(copy.UserId);
         Assert.IsNull(copy.To);
         Assert.IsNull(copy.Subject);
         Assert.IsNull(copy.ReplyTo);
         Assert.IsNull(copy.CorrelationId);
         Assert.IsNull(copy.ContentType);
         Assert.IsNull(copy.ContentEncoding);
         Assert.AreEqual(0, copy.AbsoluteExpiryTime);
         Assert.AreEqual(0, copy.CreationTime);
         Assert.IsNull(copy.GroupId);
         Assert.AreEqual(0, copy.GroupSequence);
         Assert.IsNull(copy.ReplyToGroupId);
         Assert.IsTrue(copy.IsEmpty());
         Assert.AreEqual(0, copy.GetElementCount());
      }

      [Test]
      public void TestCopyConstructor()
      {
         Properties properties = new Properties();

         properties.MessageId = TEST_MESSAGE_ID;
         properties.UserId = TEST_USER_ID;
         properties.To = TEST_TO_ADDRESS;
         properties.Subject = TEST_TO_SUBJECT;
         properties.ReplyTo = TEST_REPLYTO_ADDRESS;
         properties.CorrelationId = TEST_CORRELATION_ID;
         properties.ContentType = TEST_CONTENT_TYPE;
         properties.ContentEncoding = TEST_CONTENT_ENCODING;
         properties.AbsoluteExpiryTime = TEST_ABSOLUTE_EXPIRY_TIME;
         properties.CreationTime = TEST_CREATION_TIME;
         properties.GroupId = TEST_GROUP_ID;
         properties.GroupSequence = TEST_GROUP_SEQUENCE;
         properties.ReplyToGroupId = TEST_REPLYTO_GROUPID;

         Properties copy = new Properties(properties);

         Assert.IsFalse(copy.IsEmpty());

         Assert.IsTrue(copy.HasMessageId());
         Assert.IsTrue(copy.HasUserId());
         Assert.IsTrue(copy.HasTo());
         Assert.IsTrue(copy.HasSubject());
         Assert.IsTrue(copy.HasReplyTo());
         Assert.IsTrue(copy.HasCorrelationId());
         Assert.IsTrue(copy.HasContentType());
         Assert.IsTrue(copy.HasContentEncoding());
         Assert.IsTrue(copy.HasAbsoluteExpiryTime());
         Assert.IsTrue(copy.HasCreationTime());
         Assert.IsTrue(copy.HasGroupId());
         Assert.IsTrue(copy.HasGroupSequence());
         Assert.IsTrue(copy.HasReplyToGroupId());

         // Check bool has methods
         Assert.AreEqual(properties.HasMessageId(), copy.HasMessageId());
         Assert.AreEqual(properties.HasUserId(), copy.HasUserId());
         Assert.AreEqual(properties.HasTo(), copy.HasTo());
         Assert.AreEqual(properties.HasSubject(), copy.HasSubject());
         Assert.AreEqual(properties.HasReplyTo(), copy.HasReplyTo());
         Assert.AreEqual(properties.HasCorrelationId(), copy.HasCorrelationId());
         Assert.AreEqual(properties.HasContentType(), copy.HasContentType());
         Assert.AreEqual(properties.HasContentEncoding(), copy.HasContentEncoding());
         Assert.AreEqual(properties.HasAbsoluteExpiryTime(), copy.HasAbsoluteExpiryTime());
         Assert.AreEqual(properties.HasCreationTime(), copy.HasCreationTime());
         Assert.AreEqual(properties.HasGroupId(), copy.HasGroupId());
         Assert.AreEqual(properties.HasGroupSequence(), copy.HasGroupSequence());
         Assert.AreEqual(properties.HasReplyToGroupId(), copy.HasReplyToGroupId());

         // Test actual values copied
         Assert.AreEqual(properties.MessageId, copy.MessageId);
         Assert.AreEqual(properties.UserId, copy.UserId);
         Assert.AreEqual(properties.To, copy.To);
         Assert.AreEqual(properties.Subject, copy.Subject);
         Assert.AreEqual(properties.ReplyTo, copy.ReplyTo);
         Assert.AreEqual(properties.CorrelationId, copy.CorrelationId);
         Assert.AreEqual(properties.ContentType, copy.ContentType);
         Assert.AreEqual(properties.ContentEncoding, copy.ContentEncoding);
         Assert.AreEqual(properties.AbsoluteExpiryTime, copy.AbsoluteExpiryTime);
         Assert.AreEqual(properties.CreationTime, copy.CreationTime);
         Assert.AreEqual(properties.GroupId, copy.GroupId);
         Assert.AreEqual(properties.GroupSequence, copy.GroupSequence);
         Assert.AreEqual(properties.ReplyToGroupId, copy.ReplyToGroupId);

         Assert.AreEqual(properties.GetElementCount(), copy.GetElementCount());
      }

      [Test]
      public void TestGetElementCount()
      {
         Properties properties = new Properties();

         Assert.IsTrue(properties.IsEmpty());
         Assert.AreEqual(0, properties.GetElementCount());

         properties.MessageId = "ID";

         Assert.IsFalse(properties.IsEmpty());
         Assert.AreEqual(1, properties.GetElementCount());

         properties.MessageId = null;

         Assert.IsTrue(properties.IsEmpty());
         Assert.AreEqual(0, properties.GetElementCount());

         properties.ReplyToGroupId = "ID";
         Assert.IsFalse(properties.IsEmpty());
         Assert.AreEqual(13, properties.GetElementCount());

         properties.MessageId = "ID";
         Assert.IsFalse(properties.IsEmpty());
         Assert.AreEqual(13, properties.GetElementCount());
      }

      [Test]
      public void TestMessageId()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasMessageId());
         Assert.IsNull(properties.MessageId);

         properties.MessageId = "ID";
         Assert.IsTrue(properties.HasMessageId());
         Assert.IsNotNull(properties.MessageId);

         properties.MessageId = Guid.NewGuid();
         Assert.IsTrue(properties.HasMessageId());
         Assert.IsNotNull(properties.MessageId);

         properties.MessageId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         Assert.IsTrue(properties.HasMessageId());
         Assert.IsNotNull(properties.MessageId);

         properties.MessageId = 0ul;
         Assert.IsTrue(properties.HasMessageId());
         Assert.IsNotNull(properties.MessageId);

         properties.MessageId = null;
         Assert.IsFalse(properties.HasMessageId());
         Assert.IsNull(properties.MessageId);

         try
         {
            properties.MessageId = new Dictionary<string, string>();
            Assert.Fail("Not a valid MessageId type.");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestUserId()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasUserId());
         Assert.IsNull(properties.UserId);

         properties.UserId = ProtonByteBufferAllocator.Instance.Wrap(Encoding.UTF8.GetBytes("ID"));
         Assert.IsTrue(properties.HasUserId());
         Assert.IsNotNull(properties.UserId);

         properties.UserId = (IProtonBuffer)null;
         Assert.IsFalse(properties.HasUserId());
         Assert.IsNull(properties.UserId);
      }

      [Test]
      public void TestUserIdFromByteArray()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasUserId());
         Assert.IsNull(properties.UserId);

         properties.UserId = ProtonByteBufferAllocator.Instance.Wrap(Encoding.UTF8.GetBytes("ID"));
         Assert.IsTrue(properties.HasUserId());
         Assert.IsNotNull(properties.UserId);

         properties.UserId = (IProtonBuffer)null;
         Assert.IsFalse(properties.HasUserId());
         Assert.IsNull(properties.UserId);
      }

      [Test]
      public void TestTo()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasTo());
         Assert.IsNull(properties.To);

         properties.To = "ID";
         Assert.IsTrue(properties.HasTo());
         Assert.IsNotNull(properties.To);

         properties.To = null;
         Assert.IsFalse(properties.HasTo());
         Assert.IsNull(properties.To);
      }

      [Test]
      public void TestSubject()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasSubject());
         Assert.IsNull(properties.Subject);

         properties.Subject = "ID";
         Assert.IsTrue(properties.HasSubject());
         Assert.IsNotNull(properties.Subject);

         properties.Subject = null;
         Assert.IsFalse(properties.HasSubject());
         Assert.IsNull(properties.Subject);
      }

      [Test]
      public void TestReplyTo()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasReplyTo());
         Assert.IsNull(properties.ReplyTo);

         properties.ReplyTo = "ID";
         Assert.IsTrue(properties.HasReplyTo());
         Assert.IsNotNull(properties.ReplyTo);

         properties.ReplyTo = null;
         Assert.IsFalse(properties.HasReplyTo());
         Assert.IsNull(properties.ReplyTo);
      }

      [Test]
      public void TestCorrelationId()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasCorrelationId());
         Assert.IsNull(properties.CorrelationId);

         properties.CorrelationId = "ID";
         Assert.IsTrue(properties.HasCorrelationId());
         Assert.IsNotNull(properties.CorrelationId);

         properties.CorrelationId = null;
         Assert.IsFalse(properties.HasCorrelationId());
         Assert.IsNull(properties.CorrelationId);

         try
         {
            properties.CorrelationId = new Dictionary<string, string>();
            Assert.Fail("Not a valid MessageId type.");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestContentType()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasContentType());
         Assert.IsNull(properties.ContentType);

         properties.ContentType = "ID";
         Assert.IsTrue(properties.HasContentType());
         Assert.IsNotNull(properties.ContentType);

         properties.ContentType = null;
         Assert.IsFalse(properties.HasContentType());
         Assert.IsNull(properties.ContentType);
      }

      [Test]
      public void TestContentEncoding()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasContentEncoding());
         Assert.IsNull(properties.ContentEncoding);

         properties.ContentEncoding = "ID";
         Assert.IsTrue(properties.HasContentEncoding());
         Assert.IsNotNull(properties.ContentEncoding);

         properties.ContentEncoding = null;
         Assert.IsFalse(properties.HasContentEncoding());
         Assert.IsNull(properties.ContentEncoding);
      }

      [Test]
      public void TestAbsoluteExpiryTime()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasAbsoluteExpiryTime());
         Assert.AreEqual(0, properties.AbsoluteExpiryTime);

         properties.AbsoluteExpiryTime = 2048;
         Assert.IsTrue(properties.HasAbsoluteExpiryTime());
         Assert.AreEqual(2048, properties.AbsoluteExpiryTime);

         properties.ClearAbsoluteExpiryTime();
         Assert.IsFalse(properties.HasAbsoluteExpiryTime());
         Assert.AreEqual(0, properties.AbsoluteExpiryTime);
      }

      [Test]
      public void TestCreationTime()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasCreationTime());
         Assert.AreEqual(0, properties.CreationTime);

         properties.CreationTime = 2048;
         Assert.IsTrue(properties.HasCreationTime());
         Assert.AreEqual(2048, properties.CreationTime);

         properties.ClearCreationTime();
         Assert.IsFalse(properties.HasCreationTime());
         Assert.AreEqual(0, properties.CreationTime);
      }

      [Test]
      public void TestGroupId()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasGroupId());
         Assert.IsNull(properties.GroupId);

         properties.GroupId = "ID";
         Assert.IsTrue(properties.HasGroupId());
         Assert.IsNotNull(properties.GroupId);

         properties.GroupId = null;
         Assert.IsFalse(properties.HasGroupId());
         Assert.IsNull(properties.GroupId);
      }

      [Test]
      public void TestGroupSequence()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasGroupSequence());
         Assert.AreEqual(0, properties.GroupSequence);

         properties.GroupSequence = 2048;
         Assert.IsTrue(properties.HasGroupSequence());
         Assert.AreEqual(2048, properties.GroupSequence);

         properties.ClearGroupSequence();
         Assert.IsFalse(properties.HasGroupSequence());
         Assert.AreEqual(0, properties.GroupSequence);

         properties.GroupSequence = uint.MaxValue;
         Assert.IsTrue(properties.HasGroupSequence());
         Assert.AreEqual(uint.MaxValue, properties.GroupSequence);
      }

      [Test]
      public void TestReplyToGroupId()
      {
         Properties properties = new Properties();

         Assert.IsFalse(properties.HasReplyToGroupId());
         Assert.IsNull(properties.ReplyToGroupId);

         properties.ReplyToGroupId = "ID";
         Assert.IsTrue(properties.HasReplyToGroupId());
         Assert.IsNotNull(properties.ReplyToGroupId);

         properties.ReplyToGroupId = null;
         Assert.IsFalse(properties.HasReplyToGroupId());
         Assert.IsNull(properties.ReplyToGroupId);
      }
   }
}