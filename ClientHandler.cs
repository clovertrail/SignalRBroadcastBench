﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Microsoft.Azure.SignalR.Samples.Serverless
{
    public class ClientHandler
    {
        private List<HubConnection> _connectionList;
        private Counter _counter;
        private string _target = "SendMessage";
        public ClientHandler(string connectionString, string hubName, int count, Counter counter)
        {
            _counter = counter;
            var serviceUtils = new ServiceUtils(connectionString);

            var url = GetClientUrl(serviceUtils.Endpoint, hubName);
            _connectionList = new List<HubConnection>(count);
            for (var i = 0; i < count; i++)
            {
                // generate random userId
                var rnd = new Random();
                byte[] content = new byte[8];
                rnd.NextBytes(content);
                var userId = Encoding.UTF8.GetString(content);

                var connection = new HubConnectionBuilder()
                .WithUrl(url, option =>
                {
                    option.AccessTokenProvider = () =>
                    {
                        return Task.FromResult(serviceUtils.GenerateAccessToken(url, userId));
                    };
                }).Build();
                connection.On<string, long>(_target,
                (string server, long message) =>
                {
                    _counter.Latency(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message);
                    _counter.RecordSentSize(_target.Length + 8);
                });
                _connectionList.Add(connection);
            }
        }

        public async Task StartAsync()
        {
            var taskList = new List<Task>(_connectionList.Count);
            for (var i = 0; i < _connectionList.Count; i++)
            {
                taskList.Add(_connectionList[i].StartAsync());
            }
            await Task.WhenAll(taskList);
        }

        public async Task DisposeAsync()
        {
            var taskList = new List<Task>(_connectionList.Count);
            for (var i = 0; i < _connectionList.Count; i++)
            {
                taskList.Add(_connectionList[i].DisposeAsync());
            }
            await Task.WhenAll(taskList);
        }

        private string GetClientUrl(string endpoint, string hubName)
        {
            return $"{endpoint}:5001/client/?hub={hubName}";
        }
    }
}