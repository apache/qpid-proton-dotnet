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

namespace Apache.Qpid.Proton.Types.Transport
{
   public interface IPerformative : ICloneable
   {
      /// <summary>
      /// Provides the type of this performative.
      /// </summary>
      PerformativeType Type { get; }

      /// <summary>
      /// Visit the performative using a handler instance and provided context data.
      /// </summary>
      /// <typeparam name="T">The type of context that is provided</typeparam>
      /// <param name="handler">The handler instance to visit</param>
      /// <param name="payload">The payload that the performative was sent with</param>
      /// <param name="channel">The channel the performative was sent on</param>
      /// <param name="context">The context for this visitation</param>
      void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, ushort channel, T context);

   }
}