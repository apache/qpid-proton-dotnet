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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// A builtin proton delivery tag generator that creates tag values from an ever increasing
   /// sequence id value.
   /// </summary>
   public class ProtonPooledTagGenerator : ProtonSequentialTagGenerator
   {
      public static readonly ushort DEFAULT_MAX_NUM_POOLED_TAGS = 512;

      private readonly ushort tagPoolSize;
      private readonly RingQueue<ProtonPooledDeliveryTag> tagPool;

      public ProtonPooledTagGenerator() : this(DEFAULT_MAX_NUM_POOLED_TAGS)
      {
      }

      public ProtonPooledTagGenerator(ushort poolSize) : base()
      {
         if (poolSize == 0)
         {
            throw new ArgumentOutOfRangeException("The tag pool size cannot be zero");
         }

         this.tagPoolSize = poolSize;
         this.tagPool = new RingQueue<ProtonPooledDeliveryTag>(tagPoolSize);
      }

      public override IDeliveryTag NextTag()
      {
         ProtonPooledDeliveryTag nextTag = tagPool.Poll();
         if (nextTag != null)
         {
            return nextTag.CheckOut();
         }
         else
         {
            return CreateTag();
         }
      }

      private IDeliveryTag CreateTag()
      {
         IDeliveryTag nextTag = null;

         if (nextTagId >= 0 && nextTagId < tagPoolSize)
         {
            // Pooled tag that will return to pool on next release.
            nextTag = new ProtonPooledDeliveryTag(tagPool, nextTagId++).CheckOut();
         }
         else
         {
            // Non-pooled tag that will not return to the pool on next release.
            nextTag = base.NextTag();
            if (nextTagId == 0)
            {
               nextTagId = tagPoolSize;
            }
         }

         return nextTag;
      }
   }

   internal sealed class ProtonPooledDeliveryTag : ProtonNumericDeliveryTag
   {
      private readonly RingQueue<ProtonPooledDeliveryTag> tagPool;

      private bool checkedOut;

      public ProtonPooledDeliveryTag(RingQueue<ProtonPooledDeliveryTag> pool, ulong tagValue) : base(tagValue)
      {
         this.tagPool = pool;
      }

      public ProtonPooledDeliveryTag CheckOut()
      {
         this.checkedOut = true;
         return this;
      }

      public override void Release()
      {
         if (checkedOut)
         {
            tagPool.Offer(this);
            checkedOut = false;
         }
      }
   }
}