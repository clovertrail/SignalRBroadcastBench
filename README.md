# SignalRBroadcastBench
Benchmark for broadcast message through Azure SignalR

Launch 100 clients:
dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" client -c 100

Launch servers:
dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" server -s 100 -z 64
