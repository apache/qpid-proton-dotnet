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
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Decodes incoming AMQP frames
   /// </summary>
   public sealed class FrameDecoder
   {
      public static readonly byte AMQP_FRAME_TYPE = (byte)0;
      public static readonly byte SASL_FRAME_TYPE = (byte)1;

      public static readonly int FRAME_SIZE_BYTES = 4;

      private readonly IFrameHandler frameHandler;
      private readonly ICodec codec = CodecFactory.Create();
      private FrameParserStage stage;

      // Parser stages used during the parsing process
      private readonly FrameParserStage frameSizeParser;
      private readonly FrameParserStage frameBufferingStage;
      private readonly FrameParserStage frameBodyParsingStage;

      public FrameDecoder(IFrameHandler frameHandler)
      {
         this.frameHandler = frameHandler;
         this.stage = new HeaderParsingStage(this);
         this.frameSizeParser = new FrameSizeParsingStage(this);
         this.frameBufferingStage = new FrameBufferingStage(this);
         this.frameBodyParsingStage = new FrameBodyParsingStage(this);
      }

      /// <summary>
      /// Resets the parser back to the expect a header state.
      /// </summary>
      public void ResetToExpectingHeader()
      {
         this.stage = new HeaderParsingStage(this);
      }

      /// <summary>
      /// Accepts an incoming stream of bytes and parses a single frame from that stream
      /// in order to signal the test driver of some event.
      /// </summary>
      /// <param name="stream"></param>
      public void Ingest(Stream stream)
      {
         try
         {
            // Parses in-incoming data and emit one complete frame before returning, caller should
            // ensure that the input buffer is drained into the engine or stop if the engine
            // has changed to a non-writable state.
            stage.Parse(stream);
         }
         catch (AssertionError ex)
         {
            TransitionToErrorStage(ex);
            throw;
         }
         catch (Exception throwable)
         {
            AssertionError error = new AssertionError("Frame decode failed.", throwable);
            TransitionToErrorStage(error);
            throw error;
         }
      }

      #region Private frame decoder implementation

      private FrameParserStage TransitionToFrameSizeParsingStage()
      {
         return stage = frameSizeParser.Reset(0);
      }

      private FrameParserStage TransitionToFrameBufferingStage(uint frameSize)
      {
         return stage = frameBufferingStage.Reset(frameSize);
      }

      private FrameParserStage InitializeFrameBodyParsingStage(uint frameSize)
      {
         return stage = frameBodyParsingStage.Reset(frameSize);
      }

      private ParsingErrorStage TransitionToErrorStage(Exception error)
      {
         if (stage is not ParsingErrorStage)
         {
            stage = new ParsingErrorStage(this, error);
         }

         return (ParsingErrorStage)stage;
      }

      #endregion

      #region Frame parsing types

      internal abstract class FrameParserStage
      {
         protected FrameDecoder decoder;
         protected IFrameHandler frameHandler;

         internal FrameParserStage(FrameDecoder decoder)
         {
            this.decoder = decoder;
            this.frameHandler = decoder.frameHandler;
         }

         internal abstract void Parse(Stream input);

         internal abstract FrameParserStage Reset(uint frameSize);
      }

      internal class HeaderParsingStage : FrameParserStage
      {
         private readonly byte[] headerBytes = new byte[AMQPHeader.HEADER_SIZE_BYTES];

         private int headerByte;

         public HeaderParsingStage(FrameDecoder decoder) : base(decoder)
         {
         }

         internal override void Parse(Stream incoming)
         {
            int nextByte = 0;
            while ((nextByte = incoming.ReadByte()) != -1 && headerByte < AMQPHeader.HEADER_SIZE_BYTES)
            {
               headerBytes[headerByte++] = ((byte)nextByte);
            }

            if (headerByte == AMQPHeader.HEADER_SIZE_BYTES)
            {
               // Construct a new Header from the read bytes which will validate the contents
               AMQPHeader header = new AMQPHeader(headerBytes);

               // Transition to parsing the frames if any pipelined into this buffer.
               decoder.TransitionToFrameSizeParsingStage();

               if (header.IsSaslHeader)
               {
                  frameHandler.HandleHeader(AMQPHeader.SASLHeader);
               }
               else
               {
                  frameHandler.HandleHeader(AMQPHeader.Header);
               }
            }
         }

         internal override FrameParserStage Reset(uint frameSize)
         {
            headerByte = 0;
            return this;
         }
      }

      internal class FrameSizeParsingStage : FrameParserStage
      {
         private uint frameSize;
         private int multiplier = FRAME_SIZE_BYTES;

         public FrameSizeParsingStage(FrameDecoder decoder) : base(decoder)
         {
         }

         internal override void Parse(Stream input)
         {
            int nextByte = 0;
            while ((nextByte = input.ReadByte()) != -1)
            {
               frameSize |= (uint)((nextByte & 0xFF) << (--multiplier * sizeof(byte)));
               if (multiplier == 0)
               {
                  break;
               }
            }

            if (multiplier == 0)
            {
               ValidateFrameSize();

               // Normalize the frame size to the reminder portion
               uint length = (uint)(frameSize - FRAME_SIZE_BYTES);

               if ((input.Length - input.Position) < length)
               {
                  decoder.TransitionToFrameBufferingStage(length);
               }
               else
               {
                  decoder.InitializeFrameBodyParsingStage(length);
               }

               decoder.stage.Parse(input);
            }
         }

         private void ValidateFrameSize()
         {
            if (frameSize < 8)
            {
               throw new ArgumentException(String.Format(
                    "specified frame size {0} smaller than minimum frame header size 8", frameSize));
            }

            if (frameSize > frameHandler.InboundMaxFrameSize)
            {
               throw new ArgumentOutOfRangeException(String.Format(
                   "specified frame size {0} larger than maximum frame size", frameSize, frameHandler.InboundMaxFrameSize));
            }
         }

         internal override FrameSizeParsingStage Reset(uint frameSize)
         {
            multiplier = FRAME_SIZE_BYTES;
            this.frameSize = frameSize;
            return this;
         }
      }

      internal class FrameBufferingStage : FrameParserStage
      {
         private MemoryStream buffer;

         public FrameBufferingStage(FrameDecoder decoder) : base(decoder)
         {
         }

         internal override void Parse(Stream input)
         {
            if ((input.Length - input.Position) < buffer.Length - buffer.Position)
            {
               input.CopyTo(buffer);
            }
            else
            {
               input.CopyTo(buffer, (int)(buffer.Length - buffer.Position));

               // Now we can consume the buffer frame body.
               decoder.InitializeFrameBodyParsingStage((uint)buffer.Position);

               try
               {
                  decoder.stage.Parse(buffer);
               }
               finally
               {
                  buffer = null;
               }
            }
         }

         internal override FrameBufferingStage Reset(uint frameSize)
         {
            buffer = new MemoryStream((int)frameSize);
            return this;
         }
      }

      internal class FrameBodyParsingStage : FrameParserStage
      {
         private uint frameSize;

         public FrameBodyParsingStage(FrameDecoder decoder) : base(decoder)
         {
         }

         internal override void Parse(Stream input)
         {
            uint dataOffset = (uint)((input.ReadByte() << 2) & 0x3FF);
            uint frameSize = (uint)(this.frameSize + FRAME_SIZE_BYTES);

            ValidateDataOffset(dataOffset, frameSize);

            int type = input.ReadByte() & 0xFF;
            ushort channel = input.ReadUnsignedShort();

            // note that this skips over the extended header if it's present
            if (dataOffset != 8)
            {
               input.Position = input.Position + dataOffset - 8;
            }

            uint frameBodySize = frameSize - dataOffset;

            byte[] payload = null;
            Object val = null;

            if (frameBodySize > 0)
            {
               uint frameBodyStartIndex = (uint)input.Position;

               try
               {
                  decoder.codec.Decode(input);
               }
               catch (Exception e)
               {
                  throw new Exception("Decoder failed reading remote input:", e);
               }

               Codec.DataType dataType = decoder.codec.DataType;
               if (dataType != Codec.DataType.Described)
               {
                  throw new ArgumentException(
                      "Frame body type expected to be " + Codec.DataType.Described + " but was: " + dataType);
               }

               try
               {
                  val = decoder.codec.GetDescribedType();
               }
               finally
               {
                  decoder.codec.Clear();
               }

               // Slice to the known Frame body size and use that as the buffer for any payload once
               // the actual Performative has been decoded.  The implies that the data comprising the
               // performative will be held as long as the payload buffer is kept.
               if (input.Position < input.Length)
               {
                  // Check that the remaining bytes aren't part of another frame.
                  uint payloadSize = (uint)(frameBodySize - (input.Position - frameBodyStartIndex));
                  if (payloadSize > 0)
                  {
                     payload = input.ReadBytes((int)payloadSize);
                  }
               }
            }
            else
            {
               // TODO LOG.trace("{} Read: CH[{}] : {} [{}]", driver.getName(), channel, HeartBeat.INSTANCE, payload);
               decoder.TransitionToFrameSizeParsingStage();
               frameHandler.HandleHeartbeat(frameSize, channel);
               return;
            }

            if (type == AMQP_FRAME_TYPE)
            {
               PerformativeDescribedType performative = (PerformativeDescribedType)val;
               // TODO LOG.trace("{} Read: CH[{}] : {} [{}]", driver.getName(), channel, performative, payload);
               decoder.TransitionToFrameSizeParsingStage();
               frameHandler.HandlePerformative(frameSize, performative, channel, payload);
            }
            else if (type == SASL_FRAME_TYPE)
            {
               SaslDescribedType performative = (SaslDescribedType)val;
               // TODO LOG.trace("{} Read: {} [{}]", driver.getName(), performative, payload);
               decoder.TransitionToFrameSizeParsingStage();
               frameHandler.HandleSaslPerformative(frameSize, performative, channel, payload);
            }
            else
            {
               throw new ArgumentException(String.Format("unknown frame type: {0}", type));
            }
         }

         internal override FrameBodyParsingStage Reset(uint frameSize)
         {
            this.frameSize = frameSize;
            return this;
         }

         private void ValidateDataOffset(uint dataOffset, uint frameSize)
         {
            if (dataOffset < 8)
            {
               throw new ArgumentOutOfRangeException(String.Format(
                   "specified frame data offset {0} smaller than minimum frame header size {1}", dataOffset, 8));
            }

            if (dataOffset > frameSize)
            {
               throw new ArgumentOutOfRangeException(String.Format(
                   "specified frame data offset {0} larger than the frame size {1}", dataOffset, frameSize));
            }
         }
      }

      internal sealed class ParsingErrorStage : FrameParserStage
      {
         private readonly Exception parsingError;

         public ParsingErrorStage(FrameDecoder decoder, Exception parsingError) : base(decoder)
         {
            this.parsingError = parsingError;
         }

         internal override void Parse(Stream input)
         {
            throw parsingError;
         }

         internal override FrameParserStage Reset(uint frameSize)
         {
            return this;
         }
      }

      #endregion
   }
}