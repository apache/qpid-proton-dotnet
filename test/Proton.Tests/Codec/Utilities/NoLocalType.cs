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

using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Utilities
{
   public class NoLocalType : IDescribedType
   {
      public static readonly NoLocalType Instance = new NoLocalType();

      public static readonly ulong DescriptorCode = 0x0000468C00000003UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("apache.org:no-local-filter:list");

      public NoLocalType() : base()
      {
         NoLocal = "NoLocalFilter{}";
      }

      public string NoLocal { get; set; }

      public object Descriptor => DescriptorCode;

      public object Described => NoLocal;

   }
}