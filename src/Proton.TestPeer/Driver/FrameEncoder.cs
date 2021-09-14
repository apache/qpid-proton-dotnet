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

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Encodes outgoing AMQP frames
   /// </summary>
   public sealed class FrameEncoder
   {
      public static readonly byte AMQP_FRAME_TYPE = (byte)0;
      public static readonly byte SASL_FRAME_TYPE = (byte)1;

      private static readonly uint AMQP_PERFORMATIVE_PAD = 512;
      private static readonly uint FRAME_HEADER_SIZE = 8;

      private static readonly int FRAME_DOFF_SIZE = 2;

      private readonly AMQPTestDriver driver;

      private readonly ICodec codec = CodecFactory.Create();

      public FrameEncoder(AMQPTestDriver driver)
      {
         this.driver = driver;
      }

      public void HandleWrite(Stream stream, IDescribedType performative, ushort channel, byte[] payload, Action payloadToLarge)
      {
         WriteFrame(stream, performative, payload, AMQP_FRAME_TYPE, channel, driver.OutboundMaxFrameSize, payloadToLarge);
      }

      public void HandleWrite(Stream stream, IDescribedType performative, ushort channel)
      {
         WriteFrame(stream, performative, null, SASL_FRAME_TYPE, 0, driver.OutboundMaxFrameSize, null);
      }

      private void WriteFrame(Stream stream, IDescribedType performative, byte[] payload, byte frameType, ushort channel, uint maxFrameSize, Action onPayloadTooLarge)
      {
         long startIndex = stream.Position;
         uint outputBufferSize = AMQP_PERFORMATIVE_PAD + (payload != null ? (uint)payload.Length : 0u);
         uint performativeSize = WritePerformative(stream, performative, payload, maxFrameSize, onPayloadTooLarge);
         uint capacity = maxFrameSize > 0 ? maxFrameSize - performativeSize : Int32.MaxValue;
         uint payloadSize = (uint)Math.Min(payload == null ? 0 : payload.Length, capacity);

         if (payloadSize > 0)
         {
            stream.Write(payload, 0, (int)payloadSize);
         }

         long endIndex = stream.Position;

         stream.Seek(0, SeekOrigin.Begin);

         BinaryWriter writer = new BinaryWriter(stream);

         writer.Write((int)(endIndex - startIndex));
         writer.Write((byte)FRAME_DOFF_SIZE);
         writer.Write((byte)frameType);
         writer.Write(channel);
      }

      private uint WritePerformative(Stream stream, IDescribedType performative, Span<byte> payload, uint maxFrameSize, Action onPayloadTooLarge)
      {
         stream.Seek(stream.Position + FRAME_HEADER_SIZE, SeekOrigin.Current);

         long encodedSize = 0;
         long startIndex = stream.Position;

         if (performative != null)
         {
            try
            {
               codec.PutDescribedType(performative);
               encodedSize = codec.Encode(new BinaryWriter(stream));
            }
            finally
            {
               codec.Clear();
            }
         }

         long performativeSize = stream.Position - startIndex;

         if (performativeSize != encodedSize)
         {
            throw new InvalidOperationException(String.Format(
                "Unable to encode performative {0} of {1} bytes into provided proton buffer, only wrote {2} bytes",
                performative, performativeSize, encodedSize));
         }

         if (onPayloadTooLarge != null && maxFrameSize > 0 && payload != null && (payload.Length + performativeSize) > maxFrameSize)
         {
            // Next iteration will re-encode the frame body again with updates from the <payload-to-large>
            // handler and then we can move onto the body portion.
            onPayloadTooLarge.Invoke();
            stream.Seek(startIndex, SeekOrigin.Begin);
            performativeSize = WritePerformative(stream, performative, payload, maxFrameSize, null);
         }

         return (uint)performativeSize;
      }
   }
}