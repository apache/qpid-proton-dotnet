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
using System.Threading;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Utilities
{
   public class IdGeneratorTests
   {
      private IdGenerator generator;

      [SetUp]
      public void Setup()
      {
         generator = new IdGenerator();
      }

      [Test, Timeout(20000)]
      public void TestDefaultPrefix()
      {
         string generated = generator.GenerateId();
         Assert.True(generated.StartsWith(IdGenerator.DefaultPrefix));
         Assert.False(generated.Substring(IdGenerator.DefaultPrefix.Length).StartsWith(":"));
      }

      [Test, Timeout(20000)]
      public void TestNonDefaultPrefix()
      {
         generator = new IdGenerator("TEST-");
         string generated = generator.GenerateId();
         Assert.False(generated.StartsWith(IdGenerator.DefaultPrefix));
         Assert.False(generated.Substring("TEST-".Length).StartsWith(":"));
      }

      [Test, Timeout(20000)]
      public void TestIdIndexIncrements()
      {
         const int COUNT = 5;

         List<string> ids = new List<string>(COUNT);
         List<int> sequences = new List<int>();

         for (int i = 0; i < COUNT; ++i)
         {
            ids.Add(generator.GenerateId());
         }

         foreach (string id in ids)
         {
            String[] components = id.Split(":");
            sequences.Add(Int32.Parse(components[components.Length - 1]));
         }

         int? lastValue = null;
         foreach (int sequence in sequences)
         {
            if (lastValue != null)
            {
               Assert.True(sequence > lastValue);
            }

            lastValue = sequence;
         }
      }
   }
}