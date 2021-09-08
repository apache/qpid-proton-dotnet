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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Close into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public sealed class CloseInjectAction : AbstractPerformativeInjectAction<Close>
   {
      private readonly Close close = new Close();

      public CloseInjectAction(AMQPTestDriver driver) : base(driver)
      {
      }

      public override Close Performative => close;

      public CloseInjectAction WithErrorCondition(ErrorCondition error)
      {
         close.Error = error;
         return this;
      }

      public CloseInjectAction WithErrorCondition(String condition, String description)
      {
         close.Error = new ErrorCondition(new Symbol(condition), description);
         return this;
      }

      public CloseInjectAction WithErrorCondition(Symbol condition, String description)
      {
         close.Error = new ErrorCondition(condition, description);
         return this;
      }

      public CloseInjectAction WithErrorCondition(String condition, String description, IDictionary<string, object> info)
      {
         close.Error = new ErrorCondition(new Symbol(condition), description, TypeMapper.ToSymbolKeyedMap(info));
         return this;
      }

      public CloseInjectAction WithErrorCondition(Symbol condition, String description, IDictionary<Symbol, object> info)
      {
         close.Error = new ErrorCondition(condition, description, info);
         return this;
      }

      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         // We fill in a channel using the next available channel id if one isn't set, then
         // report the outbound begin to the session so it can track this new session.
         if (channel == null)
         {
            channel = 0;
         }
      }
   }
}