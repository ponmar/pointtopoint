# README #

## About ##

This repository contains code for client/server communication. The code originates from the [Race2k2 project](https://drive.google.com/drive/folders/1xs8oNkufM9pY0HBzyRL5-QLYBrGgvlLj). It has been adapted for a more general use and is indended to be a possible replacement for simple WCF communcication. By converting to this communication it will make it possible to upgrade projects from .net framework to later .net versions.

Features:
* Client and server side code for settings up connections
* Message passing over TCP (Newtonsoft JSON or custom object serialization)
* Keep alive message sending
* No authentication
* No encryption
