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
            Console.WriteLine("Example requires a valid file name to transfer");
            Environment.Exit(1);
         }

         if (!File.Exists(args[0]))
         {
            Console.WriteLine("Example requires a valid / readable file to transfer");
            Environment.Exit(1);
         }

         const string fileNameKey = "filename";
         string fileName = args[0];
         string serverHost = Environment.GetEnvironmentVariable("HOST") ?? "localhost";
         int serverPort = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? "5672");
         string address = Environment.GetEnvironmentVariable("ADDRESS") ?? "large-message-example";

         IClient client = IClient.Create();

         ConnectionOptions options = new ConnectionOptions();
         options.User = Environment.GetEnvironmentVariable("USER");
         options.Password = Environment.GetEnvironmentVariable("PASSWORD");

         using IConnection connection = client.Connect(serverHost, serverPort, options);
         using IStreamSender sender = connection.OpenStreamSender(address);
         using FileStream inputStream = File.OpenRead(fileName);

         IStreamSenderMessage message = sender.BeginMessage();
         message.SetProperty(fileNameKey, fileName);

         // Creates an OutputStream that writes the file in smaller data sections which allows for
         // larger file sizes than the single AMQP Data section bounded configuration might allow.
         // When not specifying a body size the application will need to close the output to indicate
         // the transfer is complete, here we use a try with resources approach to accomplish that.
         try
         {
            // Let the streams handle the actual transfer which will block until the full transfer
            // is complete, or if an error occurs either in the file reader or the stream sender
            // the message send should be aborted.
            using Stream output = message.Body;
            inputStream.CopyTo(output);
         }
         catch (Exception)
         {
            message.Abort();
         }

         message.Tracker.AwaitAccepted();
      }
   }
}
