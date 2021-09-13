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
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public class SourceMatcher : ListDescribedTypeMatcher
   {
      public SourceMatcher() : base(Enum.GetNames(typeof(SourceField)).Length, Source.DESCRIPTOR_CODE, Source.DESCRIPTOR_SYMBOL)
      {
      }

      public SourceMatcher(Source source) : this()
      {
         AddSourceMatchers(source);
      }

      protected override Type DescribedTypeClassType => typeof(Source);

      public virtual SourceMatcher WithAddress(string name)
      {
         return WithAddress(Is.EqualTo(name));
      }

      public virtual SourceMatcher WithDurable(TerminusDurability durability)
      {
         return WithDurable(Is.EqualTo(durability));
      }

      public virtual SourceMatcher WithExpiryPolicy(TerminusExpiryPolicy expiry)
      {
         return WithExpiryPolicy(Is.EqualTo(expiry));
      }

      public virtual SourceMatcher WithTimeout(uint timeout)
      {
         return WithTimeout(Is.EqualTo(timeout));
      }

      public virtual SourceMatcher WithDefaultTimeout()
      {
         return WithTimeout(Apache.Qpid.Proton.Test.Driver.Matchers.Matches.AnyOf(Is.NullValue(), Is.EqualTo(0u)));
      }

      public virtual SourceMatcher WithDynamic(bool dynamic)
      {
         return WithDynamic(Is.EqualTo(dynamic));
      }

      public virtual SourceMatcher WithDynamicNodeProperties(IDictionary<Symbol, object> properties)
      {
         return WithDynamicNodeProperties(Is.EqualTo(properties));
      }

      public virtual SourceMatcher WithDynamicNodeProperties(IDictionary<string, object> properties)
      {
         return WithDynamicNodeProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      public virtual SourceMatcher WithDistributionMode(string distributionMode)
      {
         return WithDistributionMode(Is.EqualTo(new Symbol(distributionMode)));
      }

      public virtual SourceMatcher WithDistributionMode(Symbol distributionMode)
      {
         return WithDistributionMode(Is.EqualTo(distributionMode));
      }

      public virtual SourceMatcher WithFilter(IDictionary<string, object> filter)
      {
         return WithFilter(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(filter)));
      }

      public virtual SourceMatcher WithFilter(IDictionary<Symbol, object> filter)
      {
         return WithFilter(Is.EqualTo(filter));
      }

      public virtual SourceMatcher WithDefaultOutcome(IDeliveryState defaultOutcome)
      {
         return WithDefaultOutcome(Is.EqualTo(defaultOutcome));
      }

      public virtual SourceMatcher WithOutcomes(params string[] outcomes)
      {
         return WithOutcomes(Is.EqualTo(TypeMapper.ToSymbolArray(outcomes)));
      }

      public virtual SourceMatcher WithOutcomes(params Symbol[] outcomes)
      {
         return WithOutcomes(Is.EqualTo(outcomes));
      }

      public virtual SourceMatcher WithCapabilities(params string[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(capabilities)));
      }

      public virtual SourceMatcher WithCapabilities(params Symbol[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(capabilities));
      }

      #region Matcher based expectations

      public virtual SourceMatcher WithAddress(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Address, m);
         return this;
      }

      public virtual SourceMatcher WithDurable(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Durable, m);
         return this;
      }

      public virtual SourceMatcher WithExpiryPolicy(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.ExpiryPolicy, m);
         return this;
      }

      public virtual SourceMatcher WithTimeout(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Timeout, m);
         return this;
      }

      public virtual SourceMatcher WithDynamic(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Dynamic, m);
         return this;
      }

      public virtual SourceMatcher WithDynamicNodeProperties(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.DynamicNodeProperties, m);
         return this;
      }

      public virtual SourceMatcher WithDistributionMode(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.DistributionMode, m);
         return this;
      }

      public virtual SourceMatcher WithFilter(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Filter, m);
         return this;
      }

      public virtual SourceMatcher WithDefaultOutcome(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.DefaultOutcome, m);
         return this;
      }

      public virtual SourceMatcher WithOutcomes(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Outcomes, m);
         return this;
      }

      public virtual SourceMatcher WithCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Capabilities, m);
         return this;
      }

      #endregion

      private void AddSourceMatchers(Source source)
      {
         if (source.Address != null)
         {
            AddFieldMatcher((int)SourceField.Address, Is.EqualTo(source.Address));
         }
         else
         {
            AddFieldMatcher((int)SourceField.Address, Is.NullValue());
         }

         if (source.Durable != null)
         {
            AddFieldMatcher((int)SourceField.Durable, Is.EqualTo(source.Durable));
         }
         else
         {
            AddFieldMatcher((int)SourceField.Durable, Is.NullValue());
         }

         if (source.ExpiryPolicy != null)
         {
            AddFieldMatcher((int)SourceField.ExpiryPolicy, Is.EqualTo(source.ExpiryPolicy));
         }
         else
         {
            AddFieldMatcher((int)SourceField.ExpiryPolicy, Is.NullValue());
         }

         if (source.Timeout != null)
         {
            AddFieldMatcher((int)SourceField.Timeout, Is.EqualTo(source.Timeout));
         }
         else
         {
            AddFieldMatcher((int)SourceField.Timeout, Is.NullValue());
         }

         AddFieldMatcher((int)SourceField.Dynamic, Is.EqualTo(source.Dynamic));

         if (source.DynamicNodeProperties != null)
         {
            AddFieldMatcher((int)SourceField.DynamicNodeProperties, Is.EqualTo(source.DynamicNodeProperties));
         }
         else
         {
            AddFieldMatcher((int)SourceField.DynamicNodeProperties, Is.NullValue());
         }

         if (source.DistributionMode != null)
         {
            AddFieldMatcher((int)SourceField.DistributionMode, Is.EqualTo(source.DistributionMode));
         }
         else
         {
            AddFieldMatcher((int)SourceField.DistributionMode, Is.NullValue());
         }

         if (source.Filter != null)
         {
            AddFieldMatcher((int)SourceField.Filter, Is.EqualTo(source.Filter));
         }
         else
         {
            AddFieldMatcher((int)SourceField.Filter, Is.NullValue());
         }

         if (source.DefaultOutcome != null)
         {
            AddFieldMatcher((int)SourceField.DefaultOutcome, Is.EqualTo((IDeliveryState)source.DefaultOutcome));
         }
         else
         {
            AddFieldMatcher((int)SourceField.DefaultOutcome, Is.NullValue());
         }

         if (source.Outcomes != null)
         {
            AddFieldMatcher((int)SourceField.Outcomes, Is.EqualTo(source.Outcomes));
         }
         else
         {
            AddFieldMatcher((int)SourceField.Outcomes, Is.NullValue());
         }

         if (source.Capabilities != null)
         {
            AddFieldMatcher((int)SourceField.Capabilities, Is.EqualTo(source.Capabilities));
         }
         else
         {
            AddFieldMatcher((int)SourceField.Capabilities, Is.NullValue());
         }
      }
   }
}