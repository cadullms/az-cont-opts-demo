using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using ui.Models;
namespace ui.Controllers
{
    public class RequestController : Controller
    {
        private IConfiguration _configuration;
        //private IConnectionMultiplexer _connectionMultiplexer;

        public RequestController(IConfiguration configuration
            //, IConnectionMultiplexer multiplexer
            )
        {
            _configuration = configuration;
            //_connectionMultiplexer = multiplexer;
        }

        public IActionResult Index()
        {
            var request = new RequestModel { RequestMessage = "default message", Count = 3 };
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Send(RequestModel req)
        {
            //var redis = _connectionMultiplexer.GetDatabase();

            req.RequestMessage = $"Thanks for sending a request @ {DateTime.UtcNow}";
            ViewData["Message"] = req.RequestMessage;
            for (var i = 1; i <= req.Count; i++)
            {
                var msg = $"{req.RequestMessage} ({i} of {req.Count})";
                await AddMessageToQueue(msg);
                //await redis.StringSetAsync("RequestMessage.Key", msg);
            }

            return View(req);
        }

        private async Task AddMessageToQueue(string message)
        {
            // committing the Fat Controller Antipattern, however...
            var connString = _configuration.GetValue<string>("queueConnectionString");
            var queueName = _configuration.GetValue<string>("queueName");
            var storageAccount = CloudStorageAccount.Parse(connString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();
            var queueMessage = new CloudQueueMessage(message);
            await queue.AddMessageAsync(queueMessage);
        }
    }
}