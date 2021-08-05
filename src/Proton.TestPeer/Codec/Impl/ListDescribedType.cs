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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec
{
   public abstract class ListDescribedType : IDescribedType
   {
      private readonly ArrayList fields;

      public ListDescribedType(int numberOfFields)
      {
         fields = new ArrayList(numberOfFields);

         for (int i = 0; i < numberOfFields; ++i)
         {
            fields.Add(null);
         }
      }

      public ListDescribedType(int numberOfFields, ListDescribedType described)
      {
         if (described.fields.Count > numberOfFields)
         {
            throw new ArgumentOutOfRangeException("List encoded exceeds expected number of elements for this type");
         }

         fields = new ArrayList(numberOfFields);

         for (int i = 0; i < numberOfFields; ++i)
         {
            if (i < described.fields.Count)
            {
               fields.Add(described.fields[i]);
            }
            else
            {
               fields.Add(null);
            }
         }
      }

      public ListDescribedType(int numberOfFields, IList described)
      {
         if (described.Count > numberOfFields)
         {
            throw new ArgumentOutOfRangeException("List encoded exceeds expected number of elements for this type");
         }

         fields = new ArrayList(numberOfFields);

         for (int i = 0; i < numberOfFields; ++i)
         {
            if (i < described.Count)
            {
               fields.Add(described[i]);
            }
            else
            {
               fields.Add(null);
            }
         }
      }

      /// <summary>
      /// Derived class must provide the descriptor value that defines this type
      /// </summary>
      public abstract object Descriptor { get; }

      public object Described
      {
         get
         {
            // Return a List containing only the 'used fields' (i.e up to the
            // highest field used)
            int highestSetField = HighestSetFieldId;

            // Create a list with the fields in the correct positions.
            IList list = new ArrayList();
            for (int j = 0; j <= highestSetField; j++)
            {
               list.Add(fields[j]);
            }

            return list;
         }
      }

      public object this[int index]
      {
         get
         {
            if (index < fields.Count)
            {
               return fields[index];
            }
            else
            {
               throw new ArgumentOutOfRangeException("Request for unknown field in type: " + this);
            }
         }
      }

      protected int HighestSetFieldId
      {
         get
         {
            int numUsedFields = 0;
            foreach (object element in fields)
            {
               if (element != null)
               {
                  numUsedFields++;
               }
            }

            return numUsedFields;
         }
      }

      protected IList GetList()
      {
         return fields;
      }

      protected Object[] GetFields()
      {
         return fields.ToArray();
      }

      public override string ToString()
      {
         return GetType().Name + " [descriptor=" + Descriptor + " fields=" + GetList() + "]";
      }
   }
}