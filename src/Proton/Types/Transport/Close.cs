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

using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class Close : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000018UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:close:list");

      public ErrorCondition Error { get; set; }

      public Close() : base() { }

      public Close(Close other) : this() => this.Error = (ErrorCondition)other.Error?.Clone();

      public object Clone()
      {
         return new Close(this);
      }

      public Close Copy()
      {
         return new Close(this);
      }

      public PerformativeType Type => PerformativeType.Close;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, ushort channel, T context)
      {
         handler.HandleClose(this, payload, channel, context);
      }

      public override string ToString()
      {
         return "Close{" + "error=" + Error + '}';
      }
   }
}