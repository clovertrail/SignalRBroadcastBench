// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Azure.SignalR.Samples.Serverless
{
    public class ServerHandler : IDisposable
    {
        private HttpClient _client;
        private List<HttpClient> _clientList;
        private readonly string _serverName;

        private readonly ServiceUtils _serviceUtils;

        private readonly string _hubName;

        private readonly string _endpoint;

        private Timer _timer;

        private TimeSpan _interval = TimeSpan.FromMilliseconds(1000);

        private string _broadcastUrl;

        private string _target = "SendMessage";

        private bool _start = false;

        private bool _disposed = false;

        private string _content;
        private int _count;
        private Counter _counter;
        private bool _multipleHttpClient;
        private bool _errorOccurs;

        public ServerHandler(string connectionString, string hubName, Counter counter, int count, int sz, bool multipleHttpClient=false)
        {
            _counter = counter;
            _count = count;
            _multipleHttpClient = multipleHttpClient;
            if (_multipleHttpClient)
            {
                _clientList = new List<HttpClient>(_count);
                for (var i = 0; i < _count; i++)
                {
                    var httpClient = new HttpClient();
                    _clientList.Add(httpClient);
                }
            }
            else
            {
                _client = new HttpClient();
            }
            
            _serverName = GenerateServerName();
            _serviceUtils = new ServiceUtils(connectionString);
            _hubName = hubName;
            _endpoint = _serviceUtils.Endpoint;
            _broadcastUrl = GetBroadcastUrl(_hubName);
            var rnd = new Random();
            byte[] content = new byte[sz];
            rnd.NextBytes(content);
            _content = Encoding.UTF8.GetString(content);
            _errorOccurs = false;
            _timer = new Timer(Broadcast, this, _interval, _interval);
            _counter.StartPrint();
        }

        private void Broadcast(object state)
        {
            ServerHandler rb = (ServerHandler)state;
            rb.BroadcastImpl();
        }

        private void BroadcastImpl()
        {
            if (_start && !_errorOccurs)
            {
                Task.Run(async () =>
                {
                    await SendBroadcastRequest();
                });
            }
        }

        public void Start()
        {
            _start = true;
        }

        public void Stop()
        {
            _start = false;
        }

        public async Task SendBroadcastRequest()
        {
            for (var i = 0; i < _count; i++)
            {
                HttpClient client = null;
                if (_multipleHttpClient)
                {
                    client = _clientList[i];
                }
                else
                {
                    client = _client;
                }
                try
                {
                    var request = BuildRequest(_broadcastUrl);
                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    _counter.RecordSentSize(_target.Length + 8);
                }
                catch (Exception e)
                {
                    _errorOccurs = true;
                    Console.WriteLine($"Fail to send message: {e.Message}");
                }
            }
        }

        private Uri GetUrl(string baseUrl)
        {
            return new UriBuilder(baseUrl).Uri;
        }

        private string GetSendToUserUrl(string hubName, string userId)
        {
            return $"{GetBaseUrl(hubName)}/user/{userId}";
        }

        private string GetSendToUsersUrl(string hubName, string userList)
        {
            return $"{GetBaseUrl(hubName)}/users/{userList}";
        }

        private string GetSendToGroupUrl(string hubName, string group)
        {
            return $"{GetBaseUrl(hubName)}/group/{group}";
        }

        private string GetSendToGroupsUrl(string hubName, string groupList)
        {
            return $"{GetBaseUrl(hubName)}/groups/{groupList}";
        }

        private string GetBroadcastUrl(string hubName)
        {
            return $"{GetBaseUrl(hubName)}";
        }

        private string GetBaseUrl(string hubName)
        {
            return $"{_endpoint}:5002/api/v1-preview/hub/{hubName.ToLower()}";
        }

        private string GenerateServerName()
        {
            return $"{Environment.MachineName}_{Guid.NewGuid():N}";
        }

        private HttpRequestMessage BuildRequest(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, GetUrl(url));

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _serviceUtils.GenerateAccessToken(url, _serverName));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payloadRequest = new PayloadMessage
            {
                Target = _target,
                Arguments = new[]
                {
                    _serverName,
                    $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(payloadRequest), Encoding.UTF8, "application/json");

            return request;
        }

        private void ShowHelp()
        {
            Console.WriteLine("*********Usage*********\n" +
                              "send user <User Id>\n" +
                              "send users <User Id List>\n" +
                              "send group <Group Name>\n" +
                              "send groups <Group List>\n" +
                              "broadcast\n" +
                              "***********************");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer.Dispose();
                _client.Dispose();
            }
        }
    }

    public class PayloadMessage
    {
        public string Target { get; set; }

        public string[] Arguments { get; set; }
    }
}
