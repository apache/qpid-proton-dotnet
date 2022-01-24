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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public abstract class AbstractListSectionMatcher : AbstractMessageSectionMatcher
   {
      public AbstractListSectionMatcher(ulong numericDescriptor, Symbol symbolicDescriptor, bool expectTrailingBytes)
         : base(numericDescriptor, symbolicDescriptor, expectTrailingBytes)
      {
      }

      protected abstract Enum GetFieldEnumByIndex(uint index);

      protected override void VerifyReceivedDescribedObject(object described)
      {
         if (!(described is IList))
         {
            throw new ArgumentException(
                "Unexpected section contents. Expected List, but got: " +
                (described == null ? "null" : described.GetType()));
         }

         uint fieldNumber = 0;
         IDictionary valueMap = new Dictionary<Enum, object>();
         foreach (object value in (IList)described)
         {
            valueMap.Add(GetFieldEnumByIndex(fieldNumber++), value);
         }

         VerifyReceivedFields(valueMap);
      }
   }
}