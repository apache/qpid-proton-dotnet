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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Frame encoder for outgoing AMQP and SASL performatives and Headers
   /// </summary>
   public sealed class ProtonFrameEncodingHandler : IEngineHandler
   {
      /**
       * Frame type indicator for AMQP protocol frames.
       */
      public static readonly byte AMQP_FRAME_TYPE = (byte)0;

      /**
       * Frame type indicator for SASL protocol frames.
       */
      public static readonly byte SASL_FRAME_TYPE = (byte)1;

      private static readonly uint AMQP_PERFORMATIVE_PAD = 256;
      private static readonly uint FRAME_HEADER_SIZE = 8;
      private static readonly uint FRAME_DOFF_SIZE = 2;

      private static readonly uint FRAME_START_BYTE = 0;
      private static readonly uint FRAME_DOFF_BYTE = 4;
      private static readonly uint FRAME_TYPE_BYTE = 5;
      private static readonly uint FRAME_CHANNEL_BYTE = 6;

      private static readonly byte[] SASL_FRAME_HEADER = new byte[] { 0, 0, 0, 0, (byte)FRAME_DOFF_SIZE, SASL_FRAME_TYPE, 0, 0 };

      private static readonly IProtonBuffer EMPTY_BUFFER = ProtonByteBufferAllocator.Instance.Wrap(new byte[0]);

      private readonly IEncoder saslEncoder = CodecFactory.SaslEncoder;
      private readonly IEncoderState saslEncoderState = CodecFactory.SaslEncoder.NewEncoderState();
      private readonly IEncoder amqpEncoder = CodecFactory.Encoder;
      private readonly IEncoderState amqpEncoderState = CodecFactory.SaslEncoder.NewEncoderState();

      private ProtonEngine engine;
      private ProtonEngineConfiguration configuration;

      public void HandlerAdded(IEngineHandlerContext context)
      {
         engine = (ProtonEngine)context.Engine;
         configuration = (ProtonEngineConfiguration)engine.Configuration;

         ((ProtonEngineHandlerContext)context).InterestMask = ProtonEngineHandlerContext.HANDLER_WRITES;
      }

      public void HandleWrite(IEngineHandlerContext context, HeaderEnvelope envelope)
      {
         context.FireWrite(envelope.Body.Buffer, null);
      }

      public void HandleWrite(IEngineHandlerContext context, SaslEnvelope envelope)
      {
         IProtonBuffer output = configuration.BufferAllocator.OutputBuffer(AMQP_PERFORMATIVE_PAD, configuration.OutboundMaxFrameSize);

         output.EnsureWritable(FRAME_HEADER_SIZE);
         output.WriteBytes(SASL_FRAME_HEADER);

         try
         {
            saslEncoder.WriteObject(output, saslEncoderState, envelope.Body);
         }
         catch (EncodeException ex)
         {
            throw new FrameEncodingException(ex);
         }
         finally
         {
            saslEncoderState.Reset();
         }

         context.FireWrite(output.SetUnsignedInt(FRAME_START_BYTE, (uint)output.ReadableBytes), null);
      }

      public void HandleWrite(IEngineHandlerContext context, OutgoingAmqpEnvelope envelope)
      {
         IProtonBuffer payload = envelope.Payload ?? EMPTY_BUFFER;
         uint maxFrameSize = configuration.OutboundMaxFrameSize;
         uint outputBufferSize = (uint)Math.Min(maxFrameSize, AMQP_PERFORMATIVE_PAD + payload.ReadableBytes);
         IProtonBuffer output = configuration.BufferAllocator.OutputBuffer(outputBufferSize, maxFrameSize);

         output.EnsureWritable(outputBufferSize);

         WritePerformative(output, amqpEncoder, amqpEncoderState, envelope.Body);

         long remainingSpace = maxFrameSize - output.WriteOffset;

         if (payload.ReadableBytes > remainingSpace)
         {
            envelope.HandlePayloadToLarge();

            WritePerformative(output, amqpEncoder, amqpEncoderState, envelope.Body);

            payload.CopyInto(payload.ReadOffset, output, output.WriteOffset, remainingSpace);
            payload.ReadOffset += remainingSpace;
         }
         else
         {
            output.WriteBytes(payload);
         }

         // Now fill in the frame header with the specified information
         output.SetUnsignedInt(FRAME_START_BYTE, (uint)output.ReadableBytes);
         output.SetUnsignedByte(FRAME_DOFF_BYTE, (byte)FRAME_DOFF_SIZE);
         output.SetUnsignedByte(FRAME_TYPE_BYTE, AMQP_FRAME_TYPE);
         output.SetUnsignedShort(FRAME_CHANNEL_BYTE, envelope.Channel);

         context.FireWrite(output, () => envelope.HandleOutgoingFrameWriteComplete());
      }

      private static void WritePerformative(IProtonBuffer target, IEncoder encoder, IEncoderState state, IPerformative performative)
      {
         target.EnsureWritable(FRAME_HEADER_SIZE);
         target.WriteOffset = FRAME_HEADER_SIZE;

         try
         {
            encoder.WriteObject(target, state, performative);
         }
         catch (EncodeException ex)
         {
            throw new FrameEncodingException(ex);
         }
         finally
         {
            state.Reset();
         }
      }
   }
}