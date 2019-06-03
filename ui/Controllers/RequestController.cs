using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Threading.Tasks;
using ui.Models;
namespace ui.Controllers
{
    public class RequestController : Controller
    {
        private IConfiguration _configuration;

        public RequestController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var request = new Request { RequestMessage = "1-100" };
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> Send([Bind("RequestMessage")]Request request)
        {
            ViewData["Message"] = $"Thanks for sending request '{request.RequestMessage}'";
            for (int i = 0; i < 15; i++)
            {
                await AddMessageToQueue(request.RequestMessage);
            }
            
            return View(request);
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