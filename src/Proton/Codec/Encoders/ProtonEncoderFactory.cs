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

using Apache.Qpid.Proton.Codec.Encoders.Messaging;
using Apache.Qpid.Proton.Codec.Encoders.Security;
using Apache.Qpid.Proton.Codec.Encoders.Transactions;
using Apache.Qpid.Proton.Codec.Encoders.Transport;

namespace Apache.Qpid.Proton.Codec.Encoders
{
   /// <summary>
   /// Defines a factory class that creates Proton specific Encoder types.
   /// </summary>
   public sealed class ProtonEncoderFactory
   {
      public static ProtonEncoder Create()
      {
         ProtonEncoder encoder = new();

         AddMessagingTypeEncoders(encoder);
         AddTransactionTypeEncoders(encoder);
         AddTransportTypeEncoders(encoder);

         return encoder;
      }

      public static ProtonEncoder CreateSasl()
      {
         return AddSaslTypeEncoders(new ProtonEncoder());
      }

      private static ProtonEncoder AddMessagingTypeEncoders(ProtonEncoder encoder)
      {
         encoder.RegisterDescribedTypeEncoder(new AcceptedTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new AmqpSequenceTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new AmqpValueTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new ApplicationPropertiesTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DataTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DeleteOnCloseTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DeleteOnNoLinksOrMessagesTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DeleteOnNoLinksTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DeleteOnNoMessagesTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DeliveryAnnotationsTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new FooterTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new HeaderTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new MessageAnnotationsTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new ModifiedTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new PropertiesTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new ReceivedTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new RejectedTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new ReleasedTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new SourceTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new TargetTypeEncoder());

         return encoder;
      }

      private static ProtonEncoder AddTransactionTypeEncoders(ProtonEncoder encoder)
      {
         encoder.RegisterDescribedTypeEncoder(new CoordinatorTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DeclaredTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DeclareTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DischargeTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new TransactionalStateTypeEncoder());

         return encoder;
      }

      private static ProtonEncoder AddTransportTypeEncoders(ProtonEncoder encoder)
      {
         encoder.RegisterDescribedTypeEncoder(new AttachTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new BeginTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new CloseTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DetachTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new DispositionTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new EndTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new ErrorConditionTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new FlowTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new OpenTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new TransferTypeEncoder());

         return encoder;
      }

      private static ProtonEncoder AddSaslTypeEncoders(ProtonEncoder encoder)
      {
         encoder.RegisterDescribedTypeEncoder(new SaslChallengeTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new SaslInitTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new SaslMechanismsTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new SaslOutcomeTypeEncoder());
         encoder.RegisterDescribedTypeEncoder(new SaslResponseTypeEncoder());

         return encoder;
      }
   }
}