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
using System.Threading;
using Apache.Qpid.Proton.Client.Exceptions;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientExceptionSupportTest
   {
      [Test]
      public void TestClientIOExceptionPassesThrough()
      {
         ClientIOException ioError = new ClientIOException("Fatal IO Error");

         Assert.AreSame(ioError, ClientExceptionSupport.CreateOrPassthroughFatal(ioError));
      }

      [Test]
      public void TestClientExceptionNotPassesThrough()
      {
         ClientException error = new ClientException("Fatal IO Error");

         Assert.AreNotSame(error, ClientExceptionSupport.CreateOrPassthroughFatal(error));
         Assert.IsTrue(ClientExceptionSupport.CreateOrPassthroughFatal(error) is ClientIOException);
      }

      [Test]
      public void TestErrorMessageTakenFromToStringIfNotPresentInException()
      {
         Exception error = new TestException();

         Assert.AreNotSame(error, ClientExceptionSupport.CreateOrPassthroughFatal(error));
         Assert.AreEqual("expected", ClientExceptionSupport.CreateOrPassthroughFatal(error).Message);
      }

      [Test]
      public void TestErrorMessageTakenFromToStringIfEmptyInException()
      {
         Exception error = new TestException();

         Assert.AreNotSame(error, ClientExceptionSupport.CreateOrPassthroughFatal(error));
         Assert.AreEqual("expected", ClientExceptionSupport.CreateOrPassthroughFatal(error).Message);
      }

      [Test]
      public void TestCauseIsClientIOExceptionExtractedAndPassedThrough()
      {
         ClientException error = new ClientException("Fatal IO Error", new ClientIOException("real error"));

         Assert.AreNotSame(error, ClientExceptionSupport.CreateOrPassthroughFatal(error));
         Assert.AreSame(error.InnerException, ClientExceptionSupport.CreateOrPassthroughFatal(error));
      }

      [Test]
      public void TestClientExceptionPassesThrough()
      {
         ClientIOException error = new ClientIOException("Non Fatal Error");

         Assert.AreSame(error, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
      }

      [Test]
      public void TestClientIOExceptionPassesThroughNonFatalCreate()
      {
         ClientIOException error = new ClientIOException("Fatal IO Error");

         Assert.AreSame(error, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
      }

      [Test]
      public void TestErrorMessageTakenFromToStringIfNotPresentInExceptionFromNonFatalCreate()
      {
         Exception error = new TestException();

         Assert.AreNotSame(error, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
         Assert.AreEqual("expected", ClientExceptionSupport.CreateNonFatalOrPassthrough(error).Message);
      }

      [Test]
      public void TestErrorMessageTakenFromToStringIfEmptyInExceptionFromNonFatalCreate()
      {
         Exception error = new TestException();

         Assert.AreNotSame(error, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
         Assert.AreEqual("expected", ClientExceptionSupport.CreateNonFatalOrPassthrough(error).Message);
      }

      [Test]
      public void TestCauseIsClientIOExceptionExtractedAndPassedThroughFromNonFatalCreate()
      {
         Exception error = new ArgumentException("Fatal IO Error", new ClientIOException("real error"));

         Assert.AreNotSame(error, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
         Assert.AreSame(error.InnerException, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
      }

      [Test]
      public void TestTimeoutExceptionConvertedToClientEquivalent()
      {
         TimeoutException error = new TimeoutException("timeout");

         Assert.AreNotSame(error, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
         Assert.IsTrue(ClientExceptionSupport.CreateNonFatalOrPassthrough(error) is ClientOperationTimedOutException);
      }

      [Test]
      public void TestIllegalStateExceptionConvertedToClientEquivalent()
      {
         InvalidOperationException error = new InvalidOperationException("timeout");

         Assert.AreNotSame(error, ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
         Assert.IsTrue(ClientExceptionSupport.CreateNonFatalOrPassthrough(error) is ClientIllegalStateException);
      }

      internal class TestException : Exception
      {
         public TestException() : base("expected")
         {
         }

         public override string ToString()
         {
            return "expected";
         }
      }
   }
}