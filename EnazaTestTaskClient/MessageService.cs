using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EnazaTestTaskClient
{
    public class MessageService : IMessageService
    {
        private HubConnection connection;
        private HttpClient _httpClient = new HttpClient();
        private Action Connect;

        public MessageService(string serverUri)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{serverUri}/message")
                .Build();
            _httpClient.BaseAddress = new Uri(serverUri);
        }

        public void OnMessage(Action<string> handler)
        {
            connection.On<string>("ReceiveMessage", handler);
        }

        public void OnDisconnect(Func<Exception, Task> func)
        {
            connection.Closed += func;
        }

        public void OnConnect(Action handler)
        {
            Connect = handler;
        }

        public async Task SendMessageAsync(string message)
        {
            await connection.InvokeAsync("SendMessage", message);
        }

        public Task<string> GetStore()
        {
            return _httpClient.GetStringAsync("api/store");
        }

        public async Task ConnectAsync()
        {
            try
            {
                await connection.StartAsync();

                Connect?.Invoke();
            }
            catch (Exception ex)
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await ConnectAsync();
            }
        }
    }
}