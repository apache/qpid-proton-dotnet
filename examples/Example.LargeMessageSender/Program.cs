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
using System.IO;
using Apache.Qpid.Proton.Client;

namespace Apache.Qpid.Proton.Examples.HelloWorld
{
   class Program
   {
      static void Main(string[] args)
      {
         string serverHost = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
         int serverPort = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5672");
         string address = Environment.GetEnvironmentVariable("ADDRESS") ?? "large-message-example";

         IClient client = IClient.Create();

         ConnectionOptions options = new ConnectionOptions();
         options.User = Environment.GetEnvironmentVariable("USER");
         options.Password = Environment.GetEnvironmentVariable("PASSWORD");

         using IConnection connection = client.Connect(serverHost, serverPort, options);
         using IStreamSender sender = connection.OpenStreamSender(address);

         IStreamSenderMessage message = sender.BeginMessage();
         message.Durable = true;

         byte[] buffer = new byte[100];
         Array.Fill(buffer, (byte)'A');

         // Creates an OutputStream that writes a single Data Section whose expected
         // size is configured in the stream options.
         OutputStreamOptions streamOptions = new OutputStreamOptions();
         streamOptions.BodyLength = buffer.Length;
         Stream output = message.GetBodyStream(streamOptions);

         const int chunkSize = 10;

         for (int i = 0; i < buffer.Length; i += chunkSize)
         {
            output.Write(buffer, i, chunkSize);
         }

         output.Close();  // This completes the message send.

         message.Tracker.AwaitAccepted();
      }
   }
}
