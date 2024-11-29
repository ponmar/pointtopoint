# README

## About

This library contains code for client-server communication. The code originates from the multiplayer game [Race2k2](https://drive.google.com/drive/folders/1xs8oNkufM9pY0HBzyRL5-QLYBrGgvlLj). It has later been adapted for a more general purpose use.

## Features

* Message passing over TCP (JSON, YAML or custom object serialization/deserialization)
* Client side code for settings up a connection to a server and sending/receiving messages
* Server side code for accepting client connections, keeping track of connected clients, storing application specific data per client and sending/receiving messages
* Keep alive message sending for detecting communication problems (supervision is however optionally implemented in the client/server application)
* Built with .NET Standard 2.0 (can be referenced from projects based on .NET Framework and later .NET versions)

No authentication or encryption is included, but it can be implemented on top of the included message passing.

## Message Format

1. Payload length (integer serialized as 4 bytes)
2. Payload (X number of bytes according to the specified length)

## Payload Format

The payload format depends on the selected object serializer. Typically it includes the object type and its serialized data.

### Included Object Serializers

* JSON via Newtonsoft.Json nuget package (see NewtonsoftJsonPayloadSerializer)
* JSON via System.Text.Json nuget package (see MsJsonPayloadSerializer)
* YAML via YamlDotNet nuget package (see YamlPayloadSerializer)

## Documentation

See included example projects for how to setup communication, sending and receiving messages.

## License

This project uses the [MIT license](LICENSE.txt).
