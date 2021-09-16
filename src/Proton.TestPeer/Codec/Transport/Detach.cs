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
   public enum DetachField
   {
      Handle,
      Closed,
      Error
   }

   public sealed class Detach : PerformativeDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:detach:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000016ul;

      public Detach() : base(Enum.GetNames(typeof(DetachField)).Length)
      {
      }

      public Detach(object described) : base(Enum.GetNames(typeof(DetachField)).Length, (IList)described)
      {
      }

      public Detach(IList described) : base(Enum.GetNames(typeof(DetachField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Detach;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public uint? Handle
      {
         get => (uint?)List[((int)DetachField.Handle)];
         set => List[((int)DetachField.Handle)] = value;
      }

      public bool? Closed
      {
         get => (bool?)List[((int)DetachField.Closed)];
         set => List[((int)DetachField.Closed)] = value;
      }

      public ErrorCondition Error
      {
         get => (ErrorCondition)List[((int)DetachField.Error)];
         set => List[((int)DetachField.Error)] = value;
      }

      public override string ToString()
      {
         return "Detach{" +
                "handle=" + Handle +
                ", closed=" + Closed +
                ", error=" + Error +
                '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, byte[] payload, ushort channel, T context)
      {
         handler.HandleDetach(frameSize, this, payload, channel, context);
      }
   }
}
