# README #

## About ##

This repository contains code for client/server communication. The code originates from the multiplayer game [Race2k2](https://drive.google.com/drive/folders/1xs8oNkufM9pY0HBzyRL5-QLYBrGgvlLj). It has been adapted for a more general use and is indended to be a possible replacement for simple WCF communcication. By converting to this communication it will make it possible to upgrade projects from .NET Framework to later .NET versions.

Features:

* Message passing over TCP (Newtonsoft JSON or custom object serialization)
* Client side code for settings up a connection to a server and sending/receiving messages
* Server side code for accepting client connections, keep track of connected clients and sending/receiving messages
* Keep alive message sending for detecting communication problems
* No authentication
* No encryption

Message format and example:

| Serializer            | Length (4 bytes in local byte order) | Payload (X bytes)                                   |
|:---------------------:|:------------------------------------:|:---------------------------------------------------:|
| NewtonsoftJsonPayload |             170                      | protocol.PublishText,Protocol { Message = "Hello" } |

Payload notes for the NewtonsoftJsonPayload serializer:

* The payload string is serialized and sent as Unicode encoded data
* protocol.PublishText is the serialized object type namespace
* Protocol is the serialized object assembly
* The last part is the object serialized into JSON data with Newtonsoft JSON nuget package

See more details about how to send and receive messages in included example projects.
