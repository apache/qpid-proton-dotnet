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
using System.Threading;
using System.Linq;
using NUnit.Framework;
using Apache.Qpid.Proton.Types.Messaging;
using System.Collections.Generic;
using Apache.Qpid.Proton.Types;
using System.Text;

namespace Apache.Qpid.Proton.Client.Impl
{
   [TestFixture, Timeout(20000)]
   public class ClientMessageTest
   {
      [Test]
      public void TestCreateEmpty()
      {
         ClientMessage<String> message = ClientMessage<string>.Create();

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         Assert.IsFalse(message.HasProperties);
         Assert.IsFalse(message.HasFooters);
         Assert.IsFalse(message.HasAnnotations);
      }

      [Test]
      public void TestCreateEmptyAdvanced()
      {
         IAdvancedMessage<string> message = ClientMessage<string>.CreateAdvancedMessage();

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         Assert.IsFalse(message.HasProperties);
         Assert.IsFalse(message.HasFooters);
         Assert.IsFalse(message.HasAnnotations);

         Assert.IsNull(message.Header);
         Assert.IsNull(message.Annotations);
         Assert.IsNull(message.ApplicationProperties);
         Assert.IsNull(message.Footer);

         Header header = new Header();
         Properties properties = new Properties();
         MessageAnnotations ma = new MessageAnnotations(new Dictionary<Symbol, object>());
         ApplicationProperties ap = new ApplicationProperties(new Dictionary<string, object>());
         Footer ft = new Footer(new Dictionary<Symbol, object>());

         message.Header = header;
         message.Properties = properties;
         message.Annotations = ma;
         message.ApplicationProperties = ap;
         message.Footer = ft;

         Assert.AreSame(header, message.Header);
         Assert.AreSame(properties, message.Properties);
         Assert.AreSame(ma, message.Annotations);
         Assert.AreSame(ap, message.ApplicationProperties);
         Assert.AreSame(ft, message.Footer);
      }

      [Test]
      public void TestCreateWithBody()
      {
         ClientMessage<string> message = ClientMessage<string>.Create(new AmqpValue("test"));

         Assert.IsNotNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(1, message.GetBodySections().Count());

         Assert.AreEqual("test", message.Body);

         message.ForEachBodySection(value =>
         {
            Assert.AreEqual(new AmqpValue("test"), value);
         });
      }

      [Test]
      public void TestToAdvancedMessageReturnsSameInstance()
      {
         IMessage<string> message = IMessage<string>.Create("test");

         Assert.IsNotNull(message.Body);

         IAdvancedMessage<string> advanced = message.ToAdvancedMessage();

         Assert.AreSame(message, advanced);

         Assert.IsNotNull(advanced.GetBodySections());
         Assert.AreEqual(1, advanced.GetBodySections().Count());

         Assert.AreEqual("test", advanced.Body);

         advanced.ForEachBodySection(value =>
         {
            Assert.AreEqual(new AmqpValue("test"), value);
         });

         Assert.AreEqual(0, advanced.MessageFormat);
         advanced.MessageFormat = 17;
         Assert.AreEqual(17, advanced.MessageFormat);
      }

      [Test]
      public void TestSetGetHeaderFields()
      {
         IAdvancedMessage<string> message = ClientMessage<string>.CreateAdvancedMessage();

         Assert.AreEqual(Header.DEFAULT_DURABILITY, message.Durable);
         Assert.AreEqual(Header.DEFAULT_FIRST_ACQUIRER, message.FirstAcquirer);
         Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT, message.DeliveryCount);
         Assert.AreEqual(Header.DEFAULT_TIME_TO_LIVE, message.TimeToLive);
         Assert.AreEqual(Header.DEFAULT_PRIORITY, message.Priority);

         message.Durable = true;
         message.FirstAcquirer = true;
         message.DeliveryCount = 10;
         message.TimeToLive = 11;
         message.Priority = 12;

         Assert.AreEqual(true, message.Durable);
         Assert.AreEqual(true, message.FirstAcquirer);
         Assert.AreEqual(10, message.DeliveryCount);
         Assert.AreEqual(11, message.TimeToLive);
         Assert.AreEqual(12, message.Priority);
      }

      [Test]
      public void TestSetGetMessagePropertiesFields()
      {
         IAdvancedMessage<string> message = ClientMessage<string>.CreateAdvancedMessage();

         Assert.IsNull(message.MessageId);
         Assert.IsNull(message.UserId);
         Assert.IsNull(message.To);
         Assert.IsNull(message.Subject);
         Assert.IsNull(message.ReplyTo);
         Assert.IsNull(message.CorrelationId);
         Assert.IsNull(message.ContentType);
         Assert.IsNull(message.ContentEncoding);
         Assert.AreEqual(0, message.CreationTime);
         Assert.AreEqual(0, message.AbsoluteExpiryTime);
         Assert.IsNull(message.GroupId);
         Assert.AreEqual(0, message.GroupSequence);
         Assert.IsNull(message.ReplyToGroupId);

         message.MessageId = "message-id";
         message.UserId = Encoding.UTF8.GetBytes("user-id");
         message.To = "to";
         message.Subject = "subject";
         message.ReplyTo = "replyTo";
         message.CorrelationId = "correlationId";
         message.ContentType = "contentType";
         message.ContentEncoding = "contentEncoding";
         message.CreationTime = 32;
         message.AbsoluteExpiryTime = 64;
         message.GroupId = "groupId";
         message.GroupSequence = 128;
         message.ReplyToGroupId = "replyToGroupId";

         Assert.AreEqual("message-id", message.MessageId);
         byte[] userIdBytes = message.UserId;
         int count = Encoding.UTF8.GetDecoder().GetCharCount(userIdBytes, 0, userIdBytes.Length);
         char[] userIdChars = new char[count];
         Encoding.UTF8.GetDecoder().GetChars(userIdBytes, 0, userIdBytes.Length, userIdChars, 0);

         Assert.AreEqual("user-id", new string(userIdChars));
         Assert.AreEqual("to", message.To);
         Assert.AreEqual("subject", message.Subject);
         Assert.AreEqual("replyTo", message.ReplyTo);
         Assert.AreEqual("subject", message.Subject);
         Assert.AreEqual("correlationId", message.CorrelationId);
         Assert.AreEqual("contentType", message.ContentType);
         Assert.AreEqual("contentEncoding", message.ContentEncoding);
         Assert.AreEqual(32, message.CreationTime);
         Assert.AreEqual(64, message.AbsoluteExpiryTime);
         Assert.AreEqual("groupId", message.GroupId);
         Assert.AreEqual(128, message.GroupSequence);
         Assert.AreEqual("replyToGroupId", message.ReplyToGroupId);
      }

      [Test]
      public void TestBodySetGet()
      {
         IAdvancedMessage<string> message = ClientMessage<string>.CreateAdvancedMessage();

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         Assert.IsNotNull(message.Body = "test");
         Assert.AreEqual("test", message.Body);

         message.ForEachBodySection(value =>
         {
            Assert.AreEqual(new AmqpValue("test"), value);
         });

         message.ClearBodySections();

         Assert.AreEqual(0, message.GetBodySections().Count());
         Assert.IsNull(message.Body);

         int count = 0;
         message.ForEachBodySection(value => count++);
         Assert.AreEqual(0, count);
      }

      [Test]
      public void TestForEachMethodsOnEmptyMessage()
      {
         IAdvancedMessage<string> message = ClientMessage<string>.CreateAdvancedMessage();

         Assert.IsFalse(message.HasProperties);
         Assert.IsFalse(message.HasFooters);
         Assert.IsFalse(message.HasAnnotations);

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         message.ForEachBodySection(value =>
         {
            Assert.Fail("Should not invoke any consumers since Message is empty");
         });

         message.ForEachProperty((key, value) =>
         {
            Assert.Fail("Should not invoke any consumers since Message is empty");
         });

         message.ForEachFooter((key, value) =>
         {
            Assert.Fail("Should not invoke any consumers since Message is empty");
         });

         message.ForEachAnnotation((key, value) =>
         {
            Assert.Fail("Should not invoke any consumers since Message is empty");
         });
      }

      [Test]
      public void TestSetMultipleBodySections()
      {
         IAdvancedMessage<string> message = ClientMessage<string>.CreateAdvancedMessage();

         IList<ISection> expected = new List<ISection>();
         expected.Add(new Data(new byte[] { 0 }));
         expected.Add(new Data(new byte[] { 1 }));
         expected.Add(new Data(new byte[] { 2 }));

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         message.SetBodySections(expected);

         Assert.AreEqual(expected.Count, message.GetBodySections().Count());

         int count = 0;
         message.ForEachBodySection(value =>
         {
            Assert.AreEqual(expected[count], value);
            count++;
         });

         Assert.AreEqual(expected.Count, count);

         count = 0;
         foreach (ISection value in message.GetBodySections())
         {
            Assert.AreEqual(expected[count], value);
            count++;
         }

         Assert.AreEqual(expected.Count, count);

         message.SetBodySections(new List<ISection>());

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         message.SetBodySections(expected);

         Assert.AreEqual(expected.Count, message.GetBodySections().Count());

         message.SetBodySections(null);

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());
      }

      [Test]
      public void TestSetMultipleBodySectionsWithNullClearsOldSingleBodySection()
      {
         IAdvancedMessage<string> message = ClientMessage<string>.CreateAdvancedMessage();

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         message.Body = "test";

         Assert.IsNotNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(1, message.GetBodySections().Count());

         message.SetBodySections(null);

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());
      }

      [Test]
      public void TestAddMultipleBodySectionsPreservesOriginal()
      {
         IAdvancedMessage<byte[]> message = ClientMessage<byte[]>.CreateAdvancedMessage();

         IList<Data> expected = new List<Data>();
         expected.Add(new Data(new byte[] { 1 }));
         expected.Add(new Data(new byte[] { 2 }));
         expected.Add(new Data(new byte[] { 3 }));

         message.Body = new byte[] { 0 };

         Assert.IsNotNull(message.Body);

         foreach (Data value in expected)
         {
            message.AddBodySection(value);
         }

         Assert.AreEqual(expected.Count + 1, message.GetBodySections().Count());

         int counter = 0;
         foreach (ISection section in message.GetBodySections())
         {
            Assert.IsTrue(section is Data);
            Data dataView = (Data)section;
            Assert.AreEqual(counter++, dataView.Value[0]);
         }
      }

      [Test]
      public void TestAddMultipleBodySections()
      {
         IAdvancedMessage<byte[]> message = ClientMessage<byte[]>.CreateAdvancedMessage();

         IList<Data> expected = new List<Data>();
         expected.Add(new Data(new byte[] { 0 }));
         expected.Add(new Data(new byte[] { 1 }));
         expected.Add(new Data(new byte[] { 2 }));

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         foreach (Data value in expected)
         {
            message.AddBodySection(value);
         }

         Assert.AreEqual(expected.Count, message.GetBodySections().Count());

         int count = 0;
         message.ForEachBodySection(value =>
         {
            Assert.AreEqual(expected[count], value);
            count++;
         });

         Assert.AreEqual(expected.Count, count);

         count = 0;
         foreach (ISection section in message.GetBodySections())
         {
            Assert.AreEqual(expected[count], section);
            count++;
         }

         Assert.AreEqual(expected.Count, count);

         message.ClearBodySections();

         Assert.AreEqual(0, message.GetBodySections().Count());

         count = 0;
         foreach (ISection section in message.GetBodySections())
         {
            count++;
         }

         Assert.AreEqual(0, count);

         foreach (Data value in expected)
         {
            message.AddBodySection(value);
         }

         // setting a single body value should clear any previous sections.
         Assert.AreEqual(expected.Count, message.GetBodySections().Count());
         message.Body = new byte[] { 3 };
         Assert.AreEqual(1, message.GetBodySections().Count());
         expected[0] = new Data(new byte[] { 3 });

         IEnumerator<Data> enumerator = expected.GetEnumerator();
         foreach (ISection section in message.GetBodySections())
         {
            enumerator.MoveNext();
            Assert.AreEqual(section, enumerator.Current);
         }

         message.Body = null;

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());
      }

      [Test]
      public void TestMixSingleAndMultipleSectionAccess()
      {
         IAdvancedMessage<byte[]> message = ClientMessage<byte[]>.CreateAdvancedMessage();

         List<Data> expected = new List<Data>();
         expected.Add(new Data(new byte[] { 0 }));
         expected.Add(new Data(new byte[] { 1 }));
         expected.Add(new Data(new byte[] { 2 }));

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         message.Body = expected[0].Value;

         Assert.AreEqual(expected[0].Value, message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(1, message.GetBodySections().Count());

         message.AddBodySection(expected[1]);

         Assert.AreEqual(expected[0].Value, message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(2, message.GetBodySections().Count());

         message.AddBodySection(expected[2]);

         Assert.AreEqual(expected[0].Value, message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(3, message.GetBodySections().Count());

         int count = 0;
         foreach (ISection section in message.GetBodySections())
         {
            Assert.AreEqual(expected[count], section);
            count++;
         }

         Assert.AreEqual(expected.Count, count);
      }

      [Test]
      public void TestSetMultipleBodySectionsValidatesDefaultFormat()
      {
         ClientMessage<object> message = ClientMessage<object>.Create();

         IList<ISection> expected = new List<ISection>();
         expected.Add(new Data(new byte[] { 0 }));
         expected.Add(new AmqpValue("test"));
         expected.Add(new AmqpSequence(new List<string>()));

         Assert.Throws<ArgumentException>(() => message.SetBodySections(expected));
      }

      [Test]
      public void TestAddMultipleBodySectionsValidatesDefaultFormat()
      {
         ClientMessage<object> message = ClientMessage<object>.Create();

         List<ISection> expected1 = new List<ISection>();
         expected1.Add(new Data(new byte[] { 0 }));
         expected1.Add(new AmqpValue("test"));
         expected1.Add(new AmqpSequence(new List<ISection>()));

         Assert.Throws<ArgumentException>(() => expected1.ForEach(section => message.AddBodySection(section)));

         message.ClearBodySections();

         List<ISection> expected2 = new List<ISection>();
         expected2.Add(new AmqpSequence(new List<ISection>()));
         expected2.Add(new Data(new byte[] { 0 }));
         expected2.Add(new AmqpValue("test"));

         Assert.Throws<ArgumentException>(() => expected2.ForEach(section => message.AddBodySection(section)));

         message.ClearBodySections();

         List<ISection> expected3 = new List<ISection>();
         expected3.Add(new AmqpValue("test"));
         expected3.Add(new AmqpSequence(new List<string>()));
         expected3.Add(new Data(new byte[] { 0 }));

         Assert.Throws<ArgumentException>(() => expected3.ForEach(section => message.AddBodySection(section)));
      }

      [Test]
      public void TestReplaceOriginalWithSetBodySectionDoesNotThrowValidationErrorIfValid()
      {
         ClientMessage<object> message = ClientMessage<object>.Create();

         message.Body = "string";  // AmqpValue

         IList<ISection> expected = new List<ISection>();
         expected.Add(new Data(new byte[] { 0 }));

         Assert.DoesNotThrow(() => message.SetBodySections(expected));
      }

      [Test]
      public void TestReplaceOriginalWithSetBodySectionDoesThrowValidationErrorIfInValid()
      {
         ClientMessage<object> message = ClientMessage<object>.Create();

         message.Body = "string";  // AmqpValue

         List<ISection> expected = new List<ISection>();
         expected.Add(new Data(new byte[] { 0 }));
         expected.Add(new AmqpValue("test"));
         expected.Add(new AmqpSequence(new List<object>()));

         Assert.Throws<ArgumentException>(() => message.SetBodySections(expected));
      }

      [Test]
      public void TestAddAdditionalBodySectionsValidatesDefaultFormat()
      {
         ClientMessage<object> message = ClientMessage<object>.Create();

         message.Body = "string";  // AmqpValue

         Assert.Throws<ArgumentException>(() => message.AddBodySection(new Data(new byte[] { 0 })));
      }

      [Test]
      public void TestSetMultipleBodySectionsWithNonDefaultMessageFormat()
      {
         ClientMessage<object> message = ClientMessage<object>.Create();

         message.MessageFormat = 1;

         List<ISection> expected = new List<ISection>();
         expected.Add(new Data(new byte[] { 0 }));
         expected.Add(new AmqpValue("test"));
         expected.Add(new AmqpSequence(new List<object>()));

         Assert.DoesNotThrow(() => message.SetBodySections(expected));

         int count = 0;
         foreach (ISection section in message.GetBodySections())
         {
            Assert.AreEqual(expected[count], section);
            count++;
         }

         Assert.AreEqual(expected.Count, count);
      }

      [Test]
      public void TestAddMultipleBodySectionsWithNonDefaultMessageFormat()
      {
         ClientMessage<object> message = ClientMessage<object>.Create();

         message.MessageFormat = 1;

         List<ISection> expected = new List<ISection>();
         expected.Add(new Data(new byte[] { 0 }));
         expected.Add(new AmqpValue("test"));
         expected.Add(new AmqpSequence(new List<object>()));

         Assert.DoesNotThrow(() => message.SetBodySections(expected));

         int count = 0;
         foreach (ISection section in message.GetBodySections())
         {
            Assert.AreEqual(expected[count], section);
            count++;
         }

         Assert.AreEqual(expected.Count, count);
      }

      [Test]
      public void TestMessageAnnotation()
      {
         ClientMessage<string> message = ClientMessage<string>.Create();

         IDictionary<string, string> expectations = new Dictionary<string, string>();
         expectations.Add("test1", "1");
         expectations.Add("test2", "2");

         Assert.IsFalse(message.HasAnnotations);
         Assert.IsFalse(message.HasAnnotation("test1"));

         Assert.IsNotNull(message.SetAnnotation("test1", "1"));
         Assert.IsNotNull(message.GetAnnotation("test1"));

         Assert.IsTrue(message.HasAnnotations);
         Assert.IsTrue(message.HasAnnotation("test1"));

         Assert.IsNotNull(message.SetAnnotation("test2", "2"));
         Assert.IsNotNull(message.GetAnnotation("test2"));

         int count = 0;

         message.ForEachAnnotation((k, v) =>
         {
            Assert.IsTrue(expectations.ContainsKey(k));
            Assert.AreEqual(v, expectations[k]);
            count++;
         });

         Assert.AreEqual(expectations.Count, count);

         Assert.AreEqual("1", message.RemoveAnnotation("test1"));
         Assert.AreEqual("2", message.RemoveAnnotation("test2"));
         Assert.IsNull(message.RemoveAnnotation("test1"));
         Assert.IsNull(message.RemoveAnnotation("test2"));
         Assert.IsNull(message.RemoveAnnotation("test3"));
         Assert.IsFalse(message.HasAnnotations);
         Assert.IsFalse(message.HasAnnotation("test1"));
         Assert.IsFalse(message.HasAnnotation("test2"));

         message.ForEachAnnotation((k, v) =>
         {
            Assert.Fail("Should not be any remaining Message Annotations");
         });
      }

      [Test]
      public void TestApplicationProperty()
      {
         ClientMessage<string> message = ClientMessage<string>.Create();

         IDictionary<string, string> expectations = new Dictionary<string, string>();
         expectations.Add("test1", "1");
         expectations.Add("test2", "2");

         Assert.IsFalse(message.HasProperties);
         Assert.IsFalse(message.HasProperty("test1"));

         Assert.IsNotNull(message.SetProperty("test1", "1"));
         Assert.IsNotNull(message.GetProperty("test1"));

         Assert.IsTrue(message.HasProperties);
         Assert.IsTrue(message.HasProperty("test1"));

         Assert.IsNotNull(message.SetProperty("test2", "2"));
         Assert.IsNotNull(message.GetProperty("test2"));

         int count = 0;

         message.ForEachProperty((k, v) =>
         {
            Assert.IsTrue(expectations.ContainsKey(k));
            Assert.AreEqual(v, expectations[k]);
            count++;
         });

         Assert.AreEqual(expectations.Count, count);

         Assert.AreEqual("1", message.RemoveProperty("test1"));
         Assert.AreEqual("2", message.RemoveProperty("test2"));
         Assert.IsNull(message.RemoveProperty("test1"));
         Assert.IsNull(message.RemoveProperty("test2"));
         Assert.IsNull(message.RemoveProperty("test3"));
         Assert.IsFalse(message.HasProperties);
         Assert.IsFalse(message.HasProperty("test1"));
         Assert.IsFalse(message.HasProperty("test2"));

         message.ForEachProperty((k, v) =>
         {
            Assert.Fail("Should not be any remaining Application Properties");
         });
      }

      [Test]
      public void TestFooter()
      {
         ClientMessage<string> message = ClientMessage<string>.Create();

         IDictionary<string, string> expectations = new Dictionary<string, string>();
         expectations.Add("test1", "1");
         expectations.Add("test2", "2");

         Assert.IsFalse(message.HasFooters);
         Assert.IsFalse(message.HasFooter("test1"));

         Assert.IsNotNull(message.SetFooter("test1", "1"));
         Assert.IsNotNull(message.GetFooter("test1"));

         Assert.IsTrue(message.HasFooters);
         Assert.IsTrue(message.HasFooter("test1"));

         Assert.IsNotNull(message.SetFooter("test2", "2"));
         Assert.IsNotNull(message.GetFooter("test2"));

         int count = 0;

         message.ForEachFooter((k, v) =>
         {
            Assert.IsTrue(expectations.ContainsKey(k));
            Assert.AreEqual(v, expectations[k]);
            count++;
         });

         Assert.AreEqual(expectations.Count, count);

         Assert.AreEqual("1", message.RemoveFooter("test1"));
         Assert.AreEqual("2", message.RemoveFooter("test2"));
         Assert.IsNull(message.RemoveFooter("test1"));
         Assert.IsNull(message.RemoveFooter("test2"));
         Assert.IsNull(message.RemoveFooter("test3"));
         Assert.IsFalse(message.HasFooters);
         Assert.IsFalse(message.HasFooter("test1"));
         Assert.IsFalse(message.HasFooter("test2"));

         message.ForEachFooter((k, v) =>
         {
            Assert.Fail("Should not be any remaining footers");
         });
      }

      [Test]
      public void TestGetUserIdHandlesNullPropertiesOrNullUserIDInProperties()
      {
         ClientMessage<string> message = ClientMessage<string>.Create();

         Assert.IsNull(message.Properties);
         Assert.IsNull(message.UserId);

         message.Properties = new Properties();

         Assert.IsNull(message.UserId);
      }

      [Test]
      public void TestApplicationPropertiesAccessorHandlerNullMapOrEmptyMap()
      {
         ClientMessage<string> message = ClientMessage<string>.Create();

         Assert.IsNull(message.ApplicationProperties);
         Assert.IsNull(message.GetProperty("test"));
         Assert.IsFalse(message.HasProperty("test"));
         Assert.IsFalse(message.HasProperties);

         message.ApplicationProperties = new ApplicationProperties();

         Assert.IsNotNull(message.ApplicationProperties);
         Assert.IsNull(message.GetProperty("test"));
         Assert.IsFalse(message.HasProperty("test"));
         Assert.IsFalse(message.HasProperties);
      }

      [Test]
      public void TestFooterAccessorHandlerNullMapOrEmptyMap()
      {
         ClientMessage<string> message = ClientMessage<string>.Create();

         Assert.IsNull(message.Footer);
         Assert.IsNull(message.GetFooter("test"));
         Assert.IsFalse(message.HasFooter("test"));
         Assert.IsFalse(message.HasFooters);

         message.Footer = new Footer();

         Assert.IsNotNull(message.Footer);
         Assert.IsNull(message.GetFooter("test"));
         Assert.IsFalse(message.HasFooter("test"));
         Assert.IsFalse(message.HasFooters);
      }

      [Test]
      public void TestMessageAnnotationsAccessorHandlerNullMapOrEmptyMap()
      {
         ClientMessage<string> message = ClientMessage<string>.Create();

         Assert.IsNull(message.Annotations);
         Assert.IsNull(message.GetAnnotation("test"));
         Assert.IsFalse(message.HasAnnotation("test"));
         Assert.IsFalse(message.HasAnnotations);

         message.Annotations = new MessageAnnotations();

         Assert.IsNotNull(message.Annotations);
         Assert.IsNull(message.GetAnnotation("test"));
         Assert.IsFalse(message.HasAnnotation("test"));
         Assert.IsFalse(message.HasAnnotations);
      }
   }
}