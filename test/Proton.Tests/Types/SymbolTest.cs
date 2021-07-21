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

using System.Text;
using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types
{
   [TestFixture]
   public class SymbolTest
   {
      private readonly string LARGE_SYMBOL_VALUE = "Large string: " +
                                                   "The quick brown fox jumps over the lazy dog. " +
                                                   "The quick brown fox jumps over the lazy dog. " +
                                                   "The quick brown fox jumps over the lazy dog. " +
                                                   "The quick brown fox jumps over the lazy dog. " +
                                                   "The quick brown fox jumps over the lazy dog. " +
                                                   "The quick brown fox jumps over the lazy dog. " +
                                                   "The quick brown fox jumps over the lazy dog. " +
                                                   "The quick brown fox jumps over the lazy dog.";

      [Test]
      public void TestGetSymbolWithNullString()
      {
         Assert.IsNull(Symbol.Lookup((string)null));
      }

      [Test]
      public void TestGetSymbolWithNullBuffer()
      {
         Assert.IsNull(Symbol.Lookup((IProtonBuffer)null));
      }

      [Test]
      public void TestGetSymbolWithEmptyString()
      {
         Assert.IsNotNull(Symbol.Lookup(""));
         Assert.AreSame(Symbol.Lookup(""), Symbol.Lookup(""));
      }

      [Test]
      public void TestGetSymbolWithEmptyBuffer()
      {
         Assert.IsNotNull(Symbol.Lookup(ProtonByteBufferAllocator.Instance.Allocate(0)));
         Assert.AreSame(Symbol.Lookup(ProtonByteBufferAllocator.Instance.Allocate(0)),
                        Symbol.Lookup(ProtonByteBufferAllocator.Instance.Allocate(0)));
      }

      [Test]
      public void TestCompareTo()
      {
         string symbolString1 = "Symbol-1";
         string symbolString2 = "Symbol-2";
         string symbolString3 = "Symbol-3";

         Symbol symbol1 = Symbol.Lookup(symbolString1);
         Symbol symbol2 = Symbol.Lookup(symbolString2);
         Symbol symbol3 = Symbol.Lookup(symbolString3);

         Assert.AreEqual(0, symbol1.CompareTo(symbol1));
         Assert.AreEqual(0, symbol2.CompareTo(symbol2));
         Assert.AreEqual(0, symbol3.CompareTo(symbol3));

         Assert.IsTrue(symbol2.CompareTo(symbol1) > 0);
         Assert.IsTrue(symbol3.CompareTo(symbol1) > 0);
         Assert.IsTrue(symbol3.CompareTo(symbol2) > 0);

         Assert.IsTrue(symbol1.CompareTo(symbol2) < 0);
         Assert.IsTrue(symbol1.CompareTo(symbol3) < 0);
         Assert.IsTrue(symbol2.CompareTo(symbol3) < 0);
      }

      [Test]
      public void TestEquals()
      {
         string symbolString1 = "Symbol-1";
         string symbolString2 = "Symbol-2";
         string symbolString3 = "Symbol-3";

         Symbol symbol1 = Symbol.Lookup(symbolString1);
         Symbol symbol2 = Symbol.Lookup(symbolString2);
         Symbol symbol3 = Symbol.Lookup(symbolString3);

         Assert.AreNotEqual(symbol1, symbol2);

         Assert.AreEqual(symbolString1, symbol1.ToString());
         Assert.AreEqual(symbolString2, symbol2.ToString());
         Assert.AreEqual(symbolString3, symbol3.ToString());

         Assert.AreNotEqual(symbol1, symbol2);
         Assert.AreNotEqual(symbol2, symbol3);
         Assert.AreNotEqual(symbol3, symbol1);

         Assert.AreNotEqual(symbolString1, symbol1);
         Assert.AreNotEqual(symbolString2, symbol2);
         Assert.AreNotEqual(symbolString3, symbol3);
      }

      [Test]
      public void TestHashcode()
      {
         string symbolString1 = "Symbol-1";
         string symbolString2 = "Symbol-2";

         Symbol symbol1 = Symbol.Lookup(symbolString1);
         Symbol symbol2 = Symbol.Lookup(symbolString2);

         Assert.AreNotEqual(symbol1, symbol2);
         Assert.AreNotEqual(symbol1.GetHashCode(), symbol2.GetHashCode());

         Assert.AreEqual(symbol1.GetHashCode(), Symbol.Lookup(symbolString1).GetHashCode());
         Assert.AreEqual(symbol2.GetHashCode(), Symbol.Lookup(symbolString2).GetHashCode());
      }

      [Test]
      public void TestValueOf()
      {
         string symbolString1 = "Symbol-1";
         string symbolString2 = "Symbol-2";

         Symbol symbol1 = Symbol.Lookup(symbolString1);
         Symbol symbol2 = Symbol.Lookup(symbolString2);

         Assert.AreNotEqual(symbol1, symbol2);

         Assert.AreEqual(symbolString1, symbol1.ToString());
         Assert.AreEqual(symbolString2, symbol2.ToString());
      }

      [Test]
      public void TestValueOfProducesSingleton()
      {
         string symbolString = "Symbol-string";

         Symbol symbol1 = Symbol.Lookup(symbolString);
         Symbol symbol2 = Symbol.Lookup(symbolString);

         Assert.AreEqual(symbolString, symbol1.ToString());
         Assert.AreEqual(symbolString, symbol2.ToString());

         Assert.AreSame(symbol1, symbol2);
      }

      [Test]
      public void TestGetSymbol()
      {
         string symbolString1 = "Symbol-1";
         string symbolString2 = "Symbol-2";

         Symbol symbol1 = Symbol.Lookup(symbolString1);
         Symbol symbol2 = Symbol.Lookup(symbolString2);

         Assert.AreNotEqual(symbol1, symbol2);

         Assert.AreEqual(symbolString1, symbol1.ToString());
         Assert.AreEqual(symbolString2, symbol2.ToString());
      }

      [Test]
      public void TestGetSymbolProducesSingleton()
      {
         string symbolString = "Symbol-string";

         Symbol symbol1 = Symbol.Lookup(symbolString);
         Symbol symbol2 = Symbol.Lookup(symbolString);

         Assert.AreEqual(symbolString, symbol1.ToString());
         Assert.AreEqual(symbolString, symbol2.ToString());

         Assert.AreSame(symbol1, symbol2);
      }

      [Test]
      public void TestGetSymbolAndValueOfProduceSingleton()
      {
         string symbolString = "Symbol-string";

         Symbol symbol1 = Symbol.Lookup(symbolString);
         Symbol symbol2 = Symbol.Lookup(symbolString);

         Assert.AreEqual(symbolString, symbol1.ToString());
         Assert.AreEqual(symbolString, symbol2.ToString());

         Assert.AreSame(symbol1, symbol2);
      }

      [Test]
      public void TestToStringProducesSingelton()
      {
         string symbolString = "Symbol-string";

         Symbol symbol1 = Symbol.Lookup(symbolString);
         Symbol symbol2 = Symbol.Lookup(symbolString);

         Assert.AreEqual(symbolString, symbol1.ToString());
         Assert.AreEqual(symbolString, symbol2.ToString());

         Assert.AreSame(symbol1, symbol2);
         Assert.AreSame(symbol1.ToString(), symbol2.ToString());
      }

      [Test]
      public void TestLargeSymbolNotCached()
      {
         Encoding ASCII = new ASCIIEncoding();

         Symbol symbol1 = Symbol.Lookup(LARGE_SYMBOL_VALUE);
         Symbol symbol2 = Symbol.Lookup(
             ProtonByteBufferAllocator.Instance.Wrap(ASCII.GetBytes(LARGE_SYMBOL_VALUE)));

         Assert.AreNotSame(symbol1, symbol2);
         Assert.AreNotSame(symbol1.ToString(), symbol2.ToString());
      }
   }
}