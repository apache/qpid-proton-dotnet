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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec;
using Apache.Qpid.Proton.Test.Driver.Codec.Impl;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   /// <summary>
   /// Data Section matcher that can be used with multiple expectTransfer calls to match a larger
   /// given payload block to the contents of one or more incoming Data sections split across multiple
   /// transfer frames and or multiple Data Sections within those transfer frames.
   /// </summary>
   public sealed class CompositingDataSectionMatcher : TypeSafeMatcher<Stream>
   {
      private static readonly Symbol DescriptorSymbol = new Symbol("amqp:data:binary");
      private static readonly ulong DescriptorCode = 0x0000000000000075UL;

      private readonly int expectedValueSize;
      private readonly byte[] expectedValue;
      private readonly MemoryStream expectedValueStream;

      private bool expectTrailingBytes;
      private String decodingErrorDescription;

      // State data used during validation of the composite data values
      private bool unexpectedTrailingBytes;
      private bool expectDataSectionPreamble = true;
      private int expectedCurrentDataSectionBytes = -1;
      private int expectedRemainingBytes;

      /**
       * @param expectedValue
       *        the value that is expected to be IN the received {@link Data}
       */
      public CompositingDataSectionMatcher(byte[] expectedValue)
      {
         this.expectedValue = expectedValue;
         this.expectedValueStream = new MemoryStream(expectedValue);
         this.expectedRemainingBytes = this.expectedValue.Length;
         this.expectedValueSize = this.expectedRemainingBytes;
      }

      public bool ExpectTrailingBytes
      {
         get => expectTrailingBytes;
         set => expectTrailingBytes = value;
      }

      public byte[] ExpectedValue => expectedValue;

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

            if (encodingCode == EncodingCodes.VBin8)
            {
               expectedCurrentDataSectionBytes = receivedBinary.ReadUnsignedByte();
            }
            else if (encodingCode == EncodingCodes.VBin32)
            {
               expectedCurrentDataSectionBytes = receivedBinary.ReadInt();
            }
            else
            {
               decodingErrorDescription = "Expected to read a Binary Type but read encoding code: " + encodingCode;
               return false;
            }

            if (expectedCurrentDataSectionBytes > expectedRemainingBytes)
            {
               decodingErrorDescription = "Expected encoded Binary to indicate size of: " + expectedRemainingBytes + ", " +
                                          "or less but read an encoded size of: " + expectedCurrentDataSectionBytes;
               return false;
            }

            expectDataSectionPreamble = false;  // We got the current preamble
         }

         if (expectedRemainingBytes != 0)
         {
            int currentChunkSize = Math.Min(expectedCurrentDataSectionBytes, (int)receivedBinary.ReadableBytes());

            byte[] expectedValueChunk = expectedValueStream.ReadBytes(currentChunkSize);
            byte[] currentChunk = receivedBinary.ReadBytes(currentChunkSize);

            if (!Array.Equals(expectedValueChunk, currentChunk))
            {
               return false;
            }

            expectedRemainingBytes -= currentChunkSize;
            expectedCurrentDataSectionBytes -= currentChunkSize;

            if (expectedRemainingBytes != 0 && expectedCurrentDataSectionBytes == 0)
            {
               expectDataSectionPreamble = true;
               expectedCurrentDataSectionBytes = -1;
            }
         }

         if (expectedRemainingBytes == 0 && receivedBinary.IsReadable() && !ExpectTrailingBytes)
         {
            unexpectedTrailingBytes = true;
            return false;
         }
         else
         {
            return true;
         }
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("a complete Binary encoding of a Data section that wraps")
                    .AppendText(" an collection of bytes of eventual size {").AppendValue(expectedValueSize)
                    .AppendText("}").AppendText(" containing: ").AppendValue(expectedValue);
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
                  return data.ReadUnsignedByte();
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

      private Symbol ReadSymbol32(Stream buffer)
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

      private Symbol ReadSymbol8(Stream buffer)
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
