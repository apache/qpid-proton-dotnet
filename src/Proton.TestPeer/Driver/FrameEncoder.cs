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
using Apache.Qpid.Proton.Test.Driver.Codec;
using Apache.Qpid.Proton.Test.Driver.Codec.Impl;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Encodes outgoing AMQP frames
   /// </summary>
   public sealed class FrameEncoder
   {
      public static readonly byte AMQP_FRAME_TYPE = (byte)0;
      public static readonly byte SASL_FRAME_TYPE = (byte)1;

      private static readonly uint AMQP_PERFORMATIVE_PAD = 512;
      private static readonly uint FRAME_HEADER_SIZE = 8;

      private static readonly uint FRAME_START_BYTE = 0;
      private static readonly uint FRAME_DOFF_BYTE = 4;
      private static readonly uint FRAME_DOFF_SIZE = 2;
      private static readonly uint FRAME_TYPE_BYTE = 5;
      private static readonly uint FRAME_CHANNEL_BYTE = 6;

      private readonly AMQPTestDriver driver;

      private readonly ICodec codec = CodecFactory.Create();

      public FrameEncoder(AMQPTestDriver driver)
      {
         this.driver = driver;
      }
   }
}