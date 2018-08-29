# SignalRBroadcastBench
Console benchmark for broadcast message through Azure SignalR REST API.

The test allows many clients to connect SignalR Service and waits for message from SignalR Service. There is one server which sends concurrent messages to SignalR Service through HTTP REST API. The clients and server are binded through a Hub.

The following example first launch 1 client, and then starts the server to broadcast messages to the client. The message contains a timestamp, so when the client receives the message, it gets the latency by calculating 'Now - timestamp', and record all messages' latency distribution.

Press any key on the console to stop the client or server.

# Example

## Process

Launch 1 client on VM1:

dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" client 1

Launch a server on VM2. It broadcasts 400 messages every second, the message size is 64 bytes (timestamp is not included)

dotnet run -- -c "YOUR_CONNECTION_STRING" -h "TEST_HUB" server 400 64 

The output on client console after running ~2 minutes:

`
{"Time":"2018-08-29T02:48:34Z","Counters":{"message:lt:900":210,"message:lt:500":231,"message:lt:600":228,"message:lt:300":41713,"message:lt:700":177,"message:lt:400":2057,"message:received":45681,"message:ge:1000":816,"message:recvSize":867939,"message:lt:800":249}}
{"Time":"2018-08-29T02:48:35Z","Counters":{"message:lt:900":210,"message:lt:500":231,"message:lt:600":228,"message:lt:300":42087,"message:lt:700":177,"message:lt:400":2084,"message:received":46082,"message:ge:1000":816,"message:recvSize":875558,"message:lt:800":249}}
{"Time":"2018-08-29T02:48:36Z","Counters":{"message:lt:900":210,"message:lt:500":231,"message:lt:600":228,"message:lt:300":42411,"message:lt:700":177,"message:lt:400":2116,"message:received":46438,"message:ge:1000":816,"message:recvSize":882322,"message:lt:800":249}}
`

The output on server console after running ~2 minutes:

`
{"Time":"2018-08-29T02:48:31Z","Counters":{"message:sent":44257,"message:sendSize":840883}}
{"Time":"2018-08-29T02:48:32Z","Counters":{"message:sent":44671,"message:sendSize":848749}}
{"Time":"2018-08-29T02:48:33Z","Counters":{"message:sent":45071,"message:sendSize":856349}}
{"Time":"2018-08-29T02:48:34Z","Counters":{"message:sent":45471,"message:sendSize":863949}}
{"Time":"2018-08-29T02:48:35Z","Counters":{"message:sent":45875,"message:sendSize":871625}}
`

## Explanations:

"message:lt":XXX means the message count for latency lower than XXX millionseconds.

"message:received":XXX means the received message count.

"message:send":XXX shows the sent message count. Generally, the received count should be approximately equal to sent count. If received count is far more less than sent count, that means message stocked.

"message:ge":1000 gives you the total count for message latency greater than 1000 millionseconds.

## How to find the performance number

This tool helps you find the best server sending numbers.

The recommended methology is using percentage of message count less than 1000 millionseconds.

If Count("message:ge":1000)/Count("message:received") < 0.01, that means 99% of the sending message were received less than 1000 millionseconds.

But if that value >= 0.01, the message latency is higher. For example, in the above case, the "message:ge:1000":816 tells us 816 message latency larger than 1000 ms, and the "message:received":4643 gives 4643.

Let us see the percentage of latency > 1000ms: 816/4643=0.1757. It is bad. So, sending 400 message/second is not acceptable for SignalR Service. You should lower that sending number.
