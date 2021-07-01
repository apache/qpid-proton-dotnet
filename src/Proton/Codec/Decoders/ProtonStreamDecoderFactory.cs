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

using Apache.Qpid.Proton.Codec.Decoders.Messaging;
using Apache.Qpid.Proton.Codec.Decoders.Security;
using Apache.Qpid.Proton.Codec.Decoders.Transactions;
using Apache.Qpid.Proton.Codec.Decoders.Transport;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   /// <summary>
   /// Defines a factory class that creates Proton specific Decoder types.
   /// </summary>
   public sealed class ProtonStreamDecoderFactory
   {
      public static ProtonStreamDecoder Create()
      {
         ProtonStreamDecoder decoder = new ProtonStreamDecoder();

         AddMessagingTypeDecoders(decoder);
         AddTransactionTypeDecoders(decoder);
         AddTransportTypeDecoders(decoder);

         return decoder;
      }

      public static ProtonStreamDecoder CreateSasl()
      {
         ProtonStreamDecoder decoder = new ProtonStreamDecoder();

         AddSaslTypeDecoders(decoder);

         return decoder;
      }

      private static void AddMessagingTypeDecoders(ProtonStreamDecoder Decoder)
      {
         Decoder.RegisterDescribedTypeDecoder(new AcceptedTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new AmqpSequenceTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new AmqpValueTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new ApplicationPropertiesTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DataTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DeleteOnCloseTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DeleteOnNoLinksOrMessagesTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DeleteOnNoLinksTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DeleteOnNoMessagesTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DeliveryAnnotationsTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new FooterTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new HeaderTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new MessageAnnotationsTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new ModifiedTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new PropertiesTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new ReceivedTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new RejectedTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new ReleasedTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new SourceTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new TargetTypeDecoder());
      }

      private static void AddTransactionTypeDecoders(ProtonStreamDecoder Decoder)
      {
         Decoder.RegisterDescribedTypeDecoder(new CoordinatorTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DeclaredTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DeclareTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DischargeTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new TransactionalStateTypeDecoder());
      }

      private static void AddTransportTypeDecoders(ProtonStreamDecoder Decoder)
      {
         Decoder.RegisterDescribedTypeDecoder(new AttachTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new BeginTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new CloseTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DetachTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new DispositionTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new EndTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new ErrorConditionTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new FlowTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new OpenTypeDecoder());
         Decoder.RegisterDescribedTypeDecoder(new TransferTypeDecoder());
      }

      private static void AddSaslTypeDecoders(ProtonStreamDecoder decoder)
      {
         decoder.RegisterDescribedTypeDecoder(new SaslChallengeTypeDecoder());
         decoder.RegisterDescribedTypeDecoder(new SaslInitTypeDecoder());
         decoder.RegisterDescribedTypeDecoder(new SaslMechanismsTypeDecoder());
         decoder.RegisterDescribedTypeDecoder(new SaslOutcomeTypeDecoder());
         decoder.RegisterDescribedTypeDecoder(new SaslResponseTypeDecoder());
      }
   }
}