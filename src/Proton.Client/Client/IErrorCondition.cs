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

using System.Collections.Generic;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// An immutable error condition that carries information that aids in
   /// determining the factors that lead to the error.
   /// </summary>
   public interface IErrorCondition
   {
      /// <summary>
      /// Indicates the type of error that this condition conveys
      /// </summary>
      string Condition { get; }

      /// <summary>
      /// A description of the condition that resulted in this error.
      /// </summary>
      string Description { get; }

      /// <summary>
      /// Optional supplementary information that aids in processing this error.
      /// </summary>
      IDictionary<string, object> Info { get; }

   }
}