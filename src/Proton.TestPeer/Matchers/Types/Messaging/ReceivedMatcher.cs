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
   public sealed class ReceivedMatcher : ListDescribedTypeMatcher
   {
      public ReceivedMatcher() : base(Enum.GetNames(typeof(ReceivedField)).Length, Received.DESCRIPTOR_CODE, Received.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Received);

      public ReceivedMatcher WithSectionNumber(uint sectionNo)
      {
         return WithSectionNumber(Is.EqualTo(sectionNo));
      }

      public ReceivedMatcher WithSectionOffset(uint sectionOffset)
      {
         return WithSectionOffset(Is.EqualTo(sectionOffset));
      }

      #region Matcher based expectations

      public ReceivedMatcher WithSectionNumber(IMatcher m)
      {
         AddFieldMatcher((int)ReceivedField.SectionNumber, m);
         return this;
      }

      public ReceivedMatcher WithSectionOffset(IMatcher m)
      {
         AddFieldMatcher((int)ReceivedField.SectionOffset, m);
         return this;
      }

      #endregion
   }
}