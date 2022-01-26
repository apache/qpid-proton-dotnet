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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec;
using Apache.Qpid.Proton.Test.Driver.Codec.Impl;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   /// <summary>
   /// Base matcher implementation for AMQP sections that are encoded into the message
   /// body section.
   /// </summary>
   public abstract class AbstractBodySectionMatcher : TypeSafeMatcher<Stream>
   {
      private readonly Symbol descriptorSymbol;
      private readonly ulong descriptorCode;
      private readonly Object expectedValue;
      private readonly bool permitTrailingBytes;

      private IDescribedType decodedDescribedType;
      private bool unexpectedTrailingBytes;

      public AbstractBodySectionMatcher(Symbol symbol, ulong code, object expectedValue) : this(symbol, code, expectedValue, false)
      {
      }

      public AbstractBodySectionMatcher(Symbol symbol, ulong code, object expectedValue, bool permitTrailingBytes)
      {
         this.descriptorSymbol = symbol;
         this.descriptorCode = code;
         this.expectedValue = expectedValue;
         this.permitTrailingBytes = permitTrailingBytes;
      }

      protected object ExpectedValue => expectedValue;

      protected override void DescribeMismatchSafely(Stream item, IDescription mismatchDescription)
      {
         mismatchDescription.AppendText("\nActual encoded form: ").AppendValue(item);

         if (decodedDescribedType != null)
         {
            mismatchDescription.AppendText("\nExpected descriptor: ")
                               .AppendValue(descriptorSymbol)
                               .AppendText(" / ")
                               .AppendValue(descriptorCode);
            mismatchDescription.AppendText("\nActual described type: ").AppendValue(decodedDescribedType);
         }

         if (unexpectedTrailingBytes)
         {
            mismatchDescription.AppendText("\nUnexpected trailing bytes in provided bytes after decoding!");
         }
      }

      protected override bool MatchesSafely(Stream incoming)
      {
         ICodec data = CodecFactory.Create();
         long decoded = data.Decode(incoming);
         decodedDescribedType = data.GetDescribedType();
         object descriptor = decodedDescribedType.Descriptor;

         if (!(descriptorCode.Equals(descriptor) || descriptorSymbol.Equals(descriptor)))
         {
            return false;
         }

         if (expectedValue == null && decodedDescribedType.Described != null)
         {
            return false;
         }
         else if (expectedValue != null)
         {
            if (expectedValue is IMatcher)
            {
               IMatcher matcher = (IMatcher)expectedValue;
               if (!matcher.Matches(decodedDescribedType.Described))
               {
                  return false;
               }
            }
            else if (expectedValue is IEnumerable exEnumerable &&
                     decodedDescribedType.Described is IEnumerable actEnumerable)
            {
               IEnumerator expectedEnum = exEnumerable.GetEnumerator();
               IEnumerator actualEnum = actEnumerable.GetEnumerator();

               for (int count = 0; ; count++)
               {
                  bool expectedHasData = expectedEnum.MoveNext();
                  bool actualHasData = actualEnum.MoveNext();

                  if (!expectedHasData && !actualHasData)
                  {
                     return true;
                  }

                  if (expectedHasData != actualHasData)
                  {
                     return false;
                  }

                  object expectedValue = expectedEnum.Current;
                  object actualValue = actualEnum.Current;

                  if (expectedValue == null && actualValue == null)
                  {
                     continue;
                  }
                  else if (expectedValue != null && expectedValue.Equals(actualValue))
                  {
                     continue;
                  }
                  else
                  {
                     return false;
                  }
               }
            }
            else if (!expectedValue.Equals(decodedDescribedType.Described))
            {
               return false;
            }
         }

         if (decoded < (incoming.Length - incoming.Position) && !permitTrailingBytes)
         {
            unexpectedTrailingBytes = true;
            return false;
         }

         return true;
      }
   }
}