# Getting started with the Qpid proton-dotnet Client Library

This client provides an imperative API for AMQP messaging applications

Below are some quick pointers you might find useful.

## Adding the client to your .NET application

Using the `dotnet` CLI you can add a reference to the Qpid proton-dotnet client to your application which will also download release binaries from the Nuget gallery. The following command should be run (with the appropriate version updated) in the location where you project file is saved.

    dotnet add package Apache.Qpid.Proton.Client --version 1.0.0-M9

Following this command your 'csproj' file should be updated to contain a reference to to the proton-dotnet client library and should look similar to the following example:

    <ItemGroup>
      <PackageReference Include="Apache.Qpid.Proton.Client" Version="1.0.0-M9" />
    </ItemGroup>

Users can manually add this reference as well and use the `dotnet restore` command to fetch the artifacts from the Nuget gallery.

## Creating a connection

The entry point for creating new connections with the proton-dotnet client is the Client type which provides a simple static factory method to create new instances.

```
    IClient container = IClient.Create();
```

The ``IClient`` instance serves as a container for connections created by your application and can be used to close all active connections and provides the option of adding configuration to set the AMQP container Id that will be set on connections created from a given client instance.

Once you have created a Client instance you can use that to create new connections which will be of type ``IConnection``. The Client instance provides API for creating a connection to a given host and port as well as providing connection options that carry a large set of connection specific configuration elements to customize the behavior of your connection. The basic create API looks as follows:

```
    IConnection connection = container.Connect(remoteAddress, remotePort, new ConnectionOptions());
```

The above code provides host, port and connection options however you may omit the options and proceed using client defaults which will be suitable for many applications. From your connection instance you can then proceed to create sessions, senders and receivers that you can use in your application.

### Sending a message

Once you have a connection you can create senders that can be used to send messages to a remote peer on a specified address. The connection instance provides methods for creating senders and
is used as follows:

```
    ISender sender = connection.OpenSender("address");
```

A message instance must be created before you can send it and the Message interface provides simple static factory methods for comon message types you might want to send, for this example
we will create a message that carries text in an AmqpValue body section:

```
   IMessage<string> message = IMessage<string>.Create("Hello World");
```

Once you have the message that you want to send the previously created sender can be used as follows:

```
    ITracker tracker = sender.Send(message);
```

The Send method of a sender will attempt to send the specified message and if the connection is open and the send can be performed it will return a ``ITracker`` instance to provides API for
checking if the remote has accepted the message or applied other AMQP outcomes to the sent message. The send method can block if the sender lacks credit to send at the time it is called, it will await credit from the remote and send the message once credit is granted.

#### Creating a message

The application code can create a message to be sent by using static factory methods in the ``IMessage`` type that can later be sent using the ``ISender`` type. These factory methods accept a few types that map nicely into the standard AMQP message format body sections. A typical message creation example is shown below.

```
   IMessage<string> message = IMessage<string>.Create("Hello World");
```

The above code creates a new message object that carries a string value in the body of an AMQP message and it is carried inside an ``AmqpValue`` section. Other methods exist that wrap other types in the appropriate section types, a list of those is given below:

+ **IDictionary** The factory method creates a message with the Map value wrapped in an ``AmqpValue`` section.
+ **IList** The factory method creates a message with the List value wrapped in an ``AmqpSequence`` section.
+ **byte[]** The factory method creates a message with the byte array wrapped in an ``Data`` section.
+ **Object** All other objects are assumed to be types that should be wrapped in an ``AmqpValue`` section.

It is also possible to create an empty message and set a body and it will be wrapped in AMQP section types following the same rules as listed above. Advanced users should spend time reading the API documentation of the ``IMessage`` type to learn more.

### Receiving a message

To receive a message sent to the remote peer a ``IReceiver`` instance must be created that listens on a given address for new messages to arrive. The connection instance provides methods for
creating receivers and is used as follows:

```
    IReceiver receiver = connection.OpenReceiver("address");
```

After creating the receiver the application can then call one of the available receive APIs to await the arrival of a message from a remote sender.

```
    IDelivery delivery = receiver.Receive();
```

By default receivers created from the client API have a credit window configured and will manage the outstanding credit with the remote for your application however if you have
configured the client not to manage a credit window for you then your application will need to provide receiver credit before invoking the receive APIs.

```
    receiver.AddCredit(1);
```

Once a delivery arrives an ``IDelivery`` instance is returned which provides API to both access the delivered message and to provide a disposition to the remote indicating if the delivered
message is accepted or was rejected for some reason etc. The message is obtained by calling the message API as follows:

```
    IMessage<object> received = delivery.Message();
```

Once the message is examined and processed the application can accept delivery by calling the accept method from the delivery object as follows:

```
    delivery.Accept();
```

Other settlement options exist in the delivery API which provide the application wil full access to the AMQP specification delivery outcomes for the received message.

## Examples

First build and install all the modules as detailed above (if running against a source checkout/release, rather than against released binaries) and then consult the README in the Examples directory.

