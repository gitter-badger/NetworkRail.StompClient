## Network Rail Stomp Client

This is a basic STOMP client written in C# using the .net framework. It can be used to subscribe to and collect messages from the [Network Rail data feeds](http://datafeeds.networkrail.co.uk).

The client can be configured to post the JSON content of each message to a specified URL. The theory being that a site/system elsewhere will receive these JSON messages and process and/or store them accordingly.

## Licensing

The client code in this project is licensed under the MIT license (refer to LICENSE file). 

*NOTE* : This client uses binaries from the Apache NMS project which is licensed under the [Apache License 2.0](http://www.apache.org/licenses/LICENSE-2.0.html)
