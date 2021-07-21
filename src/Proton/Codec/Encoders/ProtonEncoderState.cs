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

namespace Apache.Qpid.Proton.Codec.Encoders
{
   public class ProtonEncoderState : IEncoderState
   {
      private ProtonEncoder encoder;

      public ProtonEncoderState(ProtonEncoder encoder)
      {
         this.encoder = encoder;
      }

      public void Reset()
      {
         // Nothing needed yet.
      }

      public IEncoder Encoder
      {
         get { return this.encoder; }
      }

      public IUtf8Encoder Utf8Encoder { get; set; }

      public IProtonBuffer EncodeUtf8(IProtonBuffer buffer, string value)
      {
         if (Utf8Encoder == null)
         {
            EncodeUtf8Sequence(buffer, value);
         }
         else
         {
            Utf8Encoder.EncodeUTF8(buffer, value);
         }

         return buffer;
      }

      private void EncodeUtf8Sequence(IProtonBuffer buffer, string value)
      {
         UTF8Encoding utf8 = new UTF8Encoding();

         byte[] encoded = utf8.GetBytes(value);

         buffer.EnsureWritable(encoded.LongLength);
         buffer.WriteBytes(encoded);
      }
   }
}