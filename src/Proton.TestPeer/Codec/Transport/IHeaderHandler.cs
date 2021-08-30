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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   /// <summary>
   /// Handler Interface that can be used to implement a visitor pattern of
   /// processing the AMQP Header types as they are sent or received.
   /// </summary>
   /// <typeparam name="T">The type of the context used in the processing</typeparam>
   public interface IHeaderHandler<T>
   {
        void HandleAMQPHeader(AMQPHeader header, T context) {}

        void HandleSASLHeader(AMQPHeader header, T context) {}

   }
}