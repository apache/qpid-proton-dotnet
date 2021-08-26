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
using System.Collections;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   public abstract class PerformativeDescribedType : ListDescribedType
   {
      protected PerformativeDescribedType(int numberOfFields) : base(numberOfFields)
      {
      }

      protected PerformativeDescribedType(int numberOfFields, IList described) : base(numberOfFields, described)
      {
      }

      public abstract PerformativeType Type { get; }

      public abstract void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, Span<byte> payload, ushort channel, T context);

      public virtual object FieldValueOrSpecDefault(int index)
      {
         return this[index];
      }

      public override string ToString()
      {
         return Type + " " + List;
      }
   }
}