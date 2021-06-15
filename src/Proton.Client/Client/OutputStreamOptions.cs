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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Options class that controls various aspects of a write only stream instance
   /// created to write the contents of a section of a stream sender message.
   /// </summary>
   public class OutputStreamOptions : ICloneable
   {
      public static readonly bool DEFAULT_COMPLETE_SEND_ON_CLOSE = true;

      /// <summary>
      /// Creates a default output stream options instance.
      /// </summary>
      public OutputStreamOptions() : base()
      {
      }

      /// <summary>
      /// Create a new output stream options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The sender options instance to copy</param>
      public OutputStreamOptions(OutputStreamOptions other) : base()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public virtual object Clone()
      {
         return CopyInto(new OutputStreamOptions());
      }

      internal OutputStreamOptions CopyInto(OutputStreamOptions other)
      {
         other.CompleteSendOnClose = CompleteSendOnClose;
         other.BodyLength = BodyLength;

         return other;
      }

      /// <summary>
      /// Configures if the close of the {@link OutputStream} should result in a completion
      /// of the parent stream sender message (default is true). If there is a configured
      /// stream size and the stream is closed the parent stream sender message will always
      /// be aborted as the send would be incomplete, but the close of an Stream may not
      /// always be the desired outcome.  In the case the user wishes to add a footer to the
      /// message transmitted by the stream sender message this option should be set to false
      /// and the user should complete the stream manually.
      /// </summary>
      public bool CompleteSendOnClose { get; set; } = DEFAULT_COMPLETE_SEND_ON_CLOSE;

      /// <summary>
      /// Sets the overall stream size for this associated {@link OutputStream} that the
      /// options are applied to.
      /// </summary>
      /// <remarks>
      /// When set this option indicates the number of bytes that can be written to the stream
      /// before an error would be thrown indicating that this value was exceeded.  Conversely
      /// if the stream is closed before the number of bytes indicated is written the send will
      /// be aborted and an error will be thrown to the caller.
      /// </remarks>
      public int BodyLength { get; set; }

   }
}