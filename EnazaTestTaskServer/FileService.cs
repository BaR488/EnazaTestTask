using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnazaTestTaskServer
{
    public class FileService : IFileService
    {
        private string _filePath;

        private ReaderWriterLockSlim lockStore = new ReaderWriterLockSlim();

        private readonly Encoding _DefaultEncoding = Encoding.UTF8;

        private readonly SortedList<string, List<int>> _messagesList = new SortedList<string, List<int>>();

        public FileService(string filePath): base()
        {
            _filePath = filePath;
        }

        public FileService() : base()
        {
            if (File.Exists(_filePath))
            {
                using (StreamReader sr = new StreamReader(_filePath, _DefaultEncoding))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        AppendToList(line, _DefaultEncoding.GetBytes(line));
                    }
                }
            }
        }

        private void AppendToList(string message, byte[] messageBytes)
        {
            if (_messagesList.ContainsKey(message))
            {
                _messagesList[message].Add(messageBytes.Length);
            }
            else
            {
                _messagesList.Add(message, new List<int> {messageBytes.Length});
            }
        }
        
        public void StoreNewMessage(string message)
        {
            var messageBytes = _DefaultEncoding.GetBytes($"{message}{System.Environment.NewLine}");

            lockStore.EnterWriteLock();

            try
            {
                AppendToList(message, messageBytes);

                using (FileStream fstream = new FileStream(_filePath, FileMode.OpenOrCreate))
                {
                    if (fstream.Length != 0)
                    {
                        var newEnd = _messagesList.Take(_messagesList.IndexOfKey(message) + 1)
                            .Select(x => x.Value.Sum()).Sum();
                        byte[] output = new byte[fstream.Length - newEnd + messageBytes.Length * 2];

                        fstream.Seek(-output.Length + messageBytes.Length, SeekOrigin.End);
                        fstream.Read(output, messageBytes.Length, output.Length - messageBytes.Length);

                        for (int i = 0; i < messageBytes.Length; i++)
                        {
                            output[i] = messageBytes[i];
                        }

                        fstream.Seek(newEnd - messageBytes.Length, SeekOrigin.Begin);
                        fstream.Write(output, 0, output.Length);
                    }
                    else
                    {
                        fstream.Write(messageBytes, 0, messageBytes.Length);
                    }
                }
            }
            finally
            {
                lockStore.ExitWriteLock();
            }
        }

        public string GetStoredMessage()
        {
            lockStore.EnterReadLock();
            try
            {
                return File.Exists(_filePath) ? File.ReadAllText(_filePath) : null;
            }
            finally
            {
                lockStore.ExitReadLock();
            }
        }
    }
}