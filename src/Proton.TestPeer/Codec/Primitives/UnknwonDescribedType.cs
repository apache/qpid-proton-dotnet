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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Primitives
{
   public class UnknownDescribedType : IDescribedType
   {
      private object descriptor;
      private object described;

      public UnknownDescribedType(object descriptor, object described)
      {
         this.descriptor = descriptor;
         this.described = described;
      }

      public object Descriptor => descriptor;

      public object Described => described;

      public override bool Equals(object obj)
      {
         return obj is UnknownDescribedType type &&
                EqualityComparer<object>.Default.Equals(Descriptor, type.Descriptor) &&
                EqualityComparer<object>.Default.Equals(Described, type.Described);
      }

      public override int GetHashCode()
      {
         return HashCode.Combine(Descriptor, Described);
      }

      public override String ToString()
      {
         return "UnknownDescribedType{" + "descriptor=" + descriptor + ", described=" + described + '}';
      }
   }
}