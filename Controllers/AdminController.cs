using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> logger;
        private readonly BlogDBContext context;

        public AdminController(BlogDBContext context, ILogger<AdminController> logger)
        {
            this.logger = logger;
            this.context = context;
        }

        [Route("admin/ban")]
        [HttpGet]
        public IActionResult Ban()
        {
            logger.LogTrace("GET: Admin, Ban");
            return View(context.Bans);
        }

        [Route("admin/ban")]
        [HttpPost]
        public async Task<IActionResult> Ban(BanViewModel viewModel)
        {
            logger.LogTrace("POST: Admin, Ban");
            Ban ban = new Ban() { IpAddress = viewModel.IpAddress, EmailAddress = viewModel.Email, EndDate = DateTime.Now };

            int banCount = context.Bans.Count(b => b.IpAddress == viewModel.IpAddress || b.EmailAddress == viewModel.Email);
            ban.EndDate = ban.EndDate.Value.AddDays(banCount switch { 0 => 7, 1 => 30, _ => 300 });

            context.Bans.Add(ban);
            await context.SaveChangesAsync();

            foreach (var comment in context.Comments.Where(c => c.Email == ban.EmailAddress || c.IpAddress == ban.IpAddress))
            {
                comment.DateDeleted = DateTime.Now;
            }

            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Ban));
        }

        [Route("admin/unban/{id}")]
        [HttpPost]
        public async Task<IActionResult> Unban(long id)
        {
            logger.LogTrace("POST: Admin, Unban");

            Ban ban = context.Bans.FirstOrDefault(b => b.ID == id);
            ban.EndDate = DateTime.Now;
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Ban));
        }
    }
}
