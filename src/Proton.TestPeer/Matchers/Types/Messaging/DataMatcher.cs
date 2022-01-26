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

using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   /// <summary>
   /// Data section type matcher
   /// </summary>
   public sealed class DataMatcher : AbstractBodySectionMatcher
   {
      /// <summary>
      /// Create a match that expects an Data section encoding with the given payload.
      /// </summary>
      /// <param name="expectedValue">The value that should be encoded in the Data section</param>
      public DataMatcher(byte[] expectedValue) : this(expectedValue, false)
      {
      }

      /// <summary>
      /// Create a match that expects an Data section encoding with the given payload.
      /// </summary>
      /// <param name="expectedValue">The value that should be encoded in the Data section</param>
      /// <param name="permitTrailingBytes">Expect more bytes in the message encoding</param>
      public DataMatcher(byte[] expectedValue, bool permitTrailingBytes) :
         base(Data.DESCRIPTOR_SYMBOL, Data.DESCRIPTOR_CODE, new Binary(expectedValue), permitTrailingBytes)
      {
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("a Binary encoding of an Data that wraps: ").AppendValue(ExpectedValue);
      }
   }
}