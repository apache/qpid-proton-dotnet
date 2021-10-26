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

using System.Collections.Generic;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP stream sender instance which can be used to transmit large message
   /// payloads to the remote without needing to load the full message contents into
   /// memory.  The streaming sender will also provide flow control that attempts to
   /// provide additional safety values for out of memory situations.
   /// </summary>
   public interface IStreamSender : ISender
   {
      /// <summary>
      /// Creates and returns a new streamable message that can be used by the caller to perform
      /// streaming sends of large message payload data.  Only one streamed message can be active
      /// at a time so any successive calls to begin a new streaming message will throw an error
      /// to indicate that the previous instance has not yet been completed.
      /// </summary>
      /// <param name="deliveryAnnotations">The optional delivery annotations to transmit with the message</param>
      /// <returns>This stream sender instance</returns>
      IStreamSenderMessage BeginMessage(IDictionary<string, object> deliveryAnnotations = null);

   }
}