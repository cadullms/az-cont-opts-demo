using lib.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace worker
{
    class Program
    {
        static IConfiguration _configuration;

        public async static Task Main(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine(basePath);
            _configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine("Starting...");
            while (true)
            {
                Thread.Sleep(2_000);
                await Work();
            }
        }

        static async Task Work()
        {
            var connString = _configuration.GetValue<string>("queueConnectionString");
            var queueName = _configuration.GetValue<string>("queueName");
            var storageAccount = CloudStorageAccount.Parse(connString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            var queueMessage = await queue.GetMessageAsync();
            if (queueMessage == null)
            {
                Console.WriteLine($"queue is empty...");
                return;
            }

            var json = queueMessage.AsString;

            var msg = JsonConvert.DeserializeObject<RequestMessage>(json);

            // Default visibility timeout of 30 seconds
            Console.WriteLine($"Retrieved message id {msg.RequestID}");

            // delete message from queue
            await queue.DeleteMessageAsync(queueMessage);

            // lets do some *slightly* intensive processing
            ProcessMessage(msg);

            void ProcessMessage(RequestMessage message)
            {
                Parallel.For(0, msg.Count + 1, (i) =>
                {
                    try
                    {
                        Console.WriteLine($"Computing {i}!");
                        var result = CalculateFactorial(i);
                        Console.WriteLine($"{i}!={result}.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            }

            decimal CalculateFactorial(decimal i)
            {
                if (i > 1)
                    return CalculateFactorial(i - 1) * i;
                else
                    return 1;
            }
        }
    }
}