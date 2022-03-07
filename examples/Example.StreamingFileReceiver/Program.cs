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
         if (args.Length == 0)
         {
            Console.WriteLine("Example requires a valid directory where the incoming file should be written");
            Environment.Exit(1);
         }

         Console.WriteLine("User specified output directory: {0}", args[0]);
         Console.WriteLine("Is param an existing file: {0}", Directory.Exists(args[0]));

         if (!Directory.Exists(args[0]))
         {
            Console.WriteLine("Example requires a valid / writable directory to transfer to");
            Environment.Exit(1);
         }

         const string fileNameKey = "filename";
         string serverHost = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
         int serverPort = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5672");
         string address = Environment.GetEnvironmentVariable("ADDRESS") ?? "large-message-example";

         IClient client = IClient.Create();

         ConnectionOptions options = new ConnectionOptions();
         options.User = Environment.GetEnvironmentVariable("USER");
         options.Password = Environment.GetEnvironmentVariable("PASSWORD");

         using IConnection connection = client.Connect(serverHost, serverPort, options);
         using IStreamReceiver receiver = connection.OpenStreamReceiver(address);

         IStreamDelivery delivery = receiver.Receive();
         IStreamReceiverMessage message = delivery.Message();
         Stream inputStream = message.Body;

         // The remote should have told us the filename of the original file it sent.
         string filename = (string)message.GetProperty(fileNameKey);
         if (string.IsNullOrEmpty(filename))
         {
            Console.WriteLine("Remote did not include the source filename in the incoming message");
            Environment.Exit(1);
         }
         else
         {
            Console.WriteLine("Starting receive of incoming file named: " + filename);
         }

         string outputLocation = Path.Combine(args[0], filename);

         using FileStream outputStream = File.Create(outputLocation);

         message.Body.CopyTo(outputStream);

         Console.WriteLine("Received file written to: " + outputLocation);
      }
   }
}
