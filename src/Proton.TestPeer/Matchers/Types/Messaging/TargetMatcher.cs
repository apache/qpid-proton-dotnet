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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public class TargetMatcher : ListDescribedTypeMatcher
   {
      public TargetMatcher() : base(Enum.GetNames(typeof(TargetField)).Length, Target.DESCRIPTOR_CODE, Target.DESCRIPTOR_SYMBOL)
      {
      }

      public TargetMatcher(Target target) : this()
      {
         AddTargetMatchers(target);
      }

      protected override Type DescribedTypeClassType => typeof(Target);

      public virtual  TargetMatcher WithAddress(string name)
      {
         return WithAddress(Is.EqualTo(name));
      }

      public virtual TargetMatcher WithDurable(TerminusDurability durability)
      {
         return WithDurable(Is.EqualTo((uint)durability));
      }

      public virtual TargetMatcher WithExpiryPolicy(TerminusExpiryPolicy expiry)
      {
         return WithExpiryPolicy(Is.EqualTo(expiry.ToSymbol()));
      }

      public virtual TargetMatcher WithTimeout(uint timeout)
      {
         return WithTimeout(Is.EqualTo(timeout));
      }

      public virtual TargetMatcher WithDefaultTimeout()
      {
         return WithTimeout(Apache.Qpid.Proton.Test.Driver.Matchers.Matches.AnyOf(Is.NullValue(), Is.EqualTo(0u)));
      }

      public virtual TargetMatcher WithDynamic(bool dynamic)
      {
         return WithDynamic(Is.EqualTo(dynamic));
      }

      public virtual TargetMatcher WithDynamicNodeProperties(IDictionary<Symbol, object> properties)
      {
         return WithDynamicNodeProperties(Is.EqualTo(properties));
      }

      public virtual TargetMatcher WithDynamicNodeProperties(IDictionary<string, object> properties)
      {
         return WithDynamicNodeProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      public virtual TargetMatcher WithCapabilities(params string[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(capabilities)));
      }

      public virtual TargetMatcher WithCapabilities(params Symbol[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(capabilities));
      }

      #region Matcher based expectations

      public virtual TargetMatcher WithAddress(IMatcher m)
      {
         AddFieldMatcher((int)TargetField.Address, m);
         return this;
      }

      public virtual TargetMatcher WithDurable(IMatcher m)
      {
         AddFieldMatcher((int)TargetField.Durable, m);
         return this;
      }

      public virtual TargetMatcher WithExpiryPolicy(IMatcher m)
      {
         AddFieldMatcher((int)TargetField.ExpiryPolicy, m);
         return this;
      }

      public virtual TargetMatcher WithTimeout(IMatcher m)
      {
         AddFieldMatcher((int)TargetField.Timeout, m);
         return this;
      }

      public virtual TargetMatcher WithDynamic(IMatcher m)
      {
         AddFieldMatcher((int)TargetField.Dynamic, m);
         return this;
      }

      public virtual TargetMatcher WithDynamicNodeProperties(IMatcher m)
      {
         AddFieldMatcher((int)TargetField.DynamicNodeProperties, m);
         return this;
      }

      public virtual TargetMatcher WithCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)TargetField.Capabilities, m);
         return this;
      }

      #endregion

      private void AddTargetMatchers(Target target)
      {
         if (target.Address != null)
         {
            AddFieldMatcher((int)TargetField.Address, Is.EqualTo(target.Address));
         }
         else
         {
            AddFieldMatcher((int)TargetField.Address, Is.NullValue());
         }

         if (target.Durable != null)
         {
            AddFieldMatcher((int)TargetField.Durable, Is.EqualTo(target.Durable));
         }
         else
         {
            AddFieldMatcher((int)TargetField.Durable, Is.NullValue());
         }

         if (target.ExpiryPolicy != null)
         {
            AddFieldMatcher((int)TargetField.ExpiryPolicy, Is.EqualTo(target.ExpiryPolicy));
         }
         else
         {
            AddFieldMatcher((int)TargetField.ExpiryPolicy, Is.NullValue());
         }

         if (target.Timeout != null)
         {
            AddFieldMatcher((int)TargetField.Timeout, Is.EqualTo(target.Timeout));
         }
         else
         {
            AddFieldMatcher((int)TargetField.Timeout, Is.NullValue());
         }

         AddFieldMatcher((int)TargetField.Dynamic, Is.EqualTo(target.Dynamic));

         if (target.DynamicNodeProperties != null)
         {
            AddFieldMatcher((int)TargetField.DynamicNodeProperties, Is.EqualTo(target.DynamicNodeProperties));
         }
         else
         {
            AddFieldMatcher((int)TargetField.DynamicNodeProperties, Is.NullValue());
         }

         if (target.Capabilities != null)
         {
            AddFieldMatcher((int)TargetField.Capabilities, Is.EqualTo(target.Capabilities));
         }
         else
         {
            AddFieldMatcher((int)TargetField.Capabilities, Is.NullValue());
         }
      }
   }
}