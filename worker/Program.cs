using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading;
using System.Text.RegularExpressions;

namespace worker
{
    class Program
    {

        private static IConfiguration _configuration;

        public static void Main(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            Console.WriteLine(basePath);
            _configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            Console.WriteLine("Starting...");
            while (true)
            {
                Thread.Sleep(10);
                Work().GetAwaiter().GetResult();
            }
        }

        private static Regex _integerRangeRegex = new Regex(@"(?<from>\d+)\-(?<to>\d+)");

        private static async Task Work()
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
                return;
            }

            // Default visibility timeout of 30 seconds
            string message = queueMessage.AsString;
            Console.WriteLine($"Got message '{message}'.");
            var messageMatch = _integerRangeRegex.Match(message);
            if (!messageMatch.Success)
            {
                Console.WriteLine($"'{message}' is not a valid integer range.");
                return;
            }

            var from = int.Parse(messageMatch.Groups["from"].Value);
            var to = int.Parse(messageMatch.Groups["to"].Value);

            if (from > to)
            {
                Console.WriteLine($"'{message}' is not a valid integer range. First value must be less or equal than second.");
                return;
            }

            await queue.DeleteMessageAsync(queueMessage);

            Parallel.For(from, to + 1, (i) =>
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

        private static Decimal CalculateFactorial(Decimal i)
        {
            if (i > 1)
            {
                return CalculateFactorial(i - 1) * i;
            }
            else
            {
                return 1;
            }
        }

    }
}
