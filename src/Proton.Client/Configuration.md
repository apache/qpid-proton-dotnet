# Qpid proton-dotnet Imperative Client configuration

This file details various configuration options for the Imperative API based .NET client. Each of the resources
that allow configuration accept a configuration options object that encapsulates all configuration for that specific
resource.

## Client Options

Before creating a new connection a Client object is created which accepts a ClientOptions object to configure it.

     ClientOptions clientOptions = new();
     clientOptions.Id = "container-name";
     IClient client = IClient.Create(clientOptions);

The following options are available for configuration when creating a new **Client** instance.

+ **clientOptions.Id** Allows configuration of the AMQP Container Id used by newly created Connections, if none is set the Client instance will create a unique Container Id that will be assigned to all new connections.

## Connection Configuration Options

The ConnectionOptions object can be provided to a Client instance when creating a new connection and allows configuration of several different aspects of the resulting Connection instance.

     ConnectionOptions connectionOptions = new();
     connectionOptions.Username = "user";
     connectionOptions.Password = "pass";
     IConnection connection = client.connect(serverHost, serverPort, connectionOptions);

The following options are available for configuration when creating a new **Connection**.

+ **connectionOptions.Username** User name value used to authenticate the connection
+ **connectionOptions.Password** The password value used to authenticate the connection
+ **connectionOptions.SslEnabled** A connection level convenience option that enables or disables the transport level SSL functionality.  See the connection transport options for more details on SSL configuration, if nothing is configures the connection will attempt to configure the SSL transport using the standard system level configuration properties.
+ **connectionOptions.CloseTimeout** Timeout value that controls how long the client connection waits on resource closure before returning. By default the client waits 60 seconds for a normal close completion event.
+ **connectionOptions.SendTimeout** Timeout value that controls how long the client connection waits on completion of a synchronous message send before returning an error. By default the client will wait indefinitely for a send to complete.
+ **connectionOptions.OpenTimeout** Timeout value that controls how long the client connection waits on the AMQP Open process to complete  before returning with an error. By default the client waits 15 seconds for a connection to be established before failing.
+ **connectionOptions.RequestTimeout** Timeout value that controls how long the client connection waits on completion of various synchronous interactions, such as initiating or retiring a transaction, before returning an error. Does not affect synchronous message sends. By default the client will wait indefinitely for a request to complete.
+ **connectionOptions.DrainTimeout** Timeout value that controls how long the client connection waits on completion of a drain request for a Receiver link before failing that request with an error.  By default the client waits 60 seconds for a normal link drained completion event.
+ **connectionOptions.VirtualHost** The vhost to connect to. Used to populate the Sasl and Open hostname fields. Default is the main hostname from the hostname provided when opening the Connection.
+ **connectionOptions.TraceFrames** Configure if the newly created connection should enabled AMQP frame tracing to the system output.

### Connection Transport Options

The ConnectionOptions object exposes a set of configuration options for the underlying I/O transport layer known as the TransportOptions which allows for fine grained configuration of network level options.

     ConnectionOptions connectionOptions = new();
     connectionOptions.TransportOptions.TcpNoDelay = false;
     connectionOptions.Username = "user";
     connectionOptions.Password = "pass";
     IConnection connection = client.connect(serverHost, serverPort, connectionOptions);

The following transport layer options are available for configuration when creating a new **Connection**.

+ **TransportOptions.SendBufferSize** default is 64k
+ **TransportOptions.ReceiveBufferSize** default is 64k
+ **TransportOptions.SoLinger** default is -1
+ **TransportOptions.TcpNoDelay** default is true

### Connection SSL Options

If an secure connection is desired the ConnectionOptions exposes another options type for configuring the client for that, the SslOptions.

     ConnectionOptions connectionOptions = new();
     connectionOptions.Username = "user";
     connectionOptions.Password = "pass";
     connectionOptions.SslOptions.SslEnabled = true;
     connectionOptions.SslOptions.VerifyHost = true;
     IConnection connection = client.connect(serverHost, serverPort, connectionOptions);

The following SSL layer options are available for configuration when creating a new **Connection**.

+ **SslOptions.SslEnabled** Enables or disables the use of the SSL transport layer, default is false.
+ **SslOptions.EnableCertificateRevocationChecks**  Should Cerfiticate revocation checks be enabled (defaults to false).
+ **SslOptions.VerifyHost** Whether to verify that the hostname being connected to matches with the provided server certificate. Defaults to true.
+ **SslOptions.ServerNameOverride** Value used to validate the common name (server name) provided in the server certificate, the default is to use the host name used on connect.

### Connection Automatic Reconnect Options

When creating a new connection it is possible to configure that connection to perform automatic connection recovery.

     ConnectionOptions connectionOptions = new();
     connectionOptions.Username = "user";
     connectionOptions.Password = "pass";
     connectionOptions.ReconnectionOptions.ReconnectEnabled = true;
     connectionOptions.ReconnectionOptions.ReconnectDelay = 30_000;
     connectionOptions.ReconnectionOptions.AddReconnectLocation(hostname, port);
     IConnection connection = client.connect(serverHost, serverPort, connectionOptions);

The following connection automatic reconnect options are available for configuration when creating a new **Connection**.

* **ReconnectionOptions.ReconnectEnabled** enables connection level reconnect for the client, default is false.
+ **ReconnectionOptions.ReconnectDelay** Controls the delay between successive reconnection attempts, defaults to 10 milliseconds.  If the backoff option is not enabled this value remains constant.
+ **ReconnectionOptions.MaxReconnectDelay** The maximum time that the client will wait before attempting a reconnect.  This value is only used when the backoff feature is enabled to ensure that the delay doesn't not grow too large.  Defaults to 30 seconds as the max time between connect attempts.
+ **ReconnectionOptions.UseReconnectBackOff** Controls whether the time between reconnection attempts should grow based on a configured multiplier.  This option defaults to true.
+ **ReconnectionOptions.ReconnectBackOffMultiplier** The multiplier used to grow the reconnection delay value, defaults to 2.0d.
+ **ReconnectionOptions.MaxReconnectAttempts** The number of reconnection attempts allowed before reporting the connection as failed to the client.  The default is no limit or (-1).
+ **ReconnectionOptions.MaxInitialConnectionAttempts** For a client that has never connected to a remote peer before this option control how many attempts are made to connect before reporting the connection as failed.  The default is to use the value of maxReconnectAttempts.
+ **ReconnectionOptions.WarnAfterReconnectAttempts** Controls how often the client will log a message indicating that failover reconnection is being attempted.  The default is to log every 10 connection attempts.
+ **ReconnectionOptions.AddReconnectHost** Allows additional remote peers to be configured for use when the original connection fails or cannot be established to the host provided in the **connect** call.

## Session Configuration Options

When creating a new Session the **SessionOptions** object can be provided which allows some control over various behaviors of the session.

     SessionOptions sessionOptions = new();
     ISession session = connection.OpenSession(sessionOptions);

The following options are available for configuration when creating a new **Session**.

+ **sessionOptions.CloseTimeout** Timeout value that controls how long the client session waits on resource closure before returning. By default the client uses the matching connection level close timeout option value.
+ **sessionOptions.SendTimeout** Timeout value that sets the Session level default send timeout which can control how long a Sender waits on completion of a synchronous message send before returning an error. By default the client uses the matching connection level send timeout option value.
+ **sessionOptions.OpenTimeout** Timeout value that controls how long the client Session waits on the AMQP Open process to complete  before returning with an error. By default the client uses the matching connection level close timeout option value.
+ **sessionOptions.RequestTimeout** Timeout value that controls how long the client connection waits on completion of various synchronous interactions, such as initiating or retiring a transaction, before returning an error. Does not affect synchronous message sends. By default the client uses the matching connection level request timeout option value.
+ **sessionOptions.DrainTimeout** Timeout value that controls how long the Receiver create by this Session waits on completion of a drain request before failing that request with an error.  By default the client uses the matching connection level drain timeout option value.

## Sender Configuration Options

When creating a new Sender the **SenderOptions** object can be provided which allows some control over various behaviors of the sender.

     SenderOptions senderOptions = new();
     ISender sender = session.OpenSender("address", senderOptions);

The following options are available for configuration when creating a new **Sender**.

+ **SenderOptions.CloseTimeout** Timeout value that controls how long the client Sender waits on resource closure before returning. By default the client uses the matching session level close timeout option value.
+ **SenderOptions.SendTimeout** Timeout value that sets the Sender default send timeout which can control how long a Sender waits on completion of a synchronous message send before returning an error. By default the client uses the matching session level send timeout option value.
+ **SenderOptions.OpenTimeout** Timeout value that controls how long the client Sender waits on the AMQP Open process to complete  before returning with an error. By default the client uses the matching session level close timeout option value.
+ **SenderOptions.RequestTimeout** Timeout value that controls how long the client connection waits on completion of various synchronous interactions, such as initiating or retiring a transaction, before returning an error. Does not affect synchronous message sends. By default the client uses the matching session level request timeout option value.

## Receiver Configuration Options

When creating a new Receiver the **ReceiverOptions** object can be provided which allows some control over various behaviors of the receiver.

     ReceiverOptions receiverOptions = new();
     IReceiver receiver = session.OpenReceiver("address", receiverOptions);

The following options are available for configuration when creating a new **Receiver**.

+ **receiverOptions.CreditWindow** Configures the size of the credit window the Receiver will open with the remote which the Receiver will replenish automatically as incoming deliveries are read.  The default value is 10, to disable and control credit manually this value should be set to zero.
+ **receiverOptions.CloseTimeout** Timeout value that controls how long the **Receiver** waits on resource closure before returning. By default the client uses the matching session level close timeout option value.
+ **receiverOptions.OpenTimeout** Timeout value that controls how long the **Receiver** waits on the AMQP open process to complete before returning with an error. By default the client uses the matching session level open timeout option value.
+ **receiverOptions.RequestTimeout** Timeout value that controls how long the client Receiver waits on completion of various synchronous interactions, such settlement of a delivery, before returning an error. By default the client uses the matching session level request timeout option value.
+ **receiverOptions.DrainTimeout** Timeout value that controls how long the Receiver link waits on completion of a drain request before failing that request with an error.  By default the client uses the matching session level drain timeout option value.

## Stream Sender Configuration Options

When creating a new Sender the **SenderOptions** object can be provided which allows some control over various behaviors of the sender.

     StreamSenderOptions streamSenderOptions = new();
     IStreamSender streamSender = connection.OpenStreamSender("address", streamSenderOptions);

The following options are available for configuration when creating a new **StreamSender**.

+ **streamSenderOptions.CloseTimeout** Timeout value that controls how long the **StreamSender** waits on resource closure before returning. By default the client uses the matching session level close timeout option value.
+ **streamSenderOptions.SendTimeout** Timeout value that sets the Sender default send timeout which can control how long a Sender waits on completion of a synchronous message send before returning an error. By default the client uses the matching session level send timeout option value.
+ **streamSenderOptions.OpenTimeout** Timeout value that controls how long the **StreamSender** waits on the AMQP Open process to complete  before returning with an error. By default the client uses the matching session level close timeout option value.
+ **streamSenderOptions.RequestTimeout** Timeout value that controls how long the **StreamSender** waits on completion of various synchronous interactions, such as initiating or retiring a transaction, before returning an error. Does not affect synchronous message sends. By default the client uses the matching session level request timeout option value.

## Stream Receiver Configuration Options

When creating a new Receiver the **ReceiverOptions** object can be provided which allows some control over various behaviors of the receiver.

     StreamReceiverOptions streamReceiverOptions = new();
     IStreamReceiver streamReceiver = connection.openStreamReceiver("address", streamReceiverOptions);

The following options are available for configuration when creating a new **StreamReceiver**.

+ **streamReceiverOptions.CreditWindow** Configures the size of the credit window the Receiver will open with the remote which the Receiver will replenish automatically as incoming deliveries are read.  The default value is 10, to disable and control credit manually this value should be set to zero.
+ **streamReceiverOptions.CloseTimeout** Timeout value that controls how long the **StreamReceiver** waits on resource closure before returning. By default the client uses the matching session level close timeout option value.
+ **streamReceiverOptions.OpenTimeout** Timeout value that controls how long the **StreamReceiver** waits on the AMQP Open process to complete  before returning with an error. By default the client uses the matching session level close timeout option value.
+ **streamReceiverOptions.RequestTimeout** Timeout value that controls how long the **StreamReceiver** waits on completion of various synchronous interactions, such settlement of a delivery, before returning an error. By default the client uses the matching session level request timeout option value.
+ **streamReceiverOptions.DrainTimeout** Timeout value that controls how long the **StreamReceiver** link waits on completion of a drain request before failing that request with an error.  By default the client uses the matching session level drain timeout option value.
