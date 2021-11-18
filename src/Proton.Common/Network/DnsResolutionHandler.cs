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

using System.Threading.Tasks;
using System.Net;

namespace Apache.Qpid.Proton.Client.Network
{
   /// <summary>
   /// Uses the .NET default DNS API to resolve the given end points into
   /// IP type end points that can be used in client or server operations.
   /// </summary>
   public class DnsResolutionHandler
   {
      /// <summary>
      /// Check if the given end point is already a resolved instance or not
      /// </summary>
      /// <param name="endpoint">The endpoint to check</param>
      /// <returns>true if the endpoint given is already resolved</returns>
      public bool IsResolved(EndPoint endpoint) => (endpoint is not DnsEndPoint);

      /// <summary>
      /// Given an end point instance, asynchronously resolve that endpoint to an
      /// IP address endpoint value for use in connection or server bind operations.
      /// </summary>
      /// <param name="endpoint">The endpoint to resolve</param>
      /// <returns>A Task that will eventually resolved the given endpoint to an IPendPoint </returns>
      public async Task<EndPoint> ResolveAsync(EndPoint endpoint)
      {
         if (endpoint is DnsEndPoint address)
         {
            IPHostEntry resolved = await Dns.GetHostEntryAsync(address.Host);
            return new IPEndPoint(resolved.AddressList[0], address.Port);
         }
         else
         {
            return endpoint;
         }
      }
   }
}