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

using System.Linq;
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec;
using Apache.Qpid.Proton.Test.Driver.Codec.Impl;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   /// <summary>
   /// Data section type matcher
   /// </summary>
   public sealed class PartialDataSectionMatcher : TypeSafeMatcher<Stream>
   {
      private static readonly Symbol DescriptorSymbol = new Symbol("amqp:data:binary");
      private static readonly ulong DescriptorCode = 0x0000000000000075UL;

      private readonly bool expectDataSectionPreamble;
      private readonly byte[] expectedValue;
      private readonly int expectedEncodedSize;
      private bool expectTrailingBytes;
      private string decodingErrorDescription;
      private bool unexpectedTrailingBytes;

      /**
       * @param expectedEncodedSize
       *        the actual encoded size the Data section binary should eventually
       *        receive once all split frame transfers have arrived.
       * @param expectedValue
       *        the value that is expected to be IN the received {@link Data}
       */
      public PartialDataSectionMatcher(int expectedEncodedSize, byte[] expectedValue) : this(expectedEncodedSize, expectedValue, true)
      {
      }

      /**
       * @param expectedValue
       *        the value that is expected to be IN the received {@link Data}
       */
      public PartialDataSectionMatcher(byte[] expectedValue) : this(-1, expectedValue, false)
      {
      }

      private PartialDataSectionMatcher(int expectedEncodedSize, byte[] expectedValue, bool expectDataSectionPreamble)
      {
         this.expectedValue = expectedValue;
         this.expectedEncodedSize = expectedEncodedSize;
         this.expectDataSectionPreamble = expectDataSectionPreamble;
      }

      public bool TrailingBytesExpected
      {
         get => expectTrailingBytes;
         set => expectTrailingBytes = value;
      }

      internal byte[] ExpectedValue => expectedValue;

      protected override bool MatchesSafely(Stream receivedBinary)
      {
         if (expectDataSectionPreamble)
         {
            object descriptor = ReadDescribedTypeEncoding(receivedBinary);

            if (!(DescriptorCode.Equals(descriptor) || DescriptorSymbol.Equals(descriptor)))
            {
               return false;
            }

            // Should be a Binary AMQP type with a length value and possibly some bytes
            EncodingCodes encodingCode = receivedBinary.ReadEncodingCode();
            int binaryEncodedSize;

            if (encodingCode == EncodingCodes.VBin8)
            {
               binaryEncodedSize = receivedBinary.ReadUnsignedByte();
            }
            else if (encodingCode == EncodingCodes.VBin32)
            {
               binaryEncodedSize = receivedBinary.ReadInt();
            }
            else
            {
               decodingErrorDescription = "Expected to read a Binary Type but read encoding code: " + encodingCode;
               return false;
            }

            if (binaryEncodedSize != expectedEncodedSize)
            {
               decodingErrorDescription = "Expected encoded Binary to indicate size of: " + expectedEncodedSize + ", " +
                                          "but read an encoded size of: " + binaryEncodedSize;
               return false;
            }
         }

         if (expectedValue != null)
         {
            byte[] payload = receivedBinary.ReadBytes(expectedValue.Length);
            if (!expectedValue.SequenceEqual(payload))
            {
               return false;
            }
         }

         if (receivedBinary.IsReadable() && !TrailingBytesExpected)
         {
            unexpectedTrailingBytes = true;
            return false;
         }
         else
         {
            return true;
         }
      }

      protected override void DescribeMismatchSafely(Stream item, IDescription mismatchDescription)
      {
         mismatchDescription.AppendText("\nActual encoded form: ").AppendValue(item);

         if (decodingErrorDescription != null)
         {
            mismatchDescription.AppendText("\nExpected descriptor: ")
                               .AppendValue(DescriptorSymbol)
                               .AppendText(" / ")
                               .AppendValue(DescriptorCode);
            mismatchDescription.AppendText("\nError that failed the validation: ").AppendValue(decodingErrorDescription);
         }

         if (unexpectedTrailingBytes)
         {
            mismatchDescription.AppendText("\nUnexpected trailing bytes in provided bytes after decoding!");
         }
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("a partial Binary encoding of a Data section that wraps")
                    .AppendText(" an incomplete Binary of eventual size {").AppendValue(expectedEncodedSize)
                    .AppendText("}").AppendText(" containing: ").AppendValue(ExpectedValue);
      }

      private object ReadDescribedTypeEncoding(Stream data)
      {
         EncodingCodes encodingCode = data.ReadEncodingCode();

         if (encodingCode == EncodingCodes.DescribedTypeIndicator)
         {
            encodingCode = data.ReadEncodingCode();
            switch (encodingCode)
            {
               case EncodingCodes.ULong0:
                  return 0;
               case EncodingCodes.SmallULong:
                  return (ulong)data.ReadUnsignedByte();
               case EncodingCodes.ULong:
                  return (ulong)data.ReadUnsignedLong();
               case EncodingCodes.Sym8:
                  return ReadSymbol8(data);
               case EncodingCodes.Sym32:
                  return ReadSymbol32(data);
               default:
                  decodingErrorDescription = "Expected Unsigned Long or Symbol type but found encoding: " + encodingCode;
                  break;
            }
         }
         else
         {
            decodingErrorDescription = "Expected to read a Described Type but read encoding code: " + encodingCode;
         }

         return null;
      }

      private static Symbol ReadSymbol32(Stream buffer)
      {
         int length = buffer.ReadInt();

         if (length == 0)
         {
            return new Symbol("");
         }
         else
         {
            byte[] symbolBytes = new byte[length];
            buffer.Read(symbolBytes);

            return new Symbol(symbolBytes);
         }
      }

      private static Symbol ReadSymbol8(Stream buffer)
      {
         int length = buffer.ReadUnsignedByte();

         if (length == 0)
         {
            return new Symbol("");
         }
         else
         {
            byte[] symbolBytes = new byte[length];
            buffer.Read(symbolBytes);

            return new Symbol(symbolBytes);
         }
      }
   }
}