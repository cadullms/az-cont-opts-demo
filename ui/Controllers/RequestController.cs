using lib.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
namespace ui.Controllers
{
    public class RequestController : Controller
    {
        IConfiguration _configuration;

        public RequestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var request = new RequestMessage { Content = "default message content", Count = 3 };
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Send(RequestMessage req)
        {
            ViewData["Message"] = $"Thanks for sending a message, Count={req.Count} @ {DateTime.UtcNow}";
            for (var i = 1; i <= req.Count; i++)
            {
                var msg = new RequestMessage { Count = req.Count, Content = $"{req.Content} ({i} of {req.Count})" };
                await AddMessageToQueue(msg);
            }

            return View(req);

            async Task AddMessageToQueue(RequestMessage message)
            {
                // committing the Fat Controller Antipattern, however...
                var connString = _configuration.GetValue<string>("queueConnectionString");
                var queueName = _configuration.GetValue<string>("queueName");
                var storageAccount = CloudStorageAccount.Parse(connString);
                var queueClient = storageAccount.CreateCloudQueueClient();
                var queue = queueClient.GetQueueReference(queueName);
                await queue.CreateIfNotExistsAsync();
                var content = JsonConvert.SerializeObject(message);
                var queueMessage = new CloudQueueMessage(content);
                await queue.AddMessageAsync(queueMessage);
            }
        }
    }
}