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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public sealed class FooterMatcher : AbstractMapSectionMatcher
   {
      public FooterMatcher(bool expectTrailingBytes) :
         base(Footer.DESCRIPTOR_CODE, Footer.DESCRIPTOR_SYMBOL, expectTrailingBytes)
      {
      }

      public FooterMatcher WithEntry(string key, IMatcher m)
      {
         return (FooterMatcher)base.WithEntry(new Symbol(key), m);
      }

      public FooterMatcher WithEntry(Symbol key, IMatcher m)
      {
         return (FooterMatcher)base.WithEntry(key, m);
      }

      public override FooterMatcher WithEntry(object key, IMatcher m)
      {
         if (key is not Symbol)
         {
            throw new ArgumentException("FooterMatcher maps must use non-null Symbol keys");
         }

         return (FooterMatcher)base.WithEntry(key, m);
      }
   }
}