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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Comparators
{
   /// <summary>
   /// Compares two KeyValuePair types and checks if both the entry keys
   /// and the entry values are equivalent.
   /// </summary>
   public sealed class KeyValuePairCompartor : ILinkedCompartor
   {
      private readonly PeerEqualityComparator comparators;

      public KeyValuePairCompartor(PeerEqualityComparator comparators)
      {
         this.comparators = comparators;
      }

      public bool? Equals(object lhs, object rhs, IDescription mismatchDescription)
      {
         Type lhsType = lhs.GetType();
         Type rhsType = rhs.GetType();

         Type lhsGenericTypeDefinition = lhsType.GetTypeInfo().IsGenericType ? lhsType.GetGenericTypeDefinition() : null;
         Type rhsGenericTypeDefinition = rhsType.GetTypeInfo().IsGenericType ? rhsType.GetGenericTypeDefinition() : null;

         if (lhsGenericTypeDefinition != typeof(KeyValuePair<,>) || rhsGenericTypeDefinition != typeof(KeyValuePair<,>))
         {
            return null;
         }

         object lhsKey = lhsType.GetProperty("Key").GetValue(lhs, null);
         object rhsKey = rhsType.GetProperty("Key").GetValue(rhs, null);
         object lhsValue = lhsType.GetProperty("Value").GetValue(lhs, null);
         object rhsValue = rhsType.GetProperty("Value").GetValue(rhs, null);

         bool keysEqual = comparators.AreEqual(lhsKey, rhsKey, mismatchDescription);
         bool valuesEqual = comparators.AreEqual(lhsValue, rhsValue, mismatchDescription);

         return keysEqual && valuesEqual;
      }
   }
}