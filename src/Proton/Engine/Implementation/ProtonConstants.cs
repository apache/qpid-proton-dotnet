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

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Constant definitions for value used throughout the proton engine API
   /// </summary>
   public static class ProtonConstants
   {
      /// <summary>
      /// The minimum allowed AMQP maximum frame size defined by the specification.
      /// </summary>
      public static readonly uint MinMaxAmqpFrameSize = 512;

      /**
       * The default AMQP max frame size used by the engine and connection if none is set
       * by the client or remote peer.
       */
      public static readonly uint DefaultMaxAmqpFrameSize = 65535;

      /**
       * The maximum value for AMQP channels as defined by the specification.
       */
      public static readonly ushort ChannelMax = 65535;

      /**
       * The maximum value for AMQP handles as defined by the specification.
       */
      public static readonly uint HandleMax = 0xFFFFFFFF;

      #region Proton Engine Handler default names

      /**
       * Engine handler that acts on AMQP performatives
       */
      public static readonly string AmqpPerformativeHandler = "amqp";

      /**
       * Engine handler that acts on SASL performatives
       */
      public static readonly string SaslPerformativeHandler = "sasl";

      /**
       * Engine handler that encodes performatives and writes the resulting buffer
       */
      public static readonly string FrameEncodingHandler = "frame-encoder";

      /**
       * Engine handler that decodes performatives and forwards the frames
       */
      public static readonly string FrameDecodingHandler = "frame-decoder";

      /**
       * Engine handler that logs incoming and outgoing performatives and frames
       */
      public static readonly string FrameLoggingHandler = "frame-logger";

      #endregion
   }
}