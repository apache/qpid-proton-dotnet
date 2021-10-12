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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   public class FrameRecordingTransportHandler : IEngineHandler
   {
      private List<HeaderEnvelope> headerRead = new List<HeaderEnvelope>();
      private List<HeaderEnvelope> headerWritten = new List<HeaderEnvelope>();
      private List<IncomingAmqpEnvelope> framesRead = new List<IncomingAmqpEnvelope>();
      private List<OutgoingAmqpEnvelope> framesWritten = new List<OutgoingAmqpEnvelope>();
      private List<SaslEnvelope> saslRead = new List<SaslEnvelope>();
      private List<SaslEnvelope> saslWritten = new List<SaslEnvelope>();

      public FrameRecordingTransportHandler()
      {
      }

      public int ReadCount => headerRead.Count + framesRead.Count + saslRead.Count;

      public int WriteCount => headerWritten.Count + framesWritten.Count + saslWritten.Count;

      public IList<OutgoingAmqpEnvelope> AmqpFramesWritten => framesWritten;

      public IList<IncomingAmqpEnvelope> AmqpFramesRead => framesRead;

      public IList<SaslEnvelope> SaslFramesWritten => saslWritten;

      public IList<SaslEnvelope> SaslFramesRead => saslRead;

      public IList<HeaderEnvelope> HeadersWritten => headerWritten;

      public IList<HeaderEnvelope> HeadersRead => headerRead;

      public void HandleRead(IEngineHandlerContext context, HeaderEnvelope header)
      {
         headerRead.Add(header);
         context.FireRead(header);
      }

      public void HandleRead(IEngineHandlerContext context, SaslEnvelope frame)
      {
         saslRead.Add(frame);
         context.FireRead(frame);
      }

      public void HandleRead(IEngineHandlerContext context, IncomingAmqpEnvelope frame)
      {
         framesRead.Add(frame);
         context.FireRead(frame);
      }

      public void HandleWrite(IEngineHandlerContext context, HeaderEnvelope frame)
      {
         headerWritten.Add(frame);
         context.FireWrite(frame);
      }

      public void HandleWrite(IEngineHandlerContext context, OutgoingAmqpEnvelope frame)
      {
         framesWritten.Add(frame);
         context.FireWrite(frame);
      }

      public void HandleWrite(IEngineHandlerContext context, SaslEnvelope frame)
      {
         saslWritten.Add(frame);
         context.FireWrite(frame);
      }
   }
}