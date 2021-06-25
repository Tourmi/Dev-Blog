using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Services;
using Dev_Blog.Utils;
using Dev_Blog.ViewModels;
using Dev_Blog.ViewModels.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Controllers
{
    public class AboutController : Controller
    {
        private readonly ILogger<AboutController> logger;
        private readonly EmailSchedulerService emailScheduler;
        private readonly BlogDBContext context;

        public AboutController(ILogger<AboutController> logger, EmailSchedulerService emailScheduler, BlogDBContext context)
        {
            this.logger = logger;
            this.emailScheduler = emailScheduler;
            this.context = context;
        }

        [Route("about")]
        public IActionResult Index()
        {
            logger.LogTrace("GET: About, Index");

            return View();
        }
    }
}
