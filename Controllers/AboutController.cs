using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Controllers
{
    public class AboutController : Controller
    {
        private readonly ILogger<AboutController> logger;

        public AboutController(ILogger<AboutController> logger)
        {
            this.logger = logger;
        }

        [Route("about")]
        public IActionResult Index()
        {
            logger.LogTrace("GET: About, Index");
            return View();
        }
    }
}
