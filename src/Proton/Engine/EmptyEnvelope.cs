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

using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// An empty incoming AMQP envelope that is used for singalling
   /// that an empty AMQP frame was read.
   /// </summary>
   public sealed class EmptyEnvelope : IncomingAmqpEnvelope
   {
      public static readonly EmptyEnvelope Instance = new EmptyEnvelope();

      public EmptyEnvelope() : base()
      {
      }

      public override string ToString()
      {
         return "Empty Frame";
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, T context)
      {
         // Nothing to do for empty frame.
      }
   }
}