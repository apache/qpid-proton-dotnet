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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   public enum CloseField
   {
      Error
   }

   public sealed class Close : PerformativeDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:close:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000018ul;

      public Close() : base(Enum.GetNames(typeof(CloseField)).Length)
      {
      }

      public Close(object described) : base(Enum.GetNames(typeof(CloseField)).Length, (IList)described)
      {
      }

      public Close(IList described) : base(Enum.GetNames(typeof(CloseField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Close;

      public override object Descriptor => throw new NotImplementedException();

      public ErrorCondition Error
      {
         get => (ErrorCondition)List[((int)CloseField.Error)];
         set => List[((int)CloseField.Error)] = value;
      }

      public override string ToString()
      {
         return "Close{" + "error=" + Error + '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, Span<byte> payload, ushort channel, T context)
      {
         handler.HandleClose(frameSize, this, payload, channel, context);
      }
   }
}