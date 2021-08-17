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

namespace Apache.Qpid.Proton.Types.Transport
{
   /// <summary>
   /// Handler Interface that can be used to implement a visitor pattern of
   /// processing the AMQP performative types as they are sent or received.
   /// </summary>
   /// <typeparam name="T">The type of the context used in the processing</typeparam>
   public interface IPerformativeHandler<T>
   {
      void HandleOpen(Open open, IProtonBuffer payload, ushort channel, T context) { }

      void HandleBegin(Begin begin, IProtonBuffer payload, ushort channel, T context) { }

      void HandleAttach(Attach attach, IProtonBuffer payload, ushort channel, T context) { }

      void HandleFlow(Flow flow, IProtonBuffer payload, ushort channel, T context) { }

      void HandleTransfer(Transfer transfer, IProtonBuffer payload, ushort channel, T context) { }

      void HandleDisposition(Disposition disposition, IProtonBuffer payload, ushort channel, T context) { }

      void HandleDetach(Detach detach, IProtonBuffer payload, ushort channel, T context) { }

      void HandleEnd(End end, IProtonBuffer payload, ushort channel, T context) { }

      void HandleClose(Close close, IProtonBuffer payload, ushort channel, T context) { }

   }
}