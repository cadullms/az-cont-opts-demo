using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ui.Models;

namespace ui.Controllers
{
    public class RequestController : Controller
    {

        public IActionResult Index()
        {
            var request = new Request { RequestMessage = "1-100" };
            return View(request);
        }

        [HttpPost]
        public IActionResult Send([Bind("RequestMessage")]Request request)
        {
            ViewData["Message"] = $"Thanks for sending request '{request.RequestMessage}'";
            request.RequestMessage = "";
            return View(request);
        }

    }
}
