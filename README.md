# SignalRBroadcastBench
Benchmark for broadcast message through Azure SignalR REST API. The test allows many clients to connect SignalR Service and waits for message from SignalR Service. There is one server which sends concurrent messages to SignalR Service through HTTP REST API. The clients and server are binded through a Hub.

The following example first launch 1 client, and then starts the server to broadcast messages to the client. The message contains a timestamp, so when the client receives the message, it gets the latency by calculating 'Now - timestamp', and record all messages' latency distribution.

Launch 100 clients:
dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" client 1

Launch a server which broadcasts 100 messages every second, the message size is 64 bytes (timestamp is not included)
dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" server 100 64 
