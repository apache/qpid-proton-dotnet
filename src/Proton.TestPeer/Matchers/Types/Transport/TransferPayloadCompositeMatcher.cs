/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed With
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance With
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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class TransferPayloadCompositeMatcher : TypeSafeMatcher<byte[]>
   {
      private HeaderMatcher headersMatcher;
      private string headerMatcherFailureDescription;
      private DeliveryAnnotationsMatcher deliveryAnnotationsMatcher;
      private string deliveryAnnotationsMatcherFailureDescription;
      private MessageAnnotationsMatcher messageAnnotationsMatcher;
      private string messageAnnotationsMatcherFailureDescription;
      private PropertiesMatcher propertiesMatcher;
      private string propertiesMatcherFailureDescription;
      private ApplicationPropertiesMatcher applicationPropertiesMatcher;
      private string applicationPropertiesMatcherFailureDescription;
      private List<TypeSafeMatcher<Stream>> msgContentMatchers = new List<TypeSafeMatcher<Stream>>();
      private string msgContentMatcherFailureDescription;
      private FooterMatcher footersMatcher;
      private string footerMatcherFailureDescription;
      private TypeSafeMatcher<int> payloadLengthMatcher;
      private string payloadLengthMatcherFailureDescription;

      public HeaderMatcher HeaderMatcher
      {
         get => headersMatcher;
         set => headersMatcher = value;
      }

      public DeliveryAnnotationsMatcher DeliveryAnnotationsMatcher
      {
         get => deliveryAnnotationsMatcher;
         set => deliveryAnnotationsMatcher = value;
      }

      public MessageAnnotationsMatcher MessageAnnotationsMatcher
      {
         get => messageAnnotationsMatcher;
         set => messageAnnotationsMatcher = value;
      }

      public PropertiesMatcher PropertiesMatcher
      {
         get => propertiesMatcher;
         set => propertiesMatcher = value;
      }

      public ApplicationPropertiesMatcher ApplicationPropertiesMatcher
      {
         get => applicationPropertiesMatcher;
         set => applicationPropertiesMatcher = value;
      }

      public FooterMatcher FooterMatcher
      {
         get => footersMatcher;
         set => footersMatcher = value;
      }

      public TypeSafeMatcher<int> PayloadLengthMatcher
      {
         get => payloadLengthMatcher;
         set => payloadLengthMatcher = value;
      }

      public List<TypeSafeMatcher<Stream>> MessageContentMatchers
      {
         get => msgContentMatchers;
         set => msgContentMatchers = value;
      }

      public TypeSafeMatcher<Stream> MessageContentMatcher
      {
         get
         {
            if (msgContentMatchers.Count == 0)
            {
               return null;
            }
            else if (msgContentMatchers.Count == 1)
            {
               return msgContentMatchers[0];
            }
            else
            {
               throw new InvalidOperationException("More than one matcher configured for message contents");
            }
         }
         set
         {
            if (msgContentMatchers.Count == 0)
            {
               msgContentMatchers.Add(value);
            }
            else
            {
               msgContentMatchers[0] = value;
            }
         }
      }

      public void AddMessageContentMatcher(TypeSafeMatcher<Stream> matcher)
      {
         MessageContentMatchers.Add(matcher);
      }

      public override void DescribeTo(IDescription description)
      {
         description.AppendText("a Binary encoding of a Transfer frames payload, containing an AMQP message");
      }

      protected override void DescribeMismatchSafely(byte[] item, IDescription mismatchDescription)
      {
         mismatchDescription.AppendText("\nActual encoded form of the full Transfer frame payload: ").AppendValue(item);

         // Payload Length
         if (payloadLengthMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nPayloadLengthMatcherFailed!");
            mismatchDescription.AppendText(payloadLengthMatcherFailureDescription);
            return;
         }

         // MessageHeaders Section
         if (headerMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nMessageHeadersMatcherFailed!");
            mismatchDescription.AppendText(headerMatcherFailureDescription);
            return;
         }

         // MessageHeaders Section
         if (deliveryAnnotationsMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nDeliveryAnnotationsMatcherFailed!");
            mismatchDescription.AppendText(deliveryAnnotationsMatcherFailureDescription);
            return;
         }

         // MessageAnnotations Section
         if (messageAnnotationsMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nMessageAnnotationsMatcherFailed!");
            mismatchDescription.AppendText(messageAnnotationsMatcherFailureDescription);
            return;
         }

         // Properties Section
         if (propertiesMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nPropertiesMatcherFailed!");
            mismatchDescription.AppendText(propertiesMatcherFailureDescription);
            return;
         }

         // Application Properties Section
         if (applicationPropertiesMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nApplicationPropertiesMatcherFailed!");
            mismatchDescription.AppendText(applicationPropertiesMatcherFailureDescription);
            return;
         }

         // Message Content Body Section
         if (msgContentMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nContentMatcherFailed!");
            mismatchDescription.AppendText(msgContentMatcherFailureDescription);
            return;
         }

         // Footer Section
         if (footerMatcherFailureDescription != null)
         {
            mismatchDescription.AppendText("\nContentMatcherFailed!");
            mismatchDescription.AppendText(footerMatcherFailureDescription);
         }
      }

      protected override bool MatchesSafely(byte[] payload)
      {
         MemoryStream stream = new MemoryStream(payload);
         long origLength = stream.Length;
         long bytesConsumed = 0;

         if (payloadLengthMatcher != null)
         {
            try
            {
               MatcherAssert.AssertThat("Payload length should match", origLength, payloadLengthMatcher);
            }
            catch (Exception t)
            {
               payloadLengthMatcherFailureDescription = "\nPayload Length Matcher generated throwable: " + t;

               return false;
            }
         }

         if (headersMatcher != null)
         {
            stream.Seek(bytesConsumed, SeekOrigin.Begin);

            try
            {
               bytesConsumed += headersMatcher.Verify(stream);
            }
            catch (Exception t)
            {
               headerMatcherFailureDescription += "\nMessageHeaderMatcher generated throwable: " + t;

               return false;
            }
         }

         if (deliveryAnnotationsMatcher != null)
         {
            stream.Seek(bytesConsumed, SeekOrigin.Begin);

            try
            {
               bytesConsumed += deliveryAnnotationsMatcher.Verify(stream);
            }
            catch (Exception t)
            {
               deliveryAnnotationsMatcherFailureDescription += "\nDeliveryAnnotationsMatcher generated throwable: " + t;

               return false;
            }
         }

         if (messageAnnotationsMatcher != null)
         {
            stream.Seek(bytesConsumed, SeekOrigin.Begin);

            try
            {
               bytesConsumed += messageAnnotationsMatcher.Verify(stream);
            }
            catch (Exception t)
            {
               messageAnnotationsMatcherFailureDescription += "\nMessageAnnotationsMatcher generated throwable: " + t;

               return false;
            }
         }

         if (propertiesMatcher != null)
         {
            stream.Seek(bytesConsumed, SeekOrigin.Begin);

            try
            {
               bytesConsumed += propertiesMatcher.Verify(stream);
            }
            catch (Exception t)
            {
               propertiesMatcherFailureDescription += "\nPropertiesMatcher generated throwable: " + t;

               return false;
            }
         }

         if (applicationPropertiesMatcher != null)
         {
            stream.Seek(bytesConsumed, SeekOrigin.Begin);

            try
            {
               bytesConsumed += applicationPropertiesMatcher.Verify(stream);
            }
            catch (Exception t)
            {
               applicationPropertiesMatcherFailureDescription += "\nApplicationPropertiesMatcher generated throwable: " + t;

               return false;
            }
         }

         if (msgContentMatchers.Count > 0)
         {
            foreach (IMatcher msgContentMatcher in msgContentMatchers)
            {
               stream.Seek(bytesConsumed, SeekOrigin.Begin);
               long originalReadableBytes = stream.Length - stream.Position;
               bool contentMatches = msgContentMatcher.Matches(stream);

               if (!contentMatches)
               {
                  IDescription desc = new StringDescription();
                  msgContentMatcher.DescribeTo(desc);
                  msgContentMatcher.DescribeMismatch(stream, desc);

                  msgContentMatcherFailureDescription = "\nMessageContentMatcher mismatch Description:";
                  msgContentMatcherFailureDescription += desc.ToString();

                  return false;
               }

               bytesConsumed += originalReadableBytes - (stream.Length - stream.Position);
            }
         }

         if (footersMatcher != null)
         {
            stream.Seek(bytesConsumed, SeekOrigin.Begin);
            try
            {
               bytesConsumed += footersMatcher.Verify(stream);
            }
            catch (Exception t)
            {
               footerMatcherFailureDescription += "\nFooterMatcher generated throwable: " + t;

               return false;
            }
         }

         return true;
      }
   }
}