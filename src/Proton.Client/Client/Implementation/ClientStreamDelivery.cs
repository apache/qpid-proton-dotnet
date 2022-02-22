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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// The stream delivery type manages the underlying state of an incoming
   /// streaming message delivery and provides the stream type used to read
   /// and block for reads when not all requested message data has arrived.
   /// The delivery will also manage settlement of a streaming delivery and
   /// apply receiver configuration rules like auto settlement to the delivery
   /// as incoming portions of the message arrive.
   /// </summary>
   public class ClientStreamDelivery : IStreamDelivery
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamDelivery>();

      private readonly ClientStreamReceiver receiver;
      private readonly IIncomingDelivery protonDelivery;

      private ClientStreamReceiverMessage message;
      private RawDeliveryInputStream rawInputStream;

      internal ClientStreamDelivery(ClientStreamReceiver receiver, IIncomingDelivery protonDelivery)
      {
         this.receiver = receiver;
         this.protonDelivery = protonDelivery;
         this.protonDelivery.LinkedResource = this;

         // Capture inbound events and route to an active stream or message
         protonDelivery.DeliveryReadHandler(HandleDeliveryRead)
                       .DeliveryAbortedHandler(HandleDeliveryAborted);
      }

      public IStreamReceiver Receiver => receiver;

      public bool Aborted => protonDelivery.IsAborted;

      public bool Completed => !protonDelivery.IsPartial;

      public uint MessageFormat => protonDelivery.MessageFormat;

      public bool Settled => protonDelivery.IsSettled;

      public IDeliveryState State => protonDelivery.State?.ToClientDeliveryState();

      public bool RemoteSettled => protonDelivery.IsRemotelySettled;

      public IDeliveryState RemoteState => protonDelivery.RemoteState.ToClientDeliveryState();

      public IReadOnlyDictionary<string, object> Annotations
      {
         get
         {
            if (rawInputStream != null && message == null)
            {
               throw new ClientIllegalStateException("Cannot access Delivery Annotations API after requesting an InputStream");
            }

            return ClientConversionSupport.ToStringKeyedMap(
               ((ClientStreamReceiverMessage)Message()).DeliveryAnnotations?.Value);
         }
      }

      public Stream RawInputStream
      {
         get
         {
            if (message != null)
            {
               throw new ClientIllegalStateException("Cannot access Delivery InputStream API after requesting an Message");
            }

            if (rawInputStream == null)
            {
               rawInputStream = new RawDeliveryInputStream(this);
            }

            return rawInputStream;
         }
      }

      public IStreamReceiverMessage Message()
      {
         if (rawInputStream != null && message == null)
         {
            throw new ClientIllegalStateException("Cannot access Delivery Message API after requesting an InputStream");
         }

         if (message == null)
         {
            message = new ClientStreamReceiverMessage(receiver, this, rawInputStream = new RawDeliveryInputStream(this));
         }

         return message;
      }

      public IStreamDelivery Accept()
      {
         receiver.Disposition(protonDelivery, Accepted.Instance, true);
         return this;
      }

      public IStreamDelivery Disposition(IDeliveryState state, bool settled)
      {
         receiver.Disposition(protonDelivery, state?.AsProtonType(), true);
         return this;
      }

      public IStreamDelivery Modified(bool deliveryFailed, bool undeliverableHere)
      {
         receiver.Disposition(protonDelivery, new Modified(deliveryFailed, undeliverableHere), true);
         return this;
      }

      public IStreamDelivery Reject(string condition, string description)
      {
         receiver.Disposition(protonDelivery, new Rejected(new ErrorCondition(condition, description)), true);
         return this;
      }

      public IStreamDelivery Release()
      {
         receiver.Disposition(protonDelivery, Released.Instance, true);
         return this;
      }

      public IStreamDelivery Settle()
      {
         receiver.Disposition(protonDelivery, null, true);
         return this;
      }

      #region Internal Stream Delivery API

      internal IIncomingDelivery ProtonDelivery => protonDelivery;

      internal void HandleReceiverClosed(ClientStreamReceiver receiver)
      {
         rawInputStream?.HandleReceiverClosed(receiver);
      }

      #endregion

      #region private stream delivery implementation

      private void HandleDeliveryRead(IIncomingDelivery delivery)
      {
         rawInputStream?.HandleDeliveryRead(delivery);
      }

      private void HandleDeliveryAborted(IIncomingDelivery delivery)
      {
         rawInputStream?.HandleDeliveryAborted(delivery);
      }

      #endregion

      #region Raw incoming byte stream message

      private class RawDeliveryInputStream : Stream
      {
         private const int INVALID_MARK = -1;
         private const int DEFAULT_MARK_LIMIT = 1024;

         private readonly AtomicBoolean closed = false;
         private readonly ClientStreamDelivery delivery;
         private readonly ClientStreamReceiver receiver;
         private readonly ClientSession session;
         private readonly ClientConnection connection;
         private readonly Engine.IIncomingDelivery protonDelivery;
         private readonly IProtonCompositeBuffer buffer = IProtonCompositeBuffer.Compose();

         private TaskCompletionSource<int> readRequest;

         public RawDeliveryInputStream(ClientStreamDelivery delivery)
         {
            this.delivery = delivery;
            this.receiver = delivery.receiver;
            this.protonDelivery = delivery.protonDelivery;
            this.session = (ClientSession)delivery.receiver.Session;
            this.connection = (ClientConnection)delivery.receiver.Session.Connection;
         }

         public override bool CanRead => true;

         public override bool CanSeek => false;

         public override bool CanWrite => false;

         public override long Length
         {
            get
            {
               CheckStreamStateIsValid();
               if (buffer.IsReadable)
               {
                  return buffer.WriteOffset;
               }
               else
               {
                  TaskCompletionSource<int> request = new TaskCompletionSource<int>();

                  try
                  {
                     connection.Execute(() =>
                     {
                        if (protonDelivery.Available > 0)
                        {
                           buffer.Append(protonDelivery.ReadAll().Split());
                        }

                        request.TrySetResult((int)buffer.WriteOffset);
                     });

                     return connection.Request(receiver, request).Task.GetAwaiter().GetResult();
                  }
                  catch (Exception e)
                  {
                     throw new IOException("Error getting available bytes from incoming delivery", e);
                  }
               }
            }
         }

         public override long Position
         {
            get => buffer.ReadOffset;
            set => Seek(value, SeekOrigin.Begin);
         }

         public override void Close()
         {
            if (closed.CompareAndSet(false, true))
            {
               try
               {
                  TaskCompletionSource<bool> closeRequest = new TaskCompletionSource<bool>();

                  connection.Execute(() =>
                  {
                     AutoAcceptDeliveryIfNecessary();

                     // If the deliver wasn't fully read either because there are remaining
                     // bytes locally we need to discard those to aid in retention avoidance.
                     // and to potentially open the session window to allow for fully reading
                     // and discarding any inbound bytes that remain.
                     try
                     {
                        _ = protonDelivery.ReadAll();
                     }
                     catch (EngineFailedException)
                     {
                        // Ignore as engine is down and we cannot read any more
                     }

                     // Clear anything that wasn't yet read and then clear any pending read request as EOF
                     buffer.WriteOffset = buffer.Capacity;
                     buffer.ReadOffset = buffer.Capacity;

                     buffer.Compact();

                     if (readRequest != null)
                     {
                        readRequest.TrySetResult(-1);
                        readRequest = null;
                     }

                     closeRequest.TrySetResult(true);
                  });

                  connection.Request(receiver, closeRequest);
               }
               finally
               {
                  base.Close();
               }
            }
         }

         public override void Flush()
         {
            // Nothing to do here for incoming raw message stream.
         }

         public override int ReadByte()
         {
            CheckStreamStateIsValid();

            int result = -1;

            while (true)
            {
               if (buffer.IsReadable)
               {
                  result = buffer.ReadUnsignedByte() & 0xff;
                  TryReleaseReadBuffers();
                  break;
               }
               else if (RequestMoreData() < 0)
               {
                  break;
               }
            }

            return result;
         }

         public override int Read(Span<byte> buffer)
         {
            int bytesRead = 0;
            for (; bytesRead < buffer.Length; ++bytesRead)
            {
               int result = ReadByte();
               if (result >= 0)
               {
                  buffer[bytesRead] = (byte)result;
               }
               else
               {
                  break;
               }
            }

            return bytesRead;
         }

         public override int Read(byte[] target, int offset, int length)
         {
            CheckStreamStateIsValid();

            Statics.CheckFromIndexSize(offset, length, target.Length);

            int remaining = length;
            int bytesRead = 0;

            if (length <= 0)
            {
               return 0;
            }

            while (remaining > 0)
            {
               if (buffer.IsReadable)
               {
                  if (buffer.ReadableBytes < remaining)
                  {
                     int readTarget = (int)buffer.ReadableBytes;
                     buffer.CopyInto(buffer.ReadOffset, target, offset + bytesRead, buffer.ReadableBytes);
                     buffer.ReadOffset = buffer.WriteOffset;
                     bytesRead += readTarget;
                     remaining -= readTarget;
                  }
                  else
                  {
                     buffer.CopyInto(buffer.ReadOffset, target, offset + bytesRead, remaining);
                     buffer.ReadOffset += remaining;
                     bytesRead += remaining;
                     remaining = 0;
                  }

                  TryReleaseReadBuffers();
               }
               else if (RequestMoreData() < 0)
               {
                  return bytesRead > 0 ? bytesRead : -1;
               }
            }

            return bytesRead;
         }

         public override long Seek(long offset, SeekOrigin origin)
         {
            throw new NotSupportedException("Cannot seek within the large message byte stream");
         }

         public override void SetLength(long value)
         {
            throw new NotSupportedException("Cannot set length an a message delivery incoming bytes stream");
         }

         public override void Write(byte[] buffer, int offset, int count)
         {
            throw new NotSupportedException("Cannot write to an a message delivery incoming bytes stream");
         }

         #region Delivery event handlers

         internal void HandleDeliveryRead(IIncomingDelivery delivery)
         {
            if (closed)
            {
               // Clear any pending data to expand session window if not yet complete
               _ = delivery.ReadAll();
            }
            else
            {
               // An input stream is awaiting some more incoming bytes, check to see if
               // the delivery had a non-empty transfer frame and provide them.
               if (readRequest != null)
               {
                  if (delivery.Available > 0)
                  {
                     buffer.Append(protonDelivery.ReadAll().Split());
                     readRequest.TrySetResult((int)buffer.ReadableBytes);
                     readRequest = null;
                  }
                  else if (!delivery.IsPartial)
                  {
                     AutoAcceptDeliveryIfNecessary();
                     readRequest.TrySetResult(-1);
                     readRequest = null;
                  }
               }
            }
         }

         internal void HandleDeliveryAborted(IIncomingDelivery delivery)
         {
            readRequest?.TrySetException(new ClientDeliveryAbortedException("The remote sender has aborted this delivery"));

            delivery.Settle();
         }

         internal void HandleReceiverClosed(ClientStreamReceiver receiver)
         {
            readRequest?.TrySetException(new ClientResourceRemotelyClosedException("The receiver link has been remotely closed."));
         }

         #endregion

         #region Private APIs for internal Stream use

         private void TryReleaseReadBuffers()
         {
            buffer.Reclaim();
         }

         private int RequestMoreData()
         {
            TaskCompletionSource<int> request = new TaskCompletionSource<int>();

            try
            {
               connection.Execute(() =>
               {
                  if (protonDelivery.Receiver.IsLocallyClosedOrDetached)
                  {
                     request.TrySetException(new ClientException("Cannot read from delivery due to link having been closed"));
                  }
                  else if (protonDelivery.Available > 0)
                  {
                     buffer.Append(protonDelivery.ReadAll().Split());
                     request.TrySetResult((int)buffer.ReadableBytes);
                  }
                  else if (protonDelivery.IsAborted)
                  {
                     request.TrySetException(new ClientDeliveryAbortedException("The remote sender has aborted this delivery"));
                  }
                  else if (!protonDelivery.IsPartial)
                  {
                     AutoAcceptDeliveryIfNecessary();
                     request.TrySetResult(-1);
                  }
                  else
                  {
                     readRequest = request;
                  }
               });

               return connection.Request(receiver, request).Task.GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
               throw new IOException("Error reading requested data", e);
            }
         }

         private void AutoAcceptDeliveryIfNecessary()
         {
            if (receiver.ReceiverOptions.AutoAccept && !protonDelivery.IsSettled)
            {
               if (!buffer.IsReadable && protonDelivery.Available == 0 &&
                   (protonDelivery.IsAborted || !protonDelivery.IsPartial))
               {

                  try
                  {
                     receiver.Disposition(protonDelivery, Accepted.Instance, receiver.ReceiverOptions.AutoSettle);
                  }
                  catch (Exception error)
                  {
                     LOG.Trace("Caught error while attempting to auto accept the fully read delivery.", error);
                  }
               }
            }
         }

         private void CheckStreamStateIsValid()
         {
            if (closed)
            {
               throw new IOException("The InputStream has been explicitly closed");
            }

            if (receiver.IsClosed)
            {
               throw new IOException("Underlying receiver has closed", receiver.FailureCause);
            }
         }

         #endregion
      }

      #endregion
   }
}