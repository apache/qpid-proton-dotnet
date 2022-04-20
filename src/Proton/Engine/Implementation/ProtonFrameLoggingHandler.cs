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
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Frame logger for outgoing AMQP and SASL performatives and Headers
   /// </summary>
   public sealed class ProtonFrameLoggingHandler : IEngineHandler
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ProtonFrameLoggingHandler>();

      private static readonly string AMQP_IN_PREFIX = "<- AMQP";
      private static readonly string AMQP_OUT_PREFIX = "-> AMQP";
      private static readonly string SASL_IN_PREFIX = "<- SASL";
      private static readonly string SASL_OUT_PREFIX = "-> SASL";

      private static readonly int PAYLOAD_STRING_LIMIT = 64;
      private static readonly string PN_TRACE_FRM = "PN_TRACE_FRM";
      private static readonly bool TRACE_FRM_ENABLED = CheckTraceFramesEnabled();

      private bool traceFrames = TRACE_FRM_ENABLED;
      private int uniqueIdentifier;

      private static bool CheckTraceFramesEnabled()
      {
         string value = Environment.GetEnvironmentVariable(PN_TRACE_FRM)?.ToLower() ?? "false";
         return "true".Equals(value) || "1".Equals(value) || "yes".Equals(value);
      }

      internal bool TraceFrames
      {
         get => traceFrames;
         set => traceFrames = value;
      }

      #region Engine Handler event points where logging occurs.

      public void HandlerAdded(IEngineHandlerContext context)
      {
         // Provides a stable Id for the handler to use when logging frame traces so that applications with
         // multiple connections can be more easily debugged.
         uniqueIdentifier = Guid.NewGuid().GetHashCode();
      }

      public void HandleRead(IEngineHandlerContext context, HeaderEnvelope envelope)
      {
         if (traceFrames)
         {
            Trace(envelope.IsSaslHeader ? SASL_IN_PREFIX : AMQP_IN_PREFIX, uniqueIdentifier, 0, envelope.Body, null);
         }

         Log(envelope.IsSaslHeader ? SASL_IN_PREFIX : AMQP_IN_PREFIX, uniqueIdentifier, 0, envelope.Body, null);

         context.FireRead(envelope);
      }

      public void HandleRead(IEngineHandlerContext context, SaslEnvelope envelope)
      {
         if (traceFrames)
         {
            Trace(SASL_IN_PREFIX, uniqueIdentifier, 0, envelope.Body, null);
         }

         Log(SASL_IN_PREFIX, uniqueIdentifier, 0, envelope.Body, envelope.Payload);

         context.FireRead(envelope);
      }

      public void HandleRead(IEngineHandlerContext context, IncomingAmqpEnvelope envelope)
      {
         if (traceFrames)
         {
            Trace(AMQP_IN_PREFIX, uniqueIdentifier, envelope.Channel, envelope.Body, envelope.Payload);
         }

         if (LOG.IsTraceEnabled)
         {
            Log(AMQP_IN_PREFIX, uniqueIdentifier, envelope.Channel, envelope.Body, envelope.Payload);
         }

         context.FireRead(envelope);
      }

      public void HandleWrite(IEngineHandlerContext context, HeaderEnvelope envelope)
      {
         if (traceFrames)
         {
            Trace(envelope.IsSaslHeader ? SASL_OUT_PREFIX : AMQP_OUT_PREFIX, uniqueIdentifier, 0, envelope.Body, null);
         }

         Log(envelope.IsSaslHeader ? SASL_OUT_PREFIX : AMQP_OUT_PREFIX, uniqueIdentifier, 0, envelope.Body, null);

         context.FireWrite(envelope);
      }

      public void HandleWrite(IEngineHandlerContext context, OutgoingAmqpEnvelope envelope)
      {
         if (traceFrames)
         {
            Trace(AMQP_OUT_PREFIX, uniqueIdentifier, envelope.Channel, envelope.Body, envelope.Payload);
         }

         if (LOG.IsTraceEnabled)
         {
            Log(AMQP_OUT_PREFIX, uniqueIdentifier, envelope.Channel, envelope.Body, envelope.Payload);
         }

         context.FireWrite(envelope);
      }

      public void HandleWrite(IEngineHandlerContext context, SaslEnvelope envelope)
      {
         if (traceFrames)
         {
            Trace(SASL_OUT_PREFIX, uniqueIdentifier, 0, envelope.Body, null);
         }

         Log(SASL_OUT_PREFIX, uniqueIdentifier, 0, envelope.Body, null);

         context.FireWrite(envelope);
      }

      #endregion

      private static void Log(string prefix, int connection, int channel, object performative, IProtonBuffer payload)
      {
         if (payload == null)
         {
            LOG.Trace("{}:[{}:{}] {}", prefix, connection, channel, performative);
         }
         else
         {
            LOG.Trace("{}:[{}:{}] {} - {}", prefix, connection, channel, performative, StringUtils.ToQuotedString(payload, PAYLOAD_STRING_LIMIT, true));
         }
      }

      private static void Trace(string prefix, int connection, int channel, object performative, IProtonBuffer payload)
      {
         if (payload == null)
         {
            Console.WriteLine(string.Format(
               "{0}:[{1}:{2}] {3}", prefix, connection, channel, performative));
         }
         else
         {
            Console.WriteLine(string.Format(
               "{0}:[{1}:{2}] {3} - {4}", prefix, connection, channel, performative, StringUtils.ToQuotedString(payload, PAYLOAD_STRING_LIMIT, true)));
         }
      }
   }
}