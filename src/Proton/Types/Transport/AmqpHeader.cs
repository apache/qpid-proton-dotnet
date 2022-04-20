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
using System.Text;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class AmqpHeader : IEquatable<AmqpHeader>
   {
      internal static readonly byte[] PREFIX = new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P' };

      public static readonly int ProtocolIdIndex = 4;
      public static readonly int MajorVersionIndex = 5;
      public static readonly int MinorVersionIndex = 6;
      public static readonly int RevisionIndex = 7;

      public static readonly byte AmqpProtocolId = 0;
      public static readonly byte SaslProtocolId = 3;

      public static readonly int HeaderSizeBytes = 8;

      private static readonly AmqpHeader Amqp =
        new(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });

      private static readonly AmqpHeader Sasl =
        new(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 3, 1, 0, 0 });

      private IProtonBuffer buffer;

      public AmqpHeader()
      {
         SetBuffer(Amqp.buffer, false);
      }

      public AmqpHeader(byte[] headerBytes)
      {
         SetBuffer(ProtonByteBufferAllocator.Instance.Wrap(headerBytes), true);
      }

      public AmqpHeader(IProtonBuffer buffer)
      {
         SetBuffer(ProtonByteBufferAllocator.Instance.Allocate(HeaderSizeBytes, HeaderSizeBytes).WriteBytes(buffer), true);
      }

      public AmqpHeader(IProtonBuffer buffer, bool validate)
      {
         SetBuffer(ProtonByteBufferAllocator.Instance.Allocate(HeaderSizeBytes, HeaderSizeBytes).WriteBytes(buffer), validate);
      }

      public static AmqpHeader GetAMQPHeader()
      {
         return Amqp;
      }

      public static AmqpHeader GetSASLHeader()
      {
         return Sasl;
      }

      public int ProtocolId
      {
         get { return buffer.GetByte(ProtocolIdIndex) & 0xFF; }
      }

      public int Major
      {
         get { return buffer.GetByte(MajorVersionIndex) & 0xFF; }
      }

      public int Minor
      {
         get { return buffer.GetByte(MinorVersionIndex) & 0xFF; }
      }

      public int Revision
      {
         get { return buffer.GetByte(RevisionIndex) & 0xFF; }
      }

      public IProtonBuffer Buffer
      {
         get { return buffer.Copy(); }
      }

      public byte[] ToArray()
      {
         if (buffer != null)
         {
            byte[] copy = new byte[buffer.ReadableBytes];
            buffer.CopyInto(0, copy, 0, (int) buffer.ReadableBytes);
            return copy;
         }
         else
         {
            return null;
         }
      }

      public byte GetByteAt(int i)
      {
         return buffer.GetUnsignedByte(i);
      }

      public bool HasValidPrefix()
      {
         return StartsWith(buffer, PREFIX);
      }

      public bool IsSaslHeader()
      {
         return ProtocolId == SaslProtocolId;
      }

      public override int GetHashCode()
      {
         const int prime = 31;
         int result = 1;
         result = prime * result + ((buffer == null) ? 0 : buffer.GetHashCode());
         return result;
      }

      public override bool Equals(object other)
      {
         if (other != null && other.GetType() == GetType())
         {
            return Equals((AmqpHeader)other);
         }
         else
         {
            return false;
         }
      }

      public bool Equals(AmqpHeader other)
      {
         if (this == other)
         {
            return true;
         }

         if (other == null)
         {
            return false;
         }

         if (buffer == null)
         {
            if (other.buffer != null)
            {
               return false;
            }
         }
         else if (!buffer.Equals(other.buffer))
         {
            return false;
         }

         return true;
      }

      public override string ToString()
      {
         StringBuilder builder = new();
         for (int i = 0; i < buffer.ReadableBytes; ++i)
         {
            char value = (char)buffer.GetByte(i);
            if (char.IsLetter(value))
            {
               builder.Append(value);
            }
            else
            {
               builder.Append(',');
               builder.Append((int)value);
            }
         }

         return builder.ToString();
      }

      private static bool StartsWith(IProtonBuffer buffer, byte[] value)
      {
         if (buffer == null || buffer.ReadableBytes < value.Length)
         {
            return false;
         }

         for (int i = 0; i < value.Length; ++i)
         {
            if (buffer.GetByte(i) != value[i])
            {
               return false;
            }
         }

         return true;
      }

      private AmqpHeader SetBuffer(IProtonBuffer value, bool validate)
      {
         if (validate)
         {
            if (value.ReadableBytes != 8 || !StartsWith(value, PREFIX))
            {
               throw new ArgumentException("Not an AMQP header buffer");
            }

            ValidateProtocolByte(value.GetUnsignedByte(ProtocolIdIndex));
            ValidateMajorVersionByte(value.GetUnsignedByte(MajorVersionIndex));
            ValidateMinorVersionByte(value.GetUnsignedByte(MinorVersionIndex));
            ValidateRevisionByte(value.GetUnsignedByte(RevisionIndex));
         }

         buffer = value;
         return this;
      }

      /// <summary>
      /// Called to validate a byte according to a given index within the AMQP Header.
      /// If the index is outside the range of the header size an exception will be thrown.
      /// /// </summary>
      /// <param name="index">The index in the header where the byte should be validated.</param>
      /// <param name="value">The value to check validity of in the given index in the AMQP Header.</param>
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
               throw new ArgumentOutOfRangeException(
                  "Invalid AMQP Header byte index provided to validation method: " + index);
         }
      }

      private static void ValidatePrefixByte1(byte value)
      {
         if (value != PREFIX[0])
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid header byte(1) specified {0} : expected {1}", value, PREFIX[0]));
         }
      }

      private static void ValidatePrefixByte2(byte value)
      {
         if (value != PREFIX[1])
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid header byte(2) specified {0} : expected {1}", value, PREFIX[1]));
         }
      }

      private static void ValidatePrefixByte3(byte value)
      {
         if (value != PREFIX[2])
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid header byte(3) specified {0} : expected {1}", value, PREFIX[2]));
         }
      }

      private static void ValidatePrefixByte4(byte value)
      {
         if (value != PREFIX[3])
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid header byte(4) specified {0} : expected {1}", value, PREFIX[3]));
         }
      }

      private static void ValidateProtocolByte(byte value)
      {
         if (value != AmqpProtocolId && value != SaslProtocolId)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid protocol Id specified {0} : expected one of {1} or {2}",
                value, AmqpProtocolId, SaslProtocolId));
         }
      }

      private static void ValidateMajorVersionByte(byte value)
      {
         if (value != 1)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid Major version specified {0} : expected {1}", value, 1));
         }
      }

      private static void ValidateMinorVersionByte(byte value)
      {
         if (value != 0)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid Minor version specified {0} : expected {1}", value, 0));
         }
      }

      private static void ValidateRevisionByte(byte value)
      {
         if (value != 0)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Invalid revision specified {0} : expected {1}", value, 0));
         }
      }

      /// <summary>
      /// Provide this AMQP Header with a handler that will process the given AMQP header
      /// depending on the protocol type the correct handler method is invoked.
      /// </summary>
      /// <typeparam name="TContext">The type of the context that is provided to this visit</typeparam>
      /// <param name="handler">The handler instance that will process the event</param>
      /// <param name="context">The context to provide to the event call</param>
      public void Invoke<TContext>(Action<AmqpHeader, TContext> amqp, Action<AmqpHeader, TContext> sasl, TContext context)
      {
         if (IsSaslHeader())
         {
            sasl.Invoke(this, context);
         }
         else
         {
            amqp.Invoke(this, context);
         }
      }

      /// <summary>
      /// Provide this AMQP Header with a handler that will process the given AMQP header
      /// depending on the protocol type the correct handler method is invoked.
      /// </summary>
      /// <typeparam name="T">The type of the context that is provided to this visit</typeparam>
      /// <param name="handler">The handler instance that will process the event</param>
      /// <param name="context">The context to provide to the event call</param>
      public void Invoke<T>(IHeaderHandler<T> handler, T context)
      {
         if (IsSaslHeader())
         {
            handler.HandleSASLHeader(this, context);
         }
         else
         {
            handler.HandleAMQPHeader(this, context);
         }
      }
   }
}