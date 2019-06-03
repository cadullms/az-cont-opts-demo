using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace worker
{
    class Program
    {
        private static IConfiguration _configuration;

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
                Thread.Sleep(10);
                await Work();
            }
        }

        private static Regex _integerRangeRegex = new Regex(@"(?<from>\d+)\-(?<to>\d+)", RegexOptions.Compiled);

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