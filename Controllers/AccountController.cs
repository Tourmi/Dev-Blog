using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dev_Blog.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private UserManager<User> userManager;
        private BlogDBContext context;

        public AccountController(UserManager<User> userManager, BlogDBContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        [Route("Account")]
        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            User user = await userManager.GetUserAsync(User);
            return View(user);
        }
    }
}