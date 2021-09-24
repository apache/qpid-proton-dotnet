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
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonEnginePipelineTest
   {
      private ProtonEngine engine;

      [SetUp]
      public void SetUp()
      {
         engine = new ProtonEngine();
      }

      [Test]
      public void TestCreatePipeline()
      {
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         Assert.AreSame(pipeline.Engine, engine);
         Assert.IsNull(pipeline.First());
         Assert.IsNull(pipeline.Last());
         Assert.IsNull(pipeline.FirstContext());
         Assert.IsNull(pipeline.LastContext());
      }

      [Test]
      public void TestCreatePipelineRejectsNullParent()
      {
         try
         {
            new ProtonEnginePipeline(null);
            Assert.Fail("Should throw an ArgumentException");
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestAddFirstRejectsNullHandler()
      {
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         try
         {
            pipeline.AddFirst("one", null);
            Assert.Fail("Should throw an ArgumentException");
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestAddFirstRejectsNullHandlerName()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         try
         {
            pipeline.AddFirst(null, handler);
            Assert.Fail("Should throw an ArgumentException");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestAddFirstRejectsEmptyHandlerName()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         try
         {
            pipeline.AddFirst("", handler);
            Assert.Fail("Should throw an ArgumentException");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestAddFirstWithOneHandler()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("one", handler);

         Assert.AreSame(handler, pipeline.First());
         Assert.AreSame(handler, pipeline.Last());
         Assert.IsNotNull(pipeline.FirstContext());
         Assert.AreSame(handler, pipeline.FirstContext().Handler);
      }

      [Test]
      public void TestAddFirstWithMoreThanOneHandler()
      {
         IEngineHandler handler1 = new ProtonFrameEncodingHandler();
         IEngineHandler handler2 = new ProtonFrameDecodingHandler();
         IEngineHandler handler3 = new ProtonFrameEncodingHandler();

         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("three", handler3);
         pipeline.AddFirst("two", handler2);
         pipeline.AddFirst("one", handler1);

         Assert.AreSame(handler1, pipeline.First());
         Assert.AreSame(handler3, pipeline.Last());

         Assert.IsNotNull(pipeline.FirstContext());
         Assert.AreSame(handler1, pipeline.FirstContext().Handler);
         Assert.IsNotNull(pipeline.LastContext());
         Assert.AreSame(handler3, pipeline.LastContext().Handler);
      }

      [Test]
      public void TestAddLastRejectsNullHandler()
      {
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         try
         {
            pipeline.AddLast("one", null);
            Assert.Fail("Should throw an ArgumentException");
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestAddLastRejectsNullHandlerName()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         try
         {
            pipeline.AddLast(null, handler);
            Assert.Fail("Should throw an ArgumentException");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestAddLastRejectsEmptyHandlerName()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         try
         {
            pipeline.AddLast("", handler);
            Assert.Fail("Should throw an ArgumentException");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestAddLastWithOneHandler()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();

         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddLast("one", handler);

         Assert.AreSame(handler, pipeline.First());
         Assert.AreSame(handler, pipeline.Last());

         Assert.IsNotNull(pipeline.FirstContext());
         Assert.AreSame(handler, pipeline.FirstContext().Handler);
         Assert.IsNotNull(pipeline.LastContext());
         Assert.AreSame(handler, pipeline.LastContext().Handler);
      }

      [Test]
      public void TestAddLastWithMoreThanOneHandler()
      {
         IEngineHandler handler1 = new ProtonFrameEncodingHandler();
         IEngineHandler handler2 = new ProtonFrameDecodingHandler();
         IEngineHandler handler3 = new ProtonFrameEncodingHandler();

         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddLast("one", handler1);
         pipeline.AddLast("two", handler2);
         pipeline.AddLast("three", handler3);

         Assert.AreSame(handler1, pipeline.First());
         Assert.AreSame(handler3, pipeline.Last());

         Assert.IsNotNull(pipeline.FirstContext());
         Assert.AreSame(handler1, pipeline.FirstContext().Handler);
         Assert.IsNotNull(pipeline.LastContext());
         Assert.AreSame(handler3, pipeline.LastContext().Handler);
      }

      [Test]
      public void TestRemoveFirstWithOneHandler()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("one", handler);

         Assert.IsNotNull(pipeline.First());
         Assert.AreSame(pipeline, pipeline.RemoveFirst());
         Assert.IsNull(pipeline.First());
         // calling when empty should not throw.
         Assert.AreSame(pipeline, pipeline.RemoveFirst());
      }

      [Test]
      public void TestRemoveFirstWithMoreThanOneHandler()
      {
         IEngineHandler handler1 = new ProtonFrameEncodingHandler();
         IEngineHandler handler2 = new ProtonFrameDecodingHandler();
         IEngineHandler handler3 = new ProtonFrameEncodingHandler();

         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("three", handler3);
         pipeline.AddFirst("two", handler2);
         pipeline.AddFirst("one", handler1);

         Assert.AreSame(pipeline, pipeline.RemoveFirst());
         Assert.AreSame(handler2, pipeline.First());
         Assert.AreSame(pipeline, pipeline.RemoveFirst());
         Assert.AreSame(handler3, pipeline.First());
         Assert.AreSame(pipeline, pipeline.RemoveFirst());
         // calling when empty should not throw.
         Assert.AreSame(pipeline, pipeline.RemoveFirst());
         Assert.IsNull(pipeline.First());
      }

      [Test]
      public void TestRemoveLastWithOneHandler()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("one", handler);

         Assert.IsNotNull(pipeline.First());
         Assert.AreSame(pipeline, pipeline.RemoveLast());
         Assert.IsNull(pipeline.First());
         // calling when empty should not throw.
         Assert.AreSame(pipeline, pipeline.RemoveLast());
      }

      [Test]
      public void TestRemoveLastWithMoreThanOneHandler()
      {
         IEngineHandler handler1 = new ProtonFrameEncodingHandler();
         IEngineHandler handler2 = new ProtonFrameDecodingHandler();
         IEngineHandler handler3 = new ProtonFrameEncodingHandler();

         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("three", handler3);
         pipeline.AddFirst("two", handler2);
         pipeline.AddFirst("one", handler1);

         Assert.AreSame(pipeline, pipeline.RemoveLast());
         Assert.AreSame(handler2, pipeline.Last());
         Assert.AreSame(pipeline, pipeline.RemoveLast());
         Assert.AreSame(handler1, pipeline.Last());
         Assert.AreSame(pipeline, pipeline.RemoveLast());
         // calling when empty should not throw.
         Assert.AreSame(pipeline, pipeline.RemoveLast());
         Assert.IsNull(pipeline.Last());
      }

      [Test]
      public void TestRemoveWhenEmpty()
      {
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         Assert.AreSame(pipeline, pipeline.Remove("unknown"));
         Assert.AreSame(pipeline, pipeline.Remove(""));
         Assert.AreSame(pipeline, pipeline.Remove((string)null));
         Assert.AreSame(pipeline, pipeline.Remove((IEngineHandler)null));
      }

      [Test]
      public void TestRemoveWithOneHandler()
      {
         IEngineHandler handler = new ProtonFrameEncodingHandler();
         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("one", handler);

         Assert.AreSame(handler, pipeline.First());

         Assert.AreSame(pipeline, pipeline.Remove("unknown"));
         Assert.AreSame(pipeline, pipeline.Remove(""));
         Assert.AreSame(pipeline, pipeline.Remove((string)null));
         Assert.AreSame(pipeline, pipeline.Remove((IEngineHandler)null));

         Assert.AreSame(handler, pipeline.First());
         Assert.AreSame(pipeline, pipeline.Remove("one"));

         Assert.IsNull(pipeline.First());
         Assert.IsNull(pipeline.Last());

         pipeline.AddFirst("one", handler);

         Assert.AreSame(handler, pipeline.First());

         Assert.AreSame(pipeline, pipeline.Remove(handler));

         Assert.IsNull(pipeline.First());
         Assert.IsNull(pipeline.Last());
      }

      [Test]
      public void TestRemoveWithMoreThanOneHandler()
      {
         IEngineHandler handler1 = new ProtonFrameEncodingHandler();
         IEngineHandler handler2 = new ProtonFrameDecodingHandler();
         IEngineHandler handler3 = new ProtonFrameEncodingHandler();

         ProtonEnginePipeline pipeline = new ProtonEnginePipeline(engine);

         pipeline.AddFirst("three", handler3);
         pipeline.AddFirst("two", handler2);
         pipeline.AddFirst("one", handler1);

         Assert.AreSame(handler1, pipeline.First());
         Assert.AreSame(pipeline, pipeline.Remove("one"));
         Assert.AreSame(handler2, pipeline.First());
         Assert.AreSame(pipeline, pipeline.Remove("two"));
         Assert.AreSame(handler3, pipeline.First());
         Assert.AreSame(pipeline, pipeline.Remove("three"));

         Assert.IsNull(pipeline.First());
         Assert.IsNull(pipeline.Last());

         pipeline.AddFirst("three", handler3);
         pipeline.AddFirst("two", handler2);
         pipeline.AddFirst("one", handler1);

         Assert.AreSame(handler1, pipeline.First());
         Assert.AreSame(pipeline, pipeline.Remove(handler1));
         Assert.AreSame(handler2, pipeline.First());
         Assert.AreSame(pipeline, pipeline.Remove(handler2));
         Assert.AreSame(handler3, pipeline.First());
         Assert.AreSame(pipeline, pipeline.Remove(handler3));

         Assert.IsNull(pipeline.First());
         Assert.IsNull(pipeline.Last());
      }
   }
}