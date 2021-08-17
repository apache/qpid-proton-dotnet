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

using System.Numerics;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class Detach : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000016L;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:detach:list");

      private static readonly uint HANDLE = 1;
      private static readonly uint CLOSED = 2;
      private static readonly uint ERROR = 4;

      private uint modified = 0;

      private uint handle;
      private bool closed;
      private ErrorCondition error;

      public Detach() : base() { }

      public Detach(Detach other) : this()
      {
         this.closed = other.closed;
         this.handle = other.handle;
         this.error = other.error?.Copy();
         this.modified = other.modified;
      }

      #region Field access for Detach type

      public bool Closed
      {
         get { return closed; }
         set
         {
            this.modified |= CLOSED;
            this.closed = value;
         }
      }

      public uint Handle
      {
         get { return handle; }
         set
         {
            this.modified |= HANDLE;
            this.handle = value;
         }
      }

      public ErrorCondition Error
      {
         get { return error; }
         set
         {
            this.modified |= ERROR;
            this.error = value;
         }
      }

      #endregion

      #region Element count and value presence utility

      public bool IsEmpty() => modified == 0;

      public int GetElementCount() => 32 - BitOperations.LeadingZeroCount(modified);

      public bool HasHandle() => (modified & HANDLE) == HANDLE;

      public bool HasClosed() => (modified & CLOSED) == CLOSED;

      public bool HasError() => (modified & ERROR) == ERROR;

      #endregion

      public object Clone() => new Detach(this);

      public Detach Copy() => new Detach(this);

      public PerformativeType Type => PerformativeType.Detach;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, ushort channel, T context)
      {
         handler.HandleDetach(this, payload, channel, context);
      }

      public override string ToString()
      {
         return "Detach{" +
                "handle=" + (HasHandle() ? Handle : "null") +
                ", closed=" + (HasClosed() ? Closed : "null") +
                ", error=" + Error +
                '}';
      }
   }
}