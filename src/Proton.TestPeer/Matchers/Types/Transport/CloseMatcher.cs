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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class CloseMatcher : ListDescribedTypeMatcher
   {
      public CloseMatcher() : base(Enum.GetNames(typeof(CloseField)).Length, Close.DESCRIPTOR_CODE, Close.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Close);

      public CloseMatcher WithError(ErrorCondition error)
      {
         return WithError(Is.EqualTo(error));
      }

      public CloseMatcher WithError(String condition, String description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description)));
      }

      public CloseMatcher WithError(String condition, String description, IDictionary<string, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description, TypeMapper.ToSymbolKeyedMap(info))));
      }

      public CloseMatcher WithError(Symbol condition, String description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description)));
      }

      public CloseMatcher WithError(Symbol condition, String description, IDictionary<Symbol, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description, info)));
      }

      #region With API for field matching

      public CloseMatcher WithError(IMatcher m)
      {
         AddFieldMatcher((int)EndField.Error, m);
         return this;
      }

      #endregion
   }
}