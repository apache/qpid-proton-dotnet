# Apache Qpid Proton DotNet Client

Qpid Proton DotNet is a high-performance, lightweight AMQP Client that provides an
imperative API which can be used in the widest range of messaging applications.

## Adding the client to your .NET application

Using the `dotnet` CLI you can add a reference to the Qpid proton-dotnet client to your application
which will also download release binaries from the Nuget gallery. The following command
should be run (with the appropriate version updated) in the location where you project
file is saved.

    dotnet add package Apache.Qpid.Proton.Client --version 1.0.0-M9

Following this command your 'csproj' file should be updated to contain a reference to
to the proton-dotnet client library and should look similar to the following example:

    <ItemGroup>
      <PackageReference Include="Apache.Qpid.Proton.Client" Version="1.0.0-M9" />
    </ItemGroup>

Users can manually add this reference as well and use the `dotnet restore` command to
fetch the artifacts from the Nuget gallery.

## Creating a connection

The entry point for creating new connections with the proton-dotnet client is the IClient
type which provides a simple static factory method to create new instances.

    IClient container = IClient.Create();

The IClient instance serves as a container for connections created by your application and
can be used to close all active connections and provides the option of adding configuration
to set the AMQP container Id that will be set on connections created from a given client
instance.

Once you have created a IClient instance you can use that to create new connections which
will be of type IConnection. The IClient instance provides API for creating a connection
to a given host and port as well as providing connection options object that carry a large
set of connection specific configuration elements to customize the behavior of your connection.
The basic create API looks as follows:

    IConnection connection = container.Connect(remoteAddress, remotePort, new ConnectionOptions());

From you connection instance you can then proceed to create sessions, senders and receivers that
you can use in your application.

### Sending a message

Once you have a connection you can create senders that can be used to send messages to a remote
peer on a specified address. The connection instance provides methods for creating senders and
is used as follows:

    ISender sender = connection.OpenSender("address");

A message instance must be created before you can send it and the IMessage interface provides
simple static factory methods for common message types you might want to send, for this example
we will create a message that carries text in an AmqpValue body section:

    IMessage<string> message = IMessage<string>.Create("Hello World");

Once you have the message that you want to send the previously created sender can be used as
follows:

    ITracker tracker = sender.Send(message);

The Send method of a sender will attempt to send the specified message and if the connection
is open and the send can be performed it will return a ITracker instance to provides API for
checking if the remote has accepted the message or applied other AMQP outcomes to the sent
message.

### Receiving a message

To receive a message sent to the remote peer a Receiver instance must be created that listens
on a given address for new messages to arrive. The connection instance provides methods for
creating receivers and is used as follows:

    IReceiver receiver = connection.OpenReceiver("address");

After creating the receiver the application needs to provide credit to the remote which allows
for control of how many messages a remote can send to the receiver. We will add a single credit
here indicating to the remote that one message can be sent to this receiver:

    receiver.AddCredit(1);

After having granted credit to the above created receiver the application can then call one of
the available receive APIs to await the arrival of a message from a remote sender.

    IDelivery delivery = receiver.Receive();

Once a delivery arrives an IDelivery instance is returned which provides API to both access
the delivered message and to provide a disposition to the remote indicating if the delivered
message is accepted or was rejected for some reason etc. The message is obtained by calling
the message API as follows:

    IMessage<object> received = delivery.Message();

Once the message is examined and processed the application can accept delivery by calling
the accept method from the delivery object as follows:

    delivery.Accept();

Other settlement options exist in the delivery API which provide the application wil full
access to the AMQP specification delivery outcomes for the received message.

Please see http://qpid.apache.org/proton for more information.
