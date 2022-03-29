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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A single AMQP link which can be a sender or receiver instance but
   /// each expose a common set of link properties whose API is presented
   /// here.
   /// </summary>
   public interface ILink : IDisposable
   {
      /// <summary>
      /// Returns the parent client instance that hosts this link.
      /// </summary>
      IClient Client { get; }

      /// <summary>
      /// Returns the parent connection instance that hosts this link.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// Returns the parent session instance that created this link.
      /// </summary>
      ISession Session { get; }

      /// <summary>
      /// Initiates a close of the link and awaits a response from the remote that
      /// indicates completion of the close operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// link resources.
      /// </summary>
      /// <param name="error">Optional error condition to convery to the remote</param>
      void Close(IErrorCondition error = null);

      /// <summary>
      /// Initiates a detach of the link and awaits a response from the remote that
      /// indicates completion of the detach operation. If the response from the remote
      /// exceeds the configure close timeout the method returns after cleaning up the
      /// link resources.
      /// </summary>
      /// <param name="error">Optional error condition to convery to the remote</param>
      void Detach(IErrorCondition error = null);

      /// <summary>
      /// Returns the address that the link instance will send message objects to. The value
      /// returned from this method is controlled by the configuration that was used to create
      /// the link.
      /// <list type="bullet">
      /// <item>
      /// <description>
      /// If the link is configured as an anonymous link then this method returns null.
      /// </description>
      /// </item>
      /// <item>
      /// <description>
      /// If the link was created with the dynamic link methods then the method will return
      /// the dynamically created address once the remote has attached its end of the link link.
      /// Due to the need to await the remote peer to populate the dynamic address this method will
      /// block until the open of the link link has completed.
      /// </description>
      /// </item>
      /// <item>
      /// <description>
      /// If neither of the above is true then the address returned is the address passed to the
      /// original address value passed to one of the open link methods.
      /// </description>
      /// </item>
      /// </list>
      /// </summary>
      string Address { get; }

      /// <summary>
      /// Returns an immutable view of the remote Source object assigned to this link link.
      /// If the attach has not completed yet this method will block to await the attach response
      /// which carries the remote source.
      /// </summary>
      ISource Source { get; }

      /// <summary>
      /// Returns an immutable view of the remote Target object assigned to this link link.
      /// If the attach has not completed yet this method will block to await the attach response
      /// which carries the remote target.
      /// </summary>
      ITarget Target { get; }

      /// <summary>
      /// Returns the properties that the remote provided upon successfully opening the link.
      /// If the open has not completed yet this method will block to await the open response which
      /// carries the remote properties.  If the remote provides no properties this method will
      /// return null.
      /// </summary>
      IReadOnlyDictionary<string, object> Properties { get; }

      /// <summary>
      /// Returns the offered capabilities that the remote provided upon successfully opening the
      /// link. If the open has not completed yet this method will block to await the open
      /// response which carries the remote offered capabilities. If the remote provides no offered
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> OfferedCapabilities { get; }

      /// <summary>
      /// Returns the desired capabilities that the remote provided upon successfully opening the
      /// link. If the open has not completed yet this method will block to await the open
      /// response which carries the remote desired capabilities. If the remote provides no desired
      /// capabilities this method will return null.
      /// </summary>
      IReadOnlyCollection<string> DesiredCapabilities { get; }

   }
}