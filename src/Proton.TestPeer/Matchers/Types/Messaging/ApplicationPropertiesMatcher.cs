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
   public sealed class ApplicationPropertiesMatcher : AbstractMapSectionMatcher
   {
      public ApplicationPropertiesMatcher(bool expectTrailingBytes) :
         base(ApplicationProperties.DESCRIPTOR_CODE, ApplicationProperties.DESCRIPTOR_SYMBOL, expectTrailingBytes)
      {
      }

      public ApplicationPropertiesMatcher WithEntry(string key, IMatcher m)
      {
         return (ApplicationPropertiesMatcher)base.WithEntry(key, m);
      }

      public override ApplicationPropertiesMatcher WithEntry(object key, IMatcher m)
      {
         if (key is not String)
         {
            throw new ArgumentException("ApplicationProperties maps must use non-null String keys");
         }

         return (ApplicationPropertiesMatcher)base.WithEntry(key, m);
      }
   }
}