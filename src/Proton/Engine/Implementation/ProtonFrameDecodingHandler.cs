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
using System.Threading;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types.Security;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Frame encoder for outgoing AMQP and SASL performatives and Headers
   /// </summary>
   public sealed class ProtonFrameDecodingHandler : IEngineHandler, ISaslPerformativeHandler<IEngineHandlerContext>
   {
      /// <summary>
      /// Frame type indicator for AMQP protocol frames.
      /// </summary>
      public static readonly byte AMQP_FRAME_TYPE = (byte)0;

      /// <summary>
      /// Frame type indicator for SASL protocol frames.
      /// </summary>
      public static readonly byte SASL_FRAME_TYPE = (byte)1;

      /// <summary>
      /// The specified encoding size for the frame size value of each encoded frame.
      /// </summary>
      public static readonly int FRAME_SIZE_BYTES = 4;

      private readonly AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> framePool =
         AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>.IncomingEnvelopePool();

      private IDecoder decoder;
      private IDecoderState decoderState;
      private FrameParserStage stage;
      private ProtonEngine engine;
      private ProtonEngineConfiguration configuration;

      // Parser stages used during the parsing process
      private readonly FrameSizeParsingStage frameSizeParser;
      private readonly FrameBufferingStage frameBufferingStage;
      private readonly FrameBodyParsingStage frameBodyParsingStage;

      public ProtonFrameDecodingHandler()
      {
         stage = new HeaderParsingStage(this);
         frameSizeParser = new FrameSizeParsingStage(this);
         frameBufferingStage = new FrameBufferingStage(this);
         frameBodyParsingStage = new FrameBodyParsingStage(this);
      }

      public void HandlerAdded(IEngineHandlerContext context)
      {
         engine = (ProtonEngine)context.Engine;
         configuration = (ProtonEngineConfiguration)engine.Configuration;
      }

      public void EngineFailed(IEngineHandlerContext context, EngineFailedException failure)
      {
         TransitionToErrorStage(failure);
         context.FireFailed(failure);
      }

      public void HandleRead(IEngineHandlerContext context, IProtonBuffer buffer)
      {
         try
         {
            // Parses in-incoming data and emit events for complete frames before returning, caller
            // should ensure that the input buffer is drained into the engine or stop if the engine
            // has changed to a non-writable state.
            while (buffer.IsReadable && engine.IsWritable)
            {
               stage.Parse(context, buffer);
            }
         }
         catch (FrameDecodingException frameEx)
         {
            TransitionToErrorStage(frameEx).FireError(context);
         }
         catch (ProtonException pex)
         {
            TransitionToErrorStage(pex).FireError(context);
         }
         catch (DecodeException ex)
         {
            TransitionToErrorStage(new FrameDecodingException(ex.Message, ex)).FireError(context);
         }
         catch (Exception error)
         {
            TransitionToErrorStage(new ProtonException(error.Message, error)).FireError(context);
         }
      }

      public void HandleRead(IEngineHandlerContext context, SaslEnvelope envelope)
      {
         envelope.Body.Invoke(this, context);
         ((ProtonEngineHandlerContext)context).InterestMask = ProtonEngineHandlerContext.HANDLER_READS;
         context.FireRead(envelope);
      }

      public void HandleWrite(IEngineHandlerContext context, SaslEnvelope envelope)
      {
         envelope.Invoke(this, context);
         ((ProtonEngineHandlerContext)context).InterestMask = ProtonEngineHandlerContext.HANDLER_READS;
         context.FireWrite(envelope);
      }

      public void HandleOutcome(SaslOutcome saslOutcome, IEngineHandlerContext context)
      {
         // When we have read or written a SASL Outcome the next value to be read
         // should be an AMQP Header to begin the next phase of the connection.
         this.stage = new HeaderParsingStage(this);
      }

      #region Parsing stage transition handling

      private FrameParserStage TransitionToFrameSizeParsingStage()
      {
         return stage = frameSizeParser.Reset(0);
      }

      private FrameParserStage TransitionToFrameBufferingStage(int length)
      {
         return stage = frameBufferingStage.Reset(length);
      }

      private FrameParserStage InitializeFrameBodyParsingStage(int length)
      {
         return stage = frameBodyParsingStage.Reset(length);
      }

      private ParsingErrorStage TransitionToErrorStage(ProtonException error)
      {
         if (!(stage is ParsingErrorStage))
         {
            // TODO : LOG.trace("Frame decoder encountered error: ", error);
            stage = new ParsingErrorStage(this, error);
         }

         return (ParsingErrorStage)stage;
      }

      internal abstract class FrameParserStage
      {
         protected readonly ProtonFrameDecodingHandler handler;

         internal FrameParserStage(ProtonFrameDecodingHandler handler)
         {
            this.handler = handler;
         }

         /// <summary>
         /// Parse the incoming data and provide events to the parent Transport
         /// based on the contents of that data.
         /// </summary>
         /// <param name="context">Event context</param>
         /// <param name="input">The input buffer</param>
         internal abstract void Parse(IEngineHandlerContext context, IProtonBuffer input);

         /// <summary>
         /// Reset the stage to its defaults for a new cycle of parsing.
         /// </summary>
         /// <param name="length">Length value for the next section to parse</param>
         /// <returns></returns>
         internal abstract FrameParserStage Reset(int length);

      }

      private class HeaderParsingStage : FrameParserStage
      {
         private readonly byte[] headerBytes = new byte[AmqpHeader.HeaderSizeBytes];

         private int headerByte;

         public HeaderParsingStage(ProtonFrameDecodingHandler handler) : base(handler)
         {
         }

         internal override void Parse(IEngineHandlerContext context, IProtonBuffer incoming)
         {
            while (incoming.IsReadable && headerByte < AmqpHeader.HeaderSizeBytes)
            {
               byte nextByte = incoming.ReadUnsignedByte();
               try
               {
                  AmqpHeader.ValidateByte(headerByte, nextByte);
               }
               catch (ArgumentException iae)
               {
                  throw new MalformedAMQPHeaderException(
                      String.Format("Error on validation of header byte {0} with value of {1}", headerByte, nextByte), iae);
               }
               headerBytes[headerByte++] = nextByte;
            }

            if (headerByte == AmqpHeader.HeaderSizeBytes)
            {
               // Construct a new Header from the read bytes which will validate the contents
               AmqpHeader header = new AmqpHeader(headerBytes);

               // Transition to parsing the frames if any pipelined into this buffer.
               handler.TransitionToFrameSizeParsingStage();

               if (header.IsSaslHeader())
               {
                  handler.decoder = CodecFactory.SaslDecoder;
                  handler.decoderState = handler.decoder.NewDecoderState();
                  context.FireRead(HeaderEnvelope.SASL_HEADER_ENVELOPE);
               }
               else
               {
                  handler.decoder = CodecFactory.Decoder;
                  handler.decoderState = handler.decoder.NewDecoderState();
                  context.FireRead(HeaderEnvelope.AMQP_HEADER_ENVELOPE);
               }
            }
         }

         internal override HeaderParsingStage Reset(int frameSize)
         {
            headerByte = 0;
            return this;
         }
      }

      internal class FrameSizeParsingStage : FrameParserStage
      {
         private static readonly uint MinFrameSizeValue = 8;

         private int frameSize;
         private int multiplier = FRAME_SIZE_BYTES;

         public FrameSizeParsingStage(ProtonFrameDecodingHandler handler) : base(handler)
         {
         }

         internal override void Parse(IEngineHandlerContext context, IProtonBuffer input)
         {
            while (input.IsReadable)
            {
               frameSize |= (input.ReadUnsignedByte() << --multiplier * 8);
               if (multiplier == 0)
               {
                  break;
               }
            }

            if (multiplier == 0)
            {
               ValidateFrameSize();

               int length = frameSize - FRAME_SIZE_BYTES;

               if (input.ReadableBytes < length)
               {
                  handler.TransitionToFrameBufferingStage(length);
               }
               else
               {
                  handler.InitializeFrameBodyParsingStage(length);
               }

               handler.stage.Parse(context, input);
            }
         }

         private void ValidateFrameSize()
         {
            if ((uint)frameSize < MinFrameSizeValue)
            {
               throw new FrameDecodingException(String.Format(
                    "specified frame size {0} smaller than minimum frame header size 8", frameSize));
            }

            if ((uint)frameSize > handler.configuration.InboundMaxFrameSize)
            {
               throw new FrameDecodingException(String.Format(
                   "specified frame size {0} larger than maximum frame size {1}",
                   (uint)frameSize, handler.configuration.InboundMaxFrameSize));
            }
         }

         internal override FrameSizeParsingStage Reset(int frameSize)
         {
            multiplier = FRAME_SIZE_BYTES;
            this.frameSize = frameSize;
            return this;
         }
      }


      internal class FrameBufferingStage : FrameParserStage
      {
         private IProtonBuffer buffer;

         public FrameBufferingStage(ProtonFrameDecodingHandler handler) : base(handler)
         {
         }

         internal override void Parse(IEngineHandlerContext context, IProtonBuffer input)
         {
            if (input.ReadableBytes < buffer.WritableBytes)
            {
               buffer.WriteBytes(input);
            }
            else
            {
               input.CopyInto(input.ReadOffset, input, input.WriteOffset, buffer.WritableBytes);

               // Advance the buffer offsets to reflect what was copied.
               input.ReadOffset += buffer.WritableBytes;
               buffer.WriteOffset += buffer.WritableBytes;

               // Now we can consume the buffer frame body.
               handler.InitializeFrameBodyParsingStage((int)buffer.ReadableBytes);
               try
               {
                  handler.stage.Parse(context, buffer);
               }
               finally
               {
                  buffer = null;
               }
            }
         }

         internal override FrameBufferingStage Reset(int length)
         {
            buffer = ProtonByteBufferAllocator.Instance.Allocate(length, length);
            return this;
         }
      }

      internal class FrameBodyParsingStage : FrameParserStage
      {
         private int length;

         public FrameBodyParsingStage(ProtonFrameDecodingHandler handler) : base(handler)
         {
         }

         internal override void Parse(IEngineHandlerContext context, IProtonBuffer input)
         {
            int dataOffset = (input.ReadUnsignedByte() << 2) & 0x3FF;
            int frameSize = length + FRAME_SIZE_BYTES;

            ValidateDataOffset(dataOffset, frameSize);

            byte type = input.ReadUnsignedByte();
            ushort channel = input.ReadUnsignedShort();

            // Skip over the extended header if present (i.e offset > 8)
            if (dataOffset != 8)
            {
               input.ReadOffset += dataOffset - 8;
            }

            long frameBodySize = frameSize - dataOffset;

            IProtonBuffer payload = null;
            Object val = null;

            if (frameBodySize > 0)
            {
               long startReadIndex = input.ReadOffset;
               val = handler.decoder.ReadObject(input, handler.decoderState);

               // Copy the payload portion of the incoming bytes for now as the incoming may be
               // from a wrapped pooled buffer and for now we have no way of retaining or otherwise
               // ensuring that the buffer remains ours.  Since we might want to store received
               // data at a client level and decode later we could end up losing the data to reuse
               // if it was pooled.
               if (input.IsReadable)
               {
                  long payloadSize = frameBodySize - (input.ReadOffset - startReadIndex);
                  // Check that the remaining bytes aren't part of another frame.
                  if (payloadSize > 0)
                  {
                     payload = handler.configuration.BufferAllocator.Allocate(payloadSize, payloadSize);

                     input.CopyInto(input.ReadOffset, payload, 0, payloadSize);

                     input.ReadOffset += payloadSize;
                     payload.WriteOffset += payloadSize;
                  }
               }
            }
            else
            {
               handler.TransitionToFrameSizeParsingStage();
               context.FireRead(EmptyEnvelope.Instance);
               return;
            }

            if (type == AMQP_FRAME_TYPE)
            {
               IPerformative performative = (IPerformative)val;
               IncomingAmqpEnvelope frame = handler.framePool.Take(performative, channel, payload);
               handler.TransitionToFrameSizeParsingStage();
               context.FireRead(frame);
            }
            else if (type == SASL_FRAME_TYPE)
            {
               ISaslPerformative performative = (ISaslPerformative)val;
               SaslEnvelope saslFrame = new SaslEnvelope(performative);
               handler.TransitionToFrameSizeParsingStage();
               // Ensure we process transition from SASL to AMQP header state
               handler.HandleRead(context, saslFrame);
            }
            else
            {
               throw new FrameDecodingException(String.Format("unknown frame type: {0}", type));
            }
         }

         private void ValidateDataOffset(int dataOffset, int frameSize)
         {
            if (dataOffset < 8)
            {
               throw new FrameDecodingException(String.Format(
                   "specified frame data offset {0} smaller than minimum frame header size {1}", dataOffset, 8));
            }

            if (dataOffset > frameSize)
            {
               throw new FrameDecodingException(String.Format(
                   "specified frame data offset {0} larger than the frame size {1}", dataOffset, frameSize));
            }
         }

         internal override FrameBodyParsingStage Reset(int length)
         {
            this.length = length;
            return this;
         }
      }

      internal class ParsingErrorStage : FrameParserStage
      {
         private readonly ProtonException parsingError;

         public ParsingErrorStage(ProtonFrameDecodingHandler handler, ProtonException parsingError) : base(handler)
         {
            this.parsingError = parsingError;
         }

         internal void FireError(IEngineHandlerContext context)
         {
            throw parsingError;
         }

         internal override void Parse(IEngineHandlerContext context, IProtonBuffer input)
         {
            throw new FrameDecodingException(parsingError.Message, parsingError);
         }

         internal override ParsingErrorStage Reset(int length)
         {
            return this;
         }
      }

      #endregion
   }
}