using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EnazaTestTaskClient
{
    public interface IMessageService
    {
        public void OnMessage(Action<string> handler);

        public void OnDisconnect(Func<Exception, Task> func);

        public void OnConnect(Action handler);

        public Task SendMessageAsync(string message);

        public Task<string> GetStore();

        public Task ConnectAsync();
    }
}
