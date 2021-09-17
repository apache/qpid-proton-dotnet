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
   public sealed class ProtonEngineFactory : IEngineFactory
   {
      public static readonly IEngineFactory Instance = new ProtonEngineFactory();

      private ProtonEngineFactory()
      {
      }

      public IEngine CreateEngine()
      {
         ProtonEngine engine = new ProtonEngine();
         IEnginePipeline pipeline = engine.Pipeline;

         pipeline.AddLast(ProtonConstants.AmqpPerformativeHandler, new ProtonPerformativeHandler());
         //pipeline.AddLast(ProtonConstants.SaslPerformativeHandler, new ProtonSaslHandler());
         pipeline.AddLast(ProtonConstants.FrameLoggingHandler, new ProtonFrameLoggingHandler());
         pipeline.AddLast(ProtonConstants.FrameDecodingHandler, new ProtonFrameDecodingHandler());
         pipeline.AddLast(ProtonConstants.FrameEncodingHandler, new ProtonFrameEncodingHandler());

         return engine;
      }

      public IEngine CreateNonSaslEngine()
      {
         ProtonEngine engine = new ProtonEngine();
         IEnginePipeline pipeline = engine.Pipeline;

         pipeline.AddLast(ProtonConstants.AmqpPerformativeHandler, new ProtonPerformativeHandler());
         pipeline.AddLast(ProtonConstants.FrameLoggingHandler, new ProtonFrameLoggingHandler());
         pipeline.AddLast(ProtonConstants.FrameDecodingHandler, new ProtonFrameDecodingHandler());
         pipeline.AddLast(ProtonConstants.FrameEncodingHandler, new ProtonFrameEncodingHandler());

         return engine;
      }
   }
}