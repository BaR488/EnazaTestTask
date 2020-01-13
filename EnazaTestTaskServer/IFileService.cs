using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EnazaTestTaskServer
{
    public interface IFileService
    {
        public void StoreNewMessageAsync(string message);

        public Task<string> GetStoredMessageAsync();
    }
}
