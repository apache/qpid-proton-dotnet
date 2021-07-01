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
using System.IO;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public abstract class AbstractDescribedTypeDecoder : IDescribedTypeDecoder, IStreamDescribedTypeDecoder
   {
      public bool IsArrayType => false;

      #region Described Type Encoder API

      public abstract Symbol DescriptorSymbol { get; }

      public abstract ulong DescriptorCode { get; }

      public abstract Type DecodesType { get; }

      public abstract object ReadValue(IProtonBuffer buffer, IDecoderState state);

      public abstract Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count);

      public abstract void SkipValue(IProtonBuffer buffer, IDecoderState state);

      public abstract object ReadValue(Stream stream, IStreamDecoderState state);

      public abstract Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count);

      public abstract void SkipValue(Stream stream, IStreamDecoderState state);

      #endregion

      public override String ToString()
      {
         return "DescribedTypeDecoder<" + GetType().Name + ">";
      }

      protected static T CheckIsExpectedTypeAndCast<T>(ITypeDecoder actual)
      {
         if (!typeof(T).IsAssignableFrom(actual.GetType()))
         {
            throw new DecodeException(
                "Expected " + typeof(T) + "encoding but got decoder for type: " + actual.GetType().Name);
         }

         return (T) actual;
      }

      protected static T CheckIsExpectedTypeAndCast<T>(IStreamTypeDecoder actual)
      {
         if (!typeof(T).IsAssignableFrom(actual.GetType()))
         {
            throw new DecodeException(
                "Expected " + typeof(T) + "encoding but got decoder for type: " + actual.GetType().Name);
         }

         return (T) actual;
      }

      protected static void CheckIsExpectedType<T>(ITypeDecoder actual)
      {
         if (!typeof(T).IsAssignableFrom(actual.GetType()))
         {
            throw new DecodeException(
                "Expected " + typeof(T) + "encoding but got decoder for type: " + actual.GetType().Name);
         }
      }

      protected static void CheckIsExpectedType<T>(IStreamTypeDecoder actual)
      {
         if (!typeof(T).IsAssignableFrom(actual.GetType()))
         {
            throw new DecodeException(
                "Expected " + typeof(T) + "encoding but got decoder for type: " + actual.GetType().Name);
         }
      }
   }
}
