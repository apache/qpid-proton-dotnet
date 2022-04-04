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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   public sealed class AMQPHeader
   {
      static readonly byte[] PREFIX = new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P' };

      public static readonly int PROTOCOL_ID_INDEX = 4;
      public static readonly int MAJOR_VERSION_INDEX = 5;
      public static readonly int MINOR_VERSION_INDEX = 6;
      public static readonly int REVISION_INDEX = 7;

      public static readonly byte AMQP_PROTOCOL_ID = 0;
      public static readonly byte SASL_PROTOCOL_ID = 3;

      public static readonly int HEADER_SIZE_BYTES = 8;

      private static readonly AMQPHeader AMQP_HEADER =
        new AMQPHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });

      private static readonly AMQPHeader SASL_HEADER =
        new AMQPHeader(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 3, 1, 0, 0 });

      private byte[] buffer;

      public AMQPHeader() : this(AMQP_HEADER.ToArray())
      {
      }

      public AMQPHeader(byte[] headerBytes)
      {
         SetBuffer((byte[])headerBytes.Clone(), true);
      }

      public AMQPHeader(byte[] headerBytes, bool validate)
      {
         SetBuffer((byte[])headerBytes.Clone(), validate);
      }

      public byte[] Buffer
      {
         get => ToArray();
         set => SetBuffer(value, true);
      }

      public byte[] ToArray() => (byte[])buffer.Clone();

      public byte ByteAt(int i)
      {
         return buffer[i];
      }

      public int ProtocolId => buffer[PROTOCOL_ID_INDEX] & 0xFF;

      public int Major => buffer[MAJOR_VERSION_INDEX] & 0xFF;

      public int Minor => buffer[MINOR_VERSION_INDEX] & 0xFF;

      public int Revision => buffer[REVISION_INDEX] & 0xFF;

      public bool HasValidPrefix => StartsWith(buffer, PREFIX);

      public bool IsSaslHeader => ProtocolId == SASL_PROTOCOL_ID;

      public void Invoke<E>(IHeaderHandler<E> handler, E context)
      {
         if (IsSaslHeader)
         {
            handler.HandleSASLHeader(this, context);
         }
         else
         {
            handler.HandleAMQPHeader(this, context);
         }
      }

      public override string ToString()
      {
         return "AMQP-header:[" + BitConverter.ToString(buffer) + "]";
      }

      #region Static validation methods for header bytes

      public static AMQPHeader Header => AMQP_HEADER;

      public static AMQPHeader SASLHeader => SASL_HEADER;

      /// <summary>
      /// Called to validate a byte according to a given index within the AMQP Header
      ///
      /// If the index is outside the range of the header size an {@link IndexOutOfBoundsException}
      /// will be thrown.
      /// </summary>
      public static void ValidateByte(int index, byte value)
      {
         switch (index)
         {
            case 0:
               ValidatePrefixByte1(value);
               break;
            case 1:
               ValidatePrefixByte2(value);
               break;
            case 2:
               ValidatePrefixByte3(value);
               break;
            case 3:
               ValidatePrefixByte4(value);
               break;
            case 4:
               ValidateProtocolByte(value);
               break;
            case 5:
               ValidateMajorVersionByte(value);
               break;
            case 6:
               ValidateMinorVersionByte(value);
               break;
            case 7:
               ValidateRevisionByte(value);
               break;
            default:
               throw new ArgumentOutOfRangeException("Invalid AMQP Header byte index provided to validation method: " + index);
         }
      }

      private static void ValidatePrefixByte1(byte value)
      {
         if (value != PREFIX[0])
         {
            throw new ArgumentException(String.Format(
                "Invalid header byte(1) specified {0} : expected {1}", value, PREFIX[0]));
         }
      }

      private static void ValidatePrefixByte2(byte value)
      {
         if (value != PREFIX[1])
         {
            throw new ArgumentException(String.Format(
                "Invalid header byte(2) specified {0} : expected {1}", value, PREFIX[1]));
         }
      }

      private static void ValidatePrefixByte3(byte value)
      {
         if (value != PREFIX[2])
         {
            throw new ArgumentException(String.Format(
                "Invalid header byte(3) specified {0} : expected {1}", value, PREFIX[2]));
         }
      }

      private static void ValidatePrefixByte4(byte value)
      {
         if (value != PREFIX[3])
         {
            throw new ArgumentException(String.Format(
                "Invalid header byte(4) specified {0} : expected {1}", value, PREFIX[3]));
         }
      }

      private static void ValidateProtocolByte(byte value)
      {
         if (value != AMQP_PROTOCOL_ID && value != SASL_PROTOCOL_ID)
         {
            throw new ArgumentException(String.Format(
                "Invalid protocol Id specified {0} : expected one of {1} or {2}",
                value, AMQP_PROTOCOL_ID, SASL_PROTOCOL_ID));
         }
      }

      private static void ValidateMajorVersionByte(byte value)
      {
         if (value != 1)
         {
            throw new ArgumentException(String.Format(
                "Invalid Major version specified {0} : expected {1}", value, 1));
         }
      }

      private static void ValidateMinorVersionByte(byte value)
      {
         if (value != 0)
         {
            throw new ArgumentException(String.Format(
                "Invalid Minor version specified {0} : expected {1}", value, 0));
         }
      }

      private static void ValidateRevisionByte(byte value)
      {
         if (value != 0)
         {
            throw new ArgumentException(String.Format(
                "Invalid revision specified {0} : expected {1}", value, 0));
         }
      }

      #endregion

      #region The private implementation of the AMQPHeader class

      private AMQPHeader SetBuffer(byte[] buffer, bool validate)
      {
         if (validate)
         {
            if (buffer.Length != 8 || !StartsWith(buffer, PREFIX))
            {
               throw new ArgumentException("Not an AMQP header buffer");
            }

            ValidateProtocolByte(buffer[PROTOCOL_ID_INDEX]);
            ValidateMajorVersionByte(buffer[MAJOR_VERSION_INDEX]);
            ValidateMinorVersionByte(buffer[MINOR_VERSION_INDEX]);
            ValidateRevisionByte(buffer[REVISION_INDEX]);
         }

         if (buffer.Length > HEADER_SIZE_BYTES)
         {
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer is to large to be an AMQP Header value");
         }

         this.buffer = buffer;
         return this;
      }

      private static bool StartsWith(byte[] buffer, byte[] value)
      {
         if (buffer == null || buffer.Length < value.Length)
         {
            return false;
         }

         for (int i = 0; i < value.Length; ++i)
         {
            if (buffer[i] != value[i])
            {
               return false;
            }
         }

         return true;
      }

      #endregion
   }
}