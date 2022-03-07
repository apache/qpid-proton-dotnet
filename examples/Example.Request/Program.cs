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
      static void Main(string[] args)
      {
         string serverHost = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
         int serverPort = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5672");
         string address = Environment.GetEnvironmentVariable("ADDRESS") ?? "request-respond-example";

         IClient client = IClient.Create();

         ConnectionOptions options = new ConnectionOptions();
         options.User = Environment.GetEnvironmentVariable("USER");
         options.Password = Environment.GetEnvironmentVariable("PASSWORD");

         SenderOptions senderOptions = new SenderOptions();
         senderOptions.TargetOptions.Capabilities = new string[] { "queue" };

         using IConnection connection = client.Connect(serverHost, serverPort, options);
         using IReceiver dynamicReceiver = connection.OpenDynamicReceiver();
         using ISender requestor = connection.OpenSender(address, senderOptions);

         string dynamicAddress = dynamicReceiver.Address;
         Console.WriteLine("Waiting for response to requests on address: " + dynamicAddress);

         IMessage<string> request = IMessage<string>.Create("Hello World");
         request.ReplyTo = dynamicAddress;

         requestor.Send(request);

         IDelivery response = dynamicReceiver.Receive(TimeSpan.FromSeconds(10));
         IMessage<object> received = response.Message();

         Console.WriteLine("Received message with body: " + received.Body);
      }
   }
}
