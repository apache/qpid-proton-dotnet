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

using System.Threading;
using NUnit.Framework;
using Apache.Qpid.Proton.Types;
using Moq;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientConnectionCapabilitiesTest
   {
      public static readonly Symbol[] ANONYMOUS_RELAY = new Symbol[] { Symbol.Lookup("ANONYMOUS-RELAY") };
      public static readonly Symbol[] DELAYED_DELIVERY = new Symbol[] { Symbol.Lookup("DELAYED_DELIVERY") };
      public static readonly Symbol[] ANONYMOUS_RELAY_PLUS = new Symbol[] { Symbol.Lookup("ANONYMOUS-RELAY"),
                                                                            Symbol.Lookup("DELAYED_DELIVERY")};

      [Test]
      public void TestAnonymousRelaySupportedIsFalseByDefault()
      {
         ClientConnectionCapabilities capabilities = new ClientConnectionCapabilities();

         Assert.IsFalse(capabilities.AnonymousRelaySupported);
      }

      [Test]
      public void TestAnonymousRelaySupportedWhenBothIndicateInCapabilities()
      {
         DoTestIsAnonymousRelaySupported(ANONYMOUS_RELAY, ANONYMOUS_RELAY, true);
      }

      [Test]
      public void TestAnonymousRelaySupportedWhenBothIndicateInCapabilitiesAlongWithOthers()
      {
         DoTestIsAnonymousRelaySupported(ANONYMOUS_RELAY, ANONYMOUS_RELAY_PLUS, true);
      }

      [Test]
      public void TestAnonymousRelayNotSupportedWhenServerDoesNotAdvertiseIt()
      {
         DoTestIsAnonymousRelaySupported(ANONYMOUS_RELAY, null, false);
      }

      [Test]
      public void TestAnonymousRelaySupportedWhenServerAdvertisesButClientDoesNotRequestIt()
      {
         DoTestIsAnonymousRelaySupported(null, ANONYMOUS_RELAY, true);
      }

      [Test]
      public void TestAnonymousRelayNotSupportedWhenNeitherSideIndicatesIt()
      {
         DoTestIsAnonymousRelaySupported(null, null, false);
      }

      private void DoTestIsAnonymousRelaySupported(Symbol[] desired, Symbol[] offered, bool expectation)
      {
         ClientConnectionCapabilities capabilities = new ClientConnectionCapabilities();

         var connection = new Mock<Engine.IConnection>();
         connection.Setup(con => con.DesiredCapabilities).Returns(desired);
         connection.Setup(con => con.RemoteOfferedCapabilities).Returns(offered);

         capabilities.DetermineCapabilities(connection.Object);

         if (expectation)
         {
            Assert.IsTrue(capabilities.AnonymousRelaySupported);
         }
         else
         {
            Assert.IsFalse(capabilities.AnonymousRelaySupported);
         }
      }

      [Test]
      public void TestDelayedDeliverySupportedWhenBothIndicateInCapabilities()
      {
         DoTestIsDelayedDeliverySupported(DELAYED_DELIVERY, DELAYED_DELIVERY, true);
      }

      [Test]
      public void TestDelayedDeliverySupportedWhenBothIndicateInCapabilitiesAlongWithOthers()
      {
         DoTestIsDelayedDeliverySupported(DELAYED_DELIVERY, ANONYMOUS_RELAY_PLUS, true);
      }

      [Test]
      public void TestDelayedDeliveryNotSupportedWhenServerDoesNotAdvertiseIt()
      {
         DoTestIsDelayedDeliverySupported(DELAYED_DELIVERY, null, false);
      }

      [Test]
      public void TestDelayedDeliverySupportedWhenServerAdvertisesButClientDoesNotRequestIt()
      {
         DoTestIsDelayedDeliverySupported(null, DELAYED_DELIVERY, true);
      }

      [Test]
      public void TestDelayedDeliveryNotSupportedWhenNeitherSideIndicatesIt()
      {
         DoTestIsDelayedDeliverySupported(null, null, false);
      }

      private void DoTestIsDelayedDeliverySupported(Symbol[] desired, Symbol[] offered, bool expectation)
      {
         ClientConnectionCapabilities capabilities = new ClientConnectionCapabilities();

         var connection = new Mock<Engine.IConnection>();
         connection.Setup(con => con.DesiredCapabilities).Returns(desired);
         connection.Setup(con => con.RemoteOfferedCapabilities).Returns(offered);


         capabilities.DetermineCapabilities(connection.Object);

         if (expectation)
         {
            Assert.IsTrue(capabilities.DeliveryDelaySupported);
         }
         else
         {
            Assert.IsFalse(capabilities.DeliveryDelaySupported);
         }
      }
   }
}