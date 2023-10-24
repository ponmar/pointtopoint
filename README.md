# README #

## About ##

This repository contains code for client/server communication. The code originates from the multiplayer game [Race2k2](https://drive.google.com/drive/folders/1xs8oNkufM9pY0HBzyRL5-QLYBrGgvlLj). It has later been adapted for a more general purpose use.

## Features ##

* Built with .NET Standard 2.0 (can be referenced from projects based on .NET Framework and later .NET versions)
* Message passing over TCP (Newtonsoft JSON or custom object serialization)
* Client side code for settings up a connection to a server and sending/receiving messages
* Server side code for accepting client connections, keeping track of connected clients and sending/receiving messages
* Keep alive message sending for detecting communication problems
* No built-in authentication or encryption, but it can be inmplemented on top of the included message passing

## Message format and example ##

| Serializer                      | Length (4 bytes in local byte order) | Payload (X bytes)                                     |
|:-------------------------------:|:------------------------------------:|:-----------------------------------------------------:|
| NewtonsoftJsonPayloadSerializer |             170                      | `protocol.PublishText,Protocol { Message = "Hello" }` |

Payload notes for the NewtonsoftJsonPayloadSerializer:

* The payload string is serialized and sent as Unicode encoded data
* protocol.PublishText is the serialized object type namespace
* Protocol is the serialized object assembly
* The last part is the JSON data for the serialized object

### Included Object Serializers ###

* NewtonsoftJsonPayloadSerializer (uses Newtonsoft.Json nuget package)
* MsJsonPayloadSerializer (uses System.Text.Json nuget package)

## Documentation ##

See included example projects for how to setup communication, sending and receiving messages.
