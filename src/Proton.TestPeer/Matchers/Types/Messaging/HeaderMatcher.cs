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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public sealed class HeaderMatcher : AbstractListSectionMatcher
   {
      public HeaderMatcher(bool expectTrailingBytes) :
         base(Header.DESCRIPTOR_CODE, Header.DESCRIPTOR_SYMBOL, expectTrailingBytes)
      {
      }

      public HeaderMatcher WithDurable(bool durable)
      {
         return WithDurable(Is.EqualTo(durable));
      }

      public HeaderMatcher WithPriority(byte priority)
      {
         return WithPriority(Is.EqualTo(priority));
      }

      public HeaderMatcher WithTtl(uint timeToLive)
      {
         return WithTtl(Is.EqualTo(timeToLive));
      }

      public HeaderMatcher WithFirstAcquirer(bool durable)
      {
         return WithFirstAcquirer(Is.EqualTo(durable));
      }

      public HeaderMatcher WithDeliveryCount(uint deliveryCount)
      {
         return WithDeliveryCount(Is.EqualTo(deliveryCount));
      }

      #region Matcher based with methods

      public HeaderMatcher WithDurable(IMatcher m)
      {
         FieldMatchers.Add(HeaderField.DURABLE, m);
         return this;
      }

      public HeaderMatcher WithPriority(IMatcher m)
      {
         FieldMatchers.Add(HeaderField.PRIORITY, m);
         return this;
      }

      public HeaderMatcher WithTtl(IMatcher m)
      {
         FieldMatchers.Add(HeaderField.TTL, m);
         return this;
      }

      public HeaderMatcher WithFirstAcquirer(IMatcher m)
      {
         FieldMatchers.Add(HeaderField.FIRST_ACQUIRER, m);
         return this;
      }

      public HeaderMatcher WithDeliveryCount(IMatcher m)
      {
         FieldMatchers.Add(HeaderField.DELIVERY_COUNT, m);
         return this;
      }

      #endregion

      protected override Enum GetFieldEnumByIndex(uint index)
      {
         return (HeaderField)index;
      }
   }
}