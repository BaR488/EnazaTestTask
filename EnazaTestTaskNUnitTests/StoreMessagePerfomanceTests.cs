using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnazaTestTaskServer;
using NUnit.Framework;

namespace EnazaTestTaskNUnitTests
{
    public class StoreMessagePerfomanceTests
    {
        private const int taskCount = 10;

        private const int dataCount = 1000;

        private readonly List<string> dataList = new List<string>();

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [OneTimeSetUp]
        public void Setup()
        {
            for (int i = 0; i < dataCount; i++)
            {
                dataList.Add(RandomString(random.Next(10) + 1));
            }

            dataList.Sort();
        }

        /// <summary>
        /// “ест работы функций хранени€ и получени€ сообщений, с замером производительности
        /// </summary>
        [Test]
        public void TestSmart()
        {
            Stopwatch sw = new Stopwatch();
            var fileName = $"{MethodBase.GetCurrentMethod().Name}.txt";

            File.Delete(fileName);

            var fileService = new FileService(fileName);

            sw.Start();
            Task[] taskArray = new Task[taskCount];
            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew((index) =>
                {
                    var diff = int.Parse(index.ToString());
                    for (int j = diff;
                        j < dataList.Count;
                        j += taskCount)
                    {
                        fileService.StoreNewMessage(dataList[j]);
                    }
                }, i);
            }

            Task.WaitAll(taskArray);
            sw.Stop();

            var data = fileService.GetStoredMessage();

            var lineCount = data?.Split(System.Environment.NewLine).Length;

            Console.WriteLine("Elapsed={0}", sw.Elapsed);
            Assert.IsTrue(lineCount - 1 == dataList.Count);
            Assert.IsTrue(dataList.Select(x => x + System.Environment.NewLine).Aggregate((i, j) => i + j).Equals(data));
        }

        /// <summary>
        ///  онтрольный тест (решение "в лоб"), с замером производительности
        /// </summary>
        [Test]
        public void TestSTP()
        {
            ReaderWriterLockSlim lockStore = new ReaderWriterLockSlim();
            Stopwatch sw = new Stopwatch();
            var fileName = $"{MethodBase.GetCurrentMethod().Name}.txt";

            File.Delete(fileName);

            var fileService = new FileService(fileName);

            sw.Start();
            Task[] taskArray = new Task[taskCount];
            for (int i = 0; i < taskArray.Length; i++)
            {
                taskArray[i] = Task.Factory.StartNew((index) =>
                {
                    var diff = int.Parse(index.ToString());
                    for (int j = diff;
                        j < dataList.Count;
                        j += taskCount)
                    {
                        lockStore.EnterWriteLock();

                        try
                        {
                            if (!File.Exists(fileName))
                            {
                                File.Create(fileName).Close();
                            }

                            var lines = File.ReadLines(fileName).ToList();
                            lines.Add(dataList[j]);
                            lines.Sort();
                            File.WriteAllLines(fileName, lines);
                        }
                        finally
                        {
                            lockStore.ExitWriteLock();
                        }
                    }
                }, i);
            }

            Task.WaitAll(taskArray);
            sw.Stop();

            var data = fileService.GetStoredMessage();

            var lineCount = data?.Split(System.Environment.NewLine).Length;

            Console.WriteLine("Elapsed={0}", sw.Elapsed);
            Assert.IsTrue(lineCount - 1 == dataList.Count);
            Assert.IsTrue(dataList.Select(x => x + System.Environment.NewLine).Aggregate((i, j) => i + j).Equals(data));
        }
    }
}