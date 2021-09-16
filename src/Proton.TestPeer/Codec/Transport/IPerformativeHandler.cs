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
   /// <summary>
   /// Handler Interface that can be used to implement a visitor pattern of
   /// processing the AMQP performative types as they are sent or received.
   /// </summary>
   /// <typeparam name="T">The type of the context used in the processing</typeparam>
   public interface IPerformativeHandler<T>
   {
      void HandleOpen(uint frameSize, Open open, byte[] payload, ushort channel, T context);

      void HandleBegin(uint frameSize, Begin begin, byte[] payload, ushort channel, T context);

      void HandleAttach(uint frameSize, Attach attach, byte[] payload, ushort channel, T context);

      void HandleFlow(uint frameSize, Flow flow, byte[] payload, ushort channel, T context);

      void HandleTransfer(uint frameSize, Transfer transfer, byte[] payload, ushort channel, T context);

      void HandleDisposition(uint frameSize, Disposition disposition, byte[] payload, ushort channel, T context);

      void HandleDetach(uint frameSize, Detach detach, byte[] payload, ushort channel, T context);

      void HandleEnd(uint frameSize, End end, byte[] payload, ushort channel, T context);

      void HandleClose(uint frameSize, Close close, byte[] payload, ushort channel, T context);

      void HandleHeartbeat(uint frameSize, Heartbeat beat, byte[] payload, ushort channel, T context);

   }
}