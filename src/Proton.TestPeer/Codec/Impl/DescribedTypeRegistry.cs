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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public static class DescribedTypeRegistry
   {
      private static IDictionary<object, Type> describedTypes = new Dictionary<object, Type>();

      static DescribedTypeRegistry()
      {
         describedTypes.Add(Accepted.DESCRIPTOR_CODE, typeof(Accepted));
         describedTypes.Add(Accepted.DESCRIPTOR_SYMBOL, typeof(Accepted));
         describedTypes.Add(Attach.DESCRIPTOR_CODE, typeof(Attach));
         describedTypes.Add(Attach.DESCRIPTOR_SYMBOL, typeof(Attach));
         describedTypes.Add(Begin.DESCRIPTOR_CODE, typeof(Begin));
         describedTypes.Add(Begin.DESCRIPTOR_SYMBOL, typeof(Begin));
         describedTypes.Add(Close.DESCRIPTOR_CODE, typeof(Close));
         describedTypes.Add(Close.DESCRIPTOR_SYMBOL, typeof(Close));
         describedTypes.Add(Coordinator.DESCRIPTOR_CODE, typeof(Coordinator));
         describedTypes.Add(Coordinator.DESCRIPTOR_SYMBOL, typeof(Coordinator));
         describedTypes.Add(Declare.DESCRIPTOR_CODE, typeof(Declare));
         describedTypes.Add(Declare.DESCRIPTOR_SYMBOL, typeof(Declare));
         describedTypes.Add(Declared.DESCRIPTOR_CODE, typeof(Declared));
         describedTypes.Add(Declared.DESCRIPTOR_SYMBOL, typeof(Declared));
         describedTypes.Add(DeleteOnClose.DESCRIPTOR_CODE, typeof(DeleteOnClose));
         describedTypes.Add(DeleteOnClose.DESCRIPTOR_SYMBOL, typeof(DeleteOnClose));
         describedTypes.Add(DeleteOnNoLinks.DESCRIPTOR_CODE, typeof(DeleteOnNoLinks));
         describedTypes.Add(DeleteOnNoLinks.DESCRIPTOR_SYMBOL, typeof(DeleteOnNoLinks));
         describedTypes.Add(DeleteOnNoLinksOrMessages.DESCRIPTOR_CODE, typeof(DeleteOnNoLinksOrMessages));
         describedTypes.Add(DeleteOnNoLinksOrMessages.DESCRIPTOR_SYMBOL, typeof(DeleteOnNoLinksOrMessages));
         describedTypes.Add(DeleteOnNoMessages.DESCRIPTOR_CODE, typeof(DeleteOnNoMessages));
         describedTypes.Add(DeleteOnNoMessages.DESCRIPTOR_SYMBOL, typeof(DeleteOnNoMessages));
         describedTypes.Add(Detach.DESCRIPTOR_CODE, typeof(Detach));
         describedTypes.Add(Detach.DESCRIPTOR_SYMBOL, typeof(Detach));
         describedTypes.Add(Discharge.DESCRIPTOR_CODE, typeof(Discharge));
         describedTypes.Add(Discharge.DESCRIPTOR_SYMBOL, typeof(Discharge));
         describedTypes.Add(Disposition.DESCRIPTOR_CODE, typeof(Disposition));
         describedTypes.Add(Disposition.DESCRIPTOR_SYMBOL, typeof(Disposition));
         describedTypes.Add(End.DESCRIPTOR_CODE, typeof(End));
         describedTypes.Add(End.DESCRIPTOR_SYMBOL, typeof(End));
         describedTypes.Add(ErrorCondition.DESCRIPTOR_CODE, typeof(ErrorCondition));
         describedTypes.Add(ErrorCondition.DESCRIPTOR_SYMBOL, typeof(ErrorCondition));
         describedTypes.Add(Flow.DESCRIPTOR_CODE, typeof(Flow));
         describedTypes.Add(Flow.DESCRIPTOR_SYMBOL, typeof(Flow));
         describedTypes.Add(Modified.DESCRIPTOR_CODE, typeof(Modified));
         describedTypes.Add(Modified.DESCRIPTOR_SYMBOL, typeof(Modified));
         describedTypes.Add(Open.DESCRIPTOR_CODE, typeof(Open));
         describedTypes.Add(Open.DESCRIPTOR_SYMBOL, typeof(Open));
         describedTypes.Add(Received.DESCRIPTOR_CODE, typeof(Received));
         describedTypes.Add(Received.DESCRIPTOR_SYMBOL, typeof(Received));
         describedTypes.Add(Rejected.DESCRIPTOR_CODE, typeof(Rejected));
         describedTypes.Add(Rejected.DESCRIPTOR_SYMBOL, typeof(Rejected));
         describedTypes.Add(Released.DESCRIPTOR_CODE, typeof(Released));
         describedTypes.Add(Released.DESCRIPTOR_SYMBOL, typeof(Released));
         describedTypes.Add(SaslChallenge.DESCRIPTOR_CODE, typeof(SaslChallenge));
         describedTypes.Add(SaslChallenge.DESCRIPTOR_SYMBOL, typeof(SaslChallenge));
         describedTypes.Add(SaslInit.DESCRIPTOR_CODE, typeof(SaslInit));
         describedTypes.Add(SaslInit.DESCRIPTOR_SYMBOL, typeof(SaslInit));
         describedTypes.Add(SaslMechanisms.DESCRIPTOR_CODE, typeof(SaslMechanisms));
         describedTypes.Add(SaslMechanisms.DESCRIPTOR_SYMBOL, typeof(SaslMechanisms));
         describedTypes.Add(SaslOutcome.DESCRIPTOR_CODE, typeof(SaslOutcome));
         describedTypes.Add(SaslOutcome.DESCRIPTOR_SYMBOL, typeof(SaslOutcome));
         describedTypes.Add(SaslResponse.DESCRIPTOR_CODE, typeof(SaslResponse));
         describedTypes.Add(SaslResponse.DESCRIPTOR_SYMBOL, typeof(SaslResponse));
         describedTypes.Add(Source.DESCRIPTOR_CODE, typeof(Source));
         describedTypes.Add(Source.DESCRIPTOR_SYMBOL, typeof(Source));
         describedTypes.Add(Target.DESCRIPTOR_CODE, typeof(Target));
         describedTypes.Add(Target.DESCRIPTOR_SYMBOL, typeof(Target));
         describedTypes.Add(TransactionalState.DESCRIPTOR_CODE, typeof(TransactionalState));
         describedTypes.Add(TransactionalState.DESCRIPTOR_SYMBOL, typeof(TransactionalState));
         describedTypes.Add(Transfer.DESCRIPTOR_CODE, typeof(Transfer));
         describedTypes.Add(Transfer.DESCRIPTOR_SYMBOL, typeof(Transfer));
         describedTypes.Add(AmqpSequence.DESCRIPTOR_CODE, typeof(AmqpSequence));
         describedTypes.Add(AmqpSequence.DESCRIPTOR_SYMBOL, typeof(AmqpSequence));
         describedTypes.Add(AmqpValue.DESCRIPTOR_CODE, typeof(AmqpValue));
         describedTypes.Add(AmqpValue.DESCRIPTOR_SYMBOL, typeof(AmqpValue));
         describedTypes.Add(ApplicationProperties.DESCRIPTOR_CODE, typeof(ApplicationProperties));
         describedTypes.Add(ApplicationProperties.DESCRIPTOR_SYMBOL, typeof(ApplicationProperties));
         describedTypes.Add(Data.DESCRIPTOR_CODE, typeof(Data));
         describedTypes.Add(Data.DESCRIPTOR_SYMBOL, typeof(Data));
         describedTypes.Add(DeliveryAnnotations.DESCRIPTOR_CODE, typeof(DeliveryAnnotations));
         describedTypes.Add(DeliveryAnnotations.DESCRIPTOR_SYMBOL, typeof(DeliveryAnnotations));
         describedTypes.Add(Footer.DESCRIPTOR_CODE, typeof(Footer));
         describedTypes.Add(Footer.DESCRIPTOR_SYMBOL, typeof(Footer));
         describedTypes.Add(Header.DESCRIPTOR_CODE, typeof(Header));
         describedTypes.Add(Header.DESCRIPTOR_SYMBOL, typeof(Header));
         describedTypes.Add(MessageAnnotations.DESCRIPTOR_CODE, typeof(MessageAnnotations));
         describedTypes.Add(MessageAnnotations.DESCRIPTOR_SYMBOL, typeof(MessageAnnotations));
         describedTypes.Add(Properties.DESCRIPTOR_CODE, typeof(Properties));
         describedTypes.Add(Properties.DESCRIPTOR_SYMBOL, typeof(Properties));
      }

      public static IDescribedType LookupDescribedType(object descriptor, object described)
      {
         Type typeClass;

         if (describedTypes.TryGetValue(descriptor, out typeClass))
         {
            try
            {
               return (IDescribedType)Activator.CreateInstance(typeClass);
            }
            catch (Exception)
            {
            }
         }

         return new DescribedType(descriptor, described);
      }
   }
}