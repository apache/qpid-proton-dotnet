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

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   /// <summary>
   /// Base class for custom description implementations which provides some of
   /// the more universal implementation of description accumulation APIs.
   /// </summary>
   public sealed class StringDescription : BaseDescription
   {
      private readonly StringBuilder builder;

      /// <summary>
      /// Creates a new StringDescription that uses its own StringBuilder instance.
      /// </summary>
      public StringDescription() : this(new StringBuilder())
      {
      }

      /// <summary>
      /// Creates a new StringDescription that uses the provided StringBuilder instance.
      /// </summary>
      public StringDescription(StringBuilder builder)
      {
         this.builder = builder;
      }

      protected override IDescription Append(string text)
      {
         builder.Append(text);
         return this;
      }

      public override string ToString()
      {
         return builder.ToString();
      }
   }
}