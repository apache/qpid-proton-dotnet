# Client Fault Tolerance Configuration

The proton-dotnet client supports configuration to enable a connection to be handle both reestablished after an interruption and handling not being able to initially connect to a remote peer.

## Enabling Reconnection

By default the client does not attempt to reconnect to the configured remote peer, this can be easily done though by toggling the appropriate configuration option as follows:

```
   ConnectionOptions connectionOpts = new();
   connectionOpts.ReconnectEnabled = true;
```

Once enabled the client will try indefinitely to connect and if disconnected to reconnect to the remote peer that was specified in the connection call that created the connection instance.  Additional options exist to control how many attempts to connect or reconnect are performed before the client gives up and marks the connection as failed. An example of configuring reconnection attempt and delays is below, see the full [configuration](Configuration.md) document for all the available options and a description of what each does.

```
    ConnectionOptions options = new();
    options.ReconnectOptions.ReconnectEnabled = true;
    options.ReconnectOptions.MaxReconnectAttempts = 5;
    options.ReconnectOptions.MaxInitialConnectionAttempts = 5;
    options.ReconnectOptions.ReconnectDelay = 10;
```

Additional remote locations can be added to the reconnection options to allow for reconnect to an alternative location should the host specified in the connect API be unavailable, these hosts will always be tried after the host specified in the connect API and will be tried in order until a new connection is established.

```
    ConnectionOptions options = new();
    options.ReconnectOptions.ReconnectEnabled = true;
    options.ReconnectOptions.AddReconnectLocation("host2", 5672);
    options.ReconnectOptions.AddReconnectLocation("host3", 5672);
    options.ReconnectOptions.AddReconnectLocation("host4", 5672);
```

## Reconnection and Client behavior

The client reconnect handling is transparent in most cases and application code need not be adjusted to handle special case scenarios. In some very special cases the application nay need to make some configuration changes depending on how the client is used which mostly involves choosing timeouts for actions such as send timeouts.

A few select client operations and their behaviors during connection interruption as documented below:

+ **In Flight Send** A message that was sent and a valid tracker instance was returned will throw exception from any of the tracker methods that operate or wait on send outcomes which indicate a failure as the client cannot be certain if the send completed or failed.
+ **Send blocked on credit** A send that is blocked waiting on credit will continue to wait during a connection interruption and only be failed if the client reaches configured reconnect limits, or the configured send timeout is reached.
+ **Active transactions** If the application begins a new transaction and the client connection is interrupted before the transaction is committed the transaction will be marked as invalid and any call to commit will throw an exception, a call to roll back will succeed.
+ **Handling received messages** If the application received a delivery and attempts to accept it (or apply any other outcome) the disposition operation will fail indicating the disposition could not be applied.

## Reconnection event notifications

An application can configure event handlers that will be notified for various events related to the reconnection handling of the proton-dotnet client. The events available for subscription consist of following types:

+ **Connected** The client succeeded in establishing an initial connection to a remote peer.
+ **Interrupted** The client connection to a remote peer was broken it will now attempt to reconnect.
+ **Reconnected** The client succeeded in establishing an new connection to remote peer after having been interrupted.
+ **Disconnected** The client failed to establish a new connection and the configured attempt limit was reached (if set).

To subscribe to one of the above events the application must set an event handler in the connection options instance for the desired event.

As an example the client can set a handler to called upon the first successful connection to a remote peer and the event would carry the host and port where the connection was established to in a ConnectionEvent object.

```
    ConnectionOptions options = new();

    options.ConnectedHandler = (conn, location) => ... Your code here ... ;

```

Then to be notified when an active connection is interrupted a handler is set in the connection which will be called with an disconnection event that carries the host and port that the client was connected to and an exception that provides any available details on the reason for disconnection.

```
    ConnectionOptions options = new();

    options.InterruptedHandler = (conn, location) => ... Your code here ... ;

```

To be notified when a connection that was previously interrupted is successfully able to reconnect to one of the configured remote peers the reconnection event can be used which will be notified on reconnect and provided a connection event object that carries the host and port that the client reconnected to:

```
    ConnectionOptions options = new();

    options.ReconnectedHandler = (conn, location) => ... Your code here ... ;
```

To be notified when the client has given up on reconnection due to exceeding the configured reconnection attempt the application can set a handler on the disconnected event which will be given a disconnection event object that carries the host and port of the last location the client was successfully connected to and an exception object that provides any available details on the failure cause.

```
    ConnectionOptions options = new();

    options.DisconnectedHandler = (conn, location) => ... Your code here ... ;
```
