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

using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Handler of incoming frames with some API to inform the user of its
   /// prescribed frame limitations
   /// </summary>
   public interface IFrameHandler
   {
      /// <summary>
      /// How large of an incoming frame will this handler accept.
      /// </summary>
      uint InboundMaxFrameSize { get; }

      uint EmptyFrameCount { get; }

      uint PerformativeCount { get; }

      uint SaslPerformativeCount { get; }

      string Name { get; }

      ILoggerFactory LoggerFactory { get; }

      void HandleHeader(AMQPHeader header);

      void HandleSaslPerformative(uint frameSize, SaslDescribedType sasl, ushort channel, byte[] payload);

      void HandlePerformative(uint frameSize, PerformativeDescribedType amqp, ushort channel, byte[] payload);

      void HandleHeartbeat(uint frameSize, ushort channel);

   }
}