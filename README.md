# SignalRBroadcastBench
Benchmark for broadcast message through Azure SignalR

Launch 100 clients:
dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" client 100

Launch 100 servers:
dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" server 100 64 false
