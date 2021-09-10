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
   public sealed class MessageAnnotationsMatcher : AbstractMapSectionMatcher
   {
      public MessageAnnotationsMatcher(bool expectTrailingBytes) :
         base(MessageAnnotations.DESCRIPTOR_CODE, MessageAnnotations.DESCRIPTOR_SYMBOL, expectTrailingBytes)
      {
      }

      public MessageAnnotationsMatcher WithEntry(string key, IMatcher m)
      {
         return (MessageAnnotationsMatcher)base.WithEntry(new Symbol(key), m);
      }

      public MessageAnnotationsMatcher WithEntry(Symbol key, IMatcher m)
      {
         return (MessageAnnotationsMatcher)base.WithEntry(key, m);
      }

      public override MessageAnnotationsMatcher WithEntry(object key, IMatcher m)
      {
         if (key is not Symbol)
         {
            throw new ArgumentException("MessageAnnotationsMatcher maps must use non-null Symbol keys");
         }

         return (MessageAnnotationsMatcher)base.WithEntry(key, m);
      }
   }
}