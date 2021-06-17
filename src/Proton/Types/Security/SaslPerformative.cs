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

namespace Apache.Qpid.Proton.Types.Security
{
   public interface SaslPerformative : ICloneable
   {
      /// <summary>
      /// Provides the enumeration value that identifies this type.
      /// </summary>
      SaslPerformativeType Type { get; }

      /// <summary>
      /// Invokes the appropriate handler method for the type where this method
      /// was called. Provides a context object type that is passed to aid in the
      /// event processing.
      /// </summary>
      /// <typeparam name="T">context type to provide to the event handler</typeparam>
      /// <param name="handler">The SASL performative handler to invoke</param>
      /// <param name="context">The context object to provide to the handler</param>
      void Invoke<T>(SaslPerformativeHandler<T> handler, T context);

   }
}