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

using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Base class for envelope types that travel through the engine.
   /// </summary>
   public abstract class PerformativeEnvelope<T>
   {
      private T body;
      private ushort channel;
      private IProtonBuffer payload;

      internal PerformativeEnvelope(byte frameType)
      {
         FrameType = frameType;
      }

      internal PerformativeEnvelope<T> Initialize(T body, ushort channel, IProtonBuffer payload)
      {
         this.body = body;
         this.channel = channel;
         this.payload = payload;

         return this;
      }

      /// <summary>
      /// Access the performative that is the body of this envelope.
      /// </summary>
      public T Body { get => body; }

      /// <summary>
      /// Access the channel on which the performative was received.
      /// </summary>
      public ushort Channel { get => channel; }

      /// <summary>
      /// Access the payload bytes that arrived with the performative.
      /// </summary>
      public IProtonBuffer Payload { get => payload; }

      /// <summary>
      /// Provides the frame type that defines what types of performatives
      /// can be received.
      /// </summary>
      public byte FrameType { get; }

   }
}