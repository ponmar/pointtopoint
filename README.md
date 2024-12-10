# README

## About

This library contains code for client-server communication. The code originates from the multiplayer game [Race2k2](https://drive.google.com/drive/folders/1xs8oNkufM9pY0HBzyRL5-QLYBrGgvlLj). It has later been adapted for a more general purpose use.

## Features

* Lightweight message passing over TCP (IPv4 and IPv6)
* JSON, XML, YAML or custom object serialization and deserialization
* Client side code for settings up a connection to a server, sending/receiving messages and different ways to deliver messages to application specific handling code
* Server side code for accepting client connections, keeping track of connected clients, storing application specific data per client and sending/receiving messages
* Keep alive message sending for detecting communication problems (supervision is however optionally implemented in the client/server application)
* Built with .NET Standard 2.0 (can be referenced from projects based on .NET Framework and later .NET versions)

No authentication or encryption is included, but it can be implemented on top of the included message passing.

## Example Code

Here are some code snippets from the included chat client-server demo application .

Client setup:

```csharp
Messenger = new TcpMessenger(HostnameInput, port,
    new NewtonsoftJsonPayloadSerializer(typeof(PublishText).Assembly),
    new ReflectionMessageRouter(this, Dispatcher.UIThread.Invoke),
    new SocketFactory(),
    TcpMessenger.DefaultSocketOptions);

Messenger.Disconnected += Messenger_Disconnected;
Messenger.Start();
```

Server setup:

```csharp
var clientsHandler = new ClientsHandler<ChatClientHandler>(
    new NewtonsoftJsonPayloadSerializer(typeof(Text).Assembly),
    new ActivatorClientHandlerFactory(),
    new QueuingReflectionMessageRouterFactory());

var tcpServer = new TcpServer(NetworkInterface.AnyIPv4, port, new TcpListenerFactory());
var tcpServerThread = new Thread(() => tcpServer.Run(clientsHandler));
tcpServerThread.Start();
```

Message sending:
```csharp
Messenger!.Send(new PublishText(TextInput));
```

Message receiving (with the reflection based message router):
```csharp
    public void HandleMessage(Text message, IMessenger messenger)
    {
        // Handle Text payload here
    }
```

## Message Serialization

Format for each message sent over TCP:
1. Payload length (integer serialized as 4 bytes)
2. Payload (X number of bytes according to the specified length)

The payload format depends on the selected object serializer. Typically it includes the object type and its serialized data.

Object serializers:

* JSON via Newtonsoft.Json nuget package (see NewtonsoftJsonPayloadSerializer)
* JSON via System.Text.Json nuget package (see MsJsonPayloadSerializer)
* XML (see XmlPayloadSerializer)
* YAML via YamlDotNet nuget package (see YamlPayloadSerializer)
* Custom - Create your own (see IPayloadSerializer)!

## Message Routing

Received messages can be passed to application specific code in the following ways:

* C# event (see EventMessageRouter)
* Community.Toolkit.Mvvm event (see CommunityToolkitMvvmEventMessageRouter)
* Reflection based callback on "void HandleMessage(MyMessagePayload payload, ...)" methods (see ReflectionMessageRouter)
* Custom via lambda expression or action (see CustomMessageRouter)

All message routers have the possibility to use an executor to run the message routing on an appropriate thread.

## Documentation

See included example projects for how to setup communication, sending and receiving messages.

## Contributions

Please create pull requests towards the dev branch.

## License

This project uses the [MIT license](LICENSE.txt).
