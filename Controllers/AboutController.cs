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
        private readonly BlogDBContext context;

        public AboutController(ILogger<AboutController> logger, BlogDBContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        [Route("about")]
        public IActionResult Index()
        {
            logger.LogTrace("GET: About, Index");

            Post post = context.Posts.Where(p => p.IsAboutPage).Include(p => p.Author).Include(p => p.Tags).FirstOrDefault();

            return View(post);
        }
    }
}
