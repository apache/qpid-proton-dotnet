# Sending and Receiving large messages with proton-dotnet

When sending and receiving messages whose size exceeds what might otherwise be acceptable to having in memory all at once the proton-dotnet client has a flexible API to make this process simpler. The stream sender and stream receiver APIs in the proton-dotnet client offer the ability to read and write messages in manageable chunks that prevent application memory being exhausted trying to house the entire message. This API also provides a simple means of streaming files directly vs having to write a large amount of application code to perform such operations.

## Stream senders and receivers

The API for handling large message is broken out into stream senders and stream receivers that behave a bit differently than the standard senders and receivers. Unlike the standard sender and receiver which operate on whole in memory messages the streaming API makes message content available through stream where bytes can be read or written in chunks without the need to have to entire contents in memory at once.  Also the underlying streaming implementation performs tight flow control to prevent the remote from sending to much before the local side has processed it, and sender blocks send operations when necessary to wait for capacity to send pending bytes to the remote.

## Using the stream sender

To send a large message using the stream sender API you need to create a ``IStreamSender`` type which operates similar to the normal Sender API but imposes some restrictions on usage compared to the normal Sender.  Creating the stream sender is shown below:

```
   IStreamSender sender = connection.OpenStreamSender(address)
```

This code opens a new stream sender for the given address and returns a ``IStreamSender`` type which you can then use to obtain the streaming message type which you will use to write the outgoing bytes. Unlike the standard message type the streaming message is created from the sender and it tied directly to that sender instance, and only one streaming message can be active on a sender at any give time. To create an outbound stream message use the following code:

```
   IStreamSenderMessage message = sender.BeginMessage();
```

This requests that the sender initiate a new outbound streaming message and will throw an exception if another stream sender message is still active. The ``IStreamSenderMessage`` is a specialized ``IMessage`` type whose body is an output stream type meaning that it behaves much like a normal message only the application must get a reference to the body output stream to write the outgoing bytes. The code below shows how this can be done in practice.

```
    message.Durable = true;
    message.SetAnnotation("x-opt-annotation", "value");
    message.SetProperty("application-property", "value");

    // Creates an OutputStream that writes a single Data Section whose expected
    // size is configured in the stream options.
    OutputStreamOptions streamOptions = new()
    {
       BodyLength = buffer.Length
    };
    Stream output = message.GetBodyStream(streamOptions);

    while (<has data to send>)
    {
        output.Write(buffer, i, chunkSize);
    }

    output.Close();  // This completes the message send.

    message.Tracker().AwaitAccepted();
```

In the example above the application code has already obtained a stream sender message and uses it much like a normal message, setting application properties and annotations for the receiver to interpret and then begins writing from some data source a stream of bytes that will be encoded into an AMQP ``Data`` section as the body of the message, the sender will ensure that the writes occur in manageable chunks and will not retain the previously written bytes in memory. A write call the the message body output stream can block if the sender is waiting on additional capacity to send or on IO level back-pressure to ease.

Once the application has written all the payload into the message it completes the operation by closing the ``Stream`` and then it can await settlement from the remote to indicate the message was received and processed successfully.

### Sending a large file using the stream sender

Sending a file using the ``IStreamSenderMessage`` is an ideal use case for the stream sender. The first thing the application would need to do is to validate a file exists and open it (this is omitted here). Once a file has been opened the following code can be used to stream to contents to the remote peer.

```
    using IConnection connection = client.Connect(serverHost, serverPort, options);
    using IStreamSender sender = connection.OpenStreamSender(address);
    using FileStream inputStream = File.OpenRead(fileName);

    IStreamSenderMessage message = sender.BeginMessage();

    // Application can inform the other side what the original file name was.
    message.SetProperty(fileNameKey, filename);

    try
    {
       using Stream output = message.Body;
       inputStream.CopyTo(output);
    }
    catch (Exception)
    {
       message.Abort();
    }
```

In the example above the code makes use the .NET API from the ``Stream`` class to transfer the contents of a file to the remote peer, the transfer API will read the contents of the file in small chunks and write those into the provided ``Stream`` which in this case is the stream from our ``IStreamSenderMessage``.

## Using the stream receiver

To receive a large message using the stream receiver API you need to create a ``IStreamReceiver`` type which operates similar to the normal Receiver API but imposes some restrictions on usage compared to the normal Sender.  Creating the stream receiver is shown below:

```
    IStreamReceiver receiver = connection.OpenStreamReceiver(address));
```

This code opens a new stream receiver for the given address and returns a ``IStreamReceiver`` type which you can then use to obtain the streaming message type which you will use to read the incoming bytes. Just like the standard message type the streaming message is received from the receiver instance but and it is tied directly to that receiver as it read incoming bytes from the remote peer, therefore only one streaming message can be active on a receiver at any give time. To create an inbound stream message use the following code:

```
    IStreamDelivery delivery = receiver.Receive();
    IStreamReceiverMessage message = delivery.Message();
```

Once a new inbound streaming message has been received the application can read the bytes by requesting the ``Stream`` from the message body and reading from it as one would any other input stream scenario.

```
    Stream inputStream = message.Body;

    byte[] chunk = new byte[10];
    int readCount = 0;

    while (inputStream.Read(chunk) != 0)
    {
       Console.WriteLine(string.Format("Read data chunk [{0:D2}]: size => {1}", ++readCount, chunk.Length));
    }

```

In the example code above the application reads from the message body input stream and simply writes out small chunks of the body to the system console, the read calls might block while waiting for bytes to arrive from the remote but the application remains unaffected in this case.

### Receiving a large file using the stream receiver

Just as stream sending example from previously sent a large file using the ``IStreamSenderMessage`` an application can receive and write a large message directly into a file with a few quick lines of code.  An example follows which shows how this can be done, the application is responsible for choosing a proper location for the file and verifying that it has write access.

```
     using IConnection connection = client.Connect(serverHost, serverPort, options);
     using IStreamReceiver receiver = connection.OpenStreamReceiver(address);

     IStreamDelivery delivery = receiver.Receive();
     IStreamReceiverMessage message = delivery.Message();
     Stream inputStream = message.Body;

     // Application needs to define where the file should go
     using FileStream outputStream = File.Create(outputLocation);

     message.Body.CopyTo(outputStream);

```

Just as in the stream sender case the application can make use of the .NET transfer API for ``Stream`` instances to handle the bulk of the work reading small blocks of bytes and writing them into the target file, in most cases the application should add more error handling code not shown in the example. Reading from the incoming byte stream can block waiting for data from the remote which may need to be accounted for in some applications.

