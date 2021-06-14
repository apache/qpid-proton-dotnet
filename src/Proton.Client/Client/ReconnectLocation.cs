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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Represents a fixed location that can be used for connection by the client
   /// should the initially specified connection location be unreachable or become
   /// unavailable during the lifetime of the connection.
   /// </summary>
   public struct ReconnectLocation : IEquatable<ReconnectLocation>
   {
      public ReconnectLocation(string host, int port)
      {
         if (String.IsNullOrEmpty(host))
         {
            throw new ArgumentException("Remote host value cannot be null or empty");
         }

         this.Host = host;
         this.Port = port;
      }

      /// <summary>
      /// Returns the assigned remote host name or IP address as a string value.
      /// </summary>
      public string Host { get; }

      /// <summary>
      /// Returns the assigned remote port value (negative values mean use default AMQP ports).
      /// </summary>
      public int Port { get; }

      public override int GetHashCode()
      {
         return Host.GetHashCode() ^ Port.GetHashCode();
      }

      public bool Equals(ReconnectLocation other)
      {
         return other.Host.Equals(Host) && other.Port == Port;
      }

      public override bool Equals(object other)
      {
         if (other == null || other.GetType() != GetType())
         {
            return false;
         }

         return this.Equals((ReconnectLocation)other);
      }

      public static bool operator ==(ReconnectLocation x, ReconnectLocation y)
      {
         return x.Host == y.Host && x.Port == y.Port;
      }

      public static bool operator !=(ReconnectLocation x, ReconnectLocation y)
      {
         return !(x == y);
      }

      public override string ToString()
      {
         return "ReconnectLocation { " + Host + ", " + Port + " }";
      }
   }
}