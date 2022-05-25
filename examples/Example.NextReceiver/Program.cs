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
using System.Threading;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client;

namespace Apache.Qpid.Proton.Examples.NextReceiver
{
   public class Program
   {
      public static void Main(string[] args)
      {
         string serverHost = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
         int serverPort = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5672");
         string address1 = Environment.GetEnvironmentVariable("ADDRESS1") ?? "next-receiver-1-address";
         string address2 = Environment.GetEnvironmentVariable("ADDRESS2") ?? "next-receiver-1-address";

         IClient client = IClient.Create();

         ConnectionOptions options = new()
         {
            User = Environment.GetEnvironmentVariable("USER"),
            Password = Environment.GetEnvironmentVariable("PASSWORD")
         };

         using IConnection connection = client.Connect(serverHost, serverPort, options);

         _ = connection.OpenReceiver(address1);
         _ = connection.OpenReceiver(address2);

         Task.Run(() =>
         {
            try
            {
               Thread.Sleep(2000);
               IMessage<string> message1 = IMessage<string>.Create("Hello World 1");
               message1.To = address1;
               connection.Send(message1);
               Thread.Sleep(2000);
               IMessage<string> message2 = IMessage<string>.Create("Hello World 2");
               message2.To = address2;
               connection.Send(message2);
            }
            catch (Exception e)
            {
               Console.WriteLine("Exception in message send task: " + e.Message);
            }
         });

         IDelivery delivery1 = connection.NextReceiver().Receive();
         IDelivery delivery2 = connection.NextReceiver().Receive();

         Console.WriteLine("Received first message with body: " + delivery1.Message().Body);
         Console.WriteLine("Received second message with body: " + delivery2.Message().Body);
      }
   }
}
