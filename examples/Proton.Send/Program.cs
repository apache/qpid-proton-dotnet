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
using Apache.Qpid.Proton.Client;

namespace Apache.Qpid.Proton.Examples.HelloWorld
{
   class Program
   {
      private static readonly int MessageCount = 100;

      static void Main(string[] args)
      {
         string serverHost = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
         int serverPort = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5672");
         string address = Environment.GetEnvironmentVariable("ADDRESS") ?? "hello-world-example";

         IClient client = IClient.Create();

         ConnectionOptions options = new ConnectionOptions();
         options.User = Environment.GetEnvironmentVariable("USER");
         options.Password = Environment.GetEnvironmentVariable("PASSWORD");

         using IConnection connection = client.Connect(serverHost, serverPort, options);
         using ISender sender = connection.OpenSender(address);

         for (int i = 0; i < MessageCount; ++i)
         {
            IMessage<string> message = IMessage<string>.Create(string.Format("Hello World! [%s]", i));
            ITracker tracker = sender.Send(message);
            tracker.AwaitSettlement();

            Console.WriteLine(string.Format("Sent message to %s: %s", sender.Address, message.Body));
         }
      }
   }
}
