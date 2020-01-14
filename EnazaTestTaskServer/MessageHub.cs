using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EnazaTestTaskServer
{
    public class MessageHub : Hub
    {
        private readonly IFileService _fileService;

        public MessageHub(IFileService fileService) : base()
        {
            _fileService = fileService;
        }

        [HubMethodName("SendMessage")]
        public async void SendMessage(string message)
        {
            try
            {
                _fileService.StoreNewMessage(message);

                await Clients.Others.SendAsync("ReceiveMessage", message);
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, "Debug log");
            }
        }
    }
}
