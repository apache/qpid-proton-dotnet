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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// A Default No-Op SASL context that is used to provide the engine with a stub
   /// when no SASL is configured for the operating engine.
   /// </summary>
   public class ProtonEngineNoOpSaslDriver : IEngineSaslDriver
   {
      public static readonly ProtonEngineNoOpSaslDriver Instance = new ProtonEngineNoOpSaslDriver();

      public static readonly uint MinMaxSaslFrameSize = 512;

      public EngineSaslState SaslState => EngineSaslState.None;

      public SaslOutcome? SaslOutcome => Sasl.SaslOutcome.SaslOk;

      public uint MaxFrameSize
      {
         get => MinMaxSaslFrameSize;
         set {}
      }
   }
}