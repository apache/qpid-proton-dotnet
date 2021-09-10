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
   public sealed class SourceMatcher : ListDescribedTypeMatcher
   {
      public SourceMatcher() : base(Enum.GetNames(typeof(SourceField)).Length, Source.DESCRIPTOR_CODE, Source.DESCRIPTOR_SYMBOL)
      {
      }

      public SourceMatcher(Source source) : this()
      {
         AddSourceMatchers(source);
      }

      protected override Type DescribedTypeClassType => typeof(Source);

      public SourceMatcher WithAddress(string name)
      {
         return WithAddress(Is.EqualTo(name));
      }

      public SourceMatcher WithDurable(TerminusDurability durability)
      {
         return WithDurable(Is.EqualTo(durability));
      }

      public SourceMatcher WithExpiryPolicy(TerminusExpiryPolicy expiry)
      {
         return WithExpiryPolicy(Is.EqualTo(expiry));
      }

      public SourceMatcher WithTimeout(uint timeout)
      {
         return WithTimeout(Is.EqualTo(timeout));
      }

      public SourceMatcher WithDefaultTimeout()
      {
         return WithTimeout(Apache.Qpid.Proton.Test.Driver.Matchers.Matches.AnyOf(Is.NullValue(), Is.EqualTo(0u)));
      }

      public SourceMatcher WithDynamic(bool dynamic)
      {
         return WithDynamic(Is.EqualTo(dynamic));
      }

      public SourceMatcher WithDynamicNodeProperties(IDictionary<Symbol, object> properties)
      {
         return WithDynamicNodeProperties(Is.EqualTo(properties));
      }

      public SourceMatcher WithDynamicNodeProperties(IDictionary<string, object> properties)
      {
         return WithDynamicNodeProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      public SourceMatcher WithDistributionMode(string distributionMode)
      {
         return WithDistributionMode(Is.EqualTo(new Symbol(distributionMode)));
      }

      public SourceMatcher WithDistributionMode(Symbol distributionMode)
      {
         return WithDistributionMode(Is.EqualTo(distributionMode));
      }

      public SourceMatcher WithFilter(IDictionary<string, object> filter)
      {
         return WithFilter(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(filter)));
      }

      public SourceMatcher WithFilter(IDictionary<Symbol, object> filter)
      {
         return WithFilter(Is.EqualTo(filter));
      }

      public SourceMatcher WithDefaultOutcome(IDeliveryState defaultOutcome)
      {
         return WithDefaultOutcome(Is.EqualTo(defaultOutcome));
      }

      public SourceMatcher WithOutcomes(params string[] outcomes)
      {
         return WithOutcomes(Is.EqualTo(TypeMapper.ToSymbolArray(outcomes)));
      }

      public SourceMatcher WithOutcomes(params Symbol[] outcomes)
      {
         return WithOutcomes(Is.EqualTo(outcomes));
      }

      public SourceMatcher WithCapabilities(params string[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(capabilities)));
      }

      public SourceMatcher WithCapabilities(params Symbol[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(capabilities));
      }

      #region Matcher based expectations

      public SourceMatcher WithAddress(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Address, m);
         return this;
      }

      public SourceMatcher WithDurable(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Durable, m);
         return this;
      }

      public SourceMatcher WithExpiryPolicy(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.ExpiryPolicy, m);
         return this;
      }

      public SourceMatcher WithTimeout(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Timeout, m);
         return this;
      }

      public SourceMatcher WithDynamic(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Dynamic, m);
         return this;
      }

      public SourceMatcher WithDynamicNodeProperties(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.DynamicNodeProperties, m);
         return this;
      }

      public SourceMatcher WithDistributionMode(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.DistributionMode, m);
         return this;
      }

      public SourceMatcher WithFilter(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Filter, m);
         return this;
      }

      public SourceMatcher WithDefaultOutcome(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.DefaultOutcome, m);
         return this;
      }

      public SourceMatcher WithOutcomes(IMatcher m)
      {
         AddFieldMatcher((int)SourceField.Outcomes, m);
         return this;
      }

      public SourceMatcher WithCapabilities(IMatcher m)
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