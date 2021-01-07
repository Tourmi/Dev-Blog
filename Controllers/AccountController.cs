using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dev_Blog.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> logger;
        private UserManager<User> userManager;
        private BlogDBContext context;

        public AccountController(UserManager<User> userManager, BlogDBContext context, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.context = context;
            this.logger = logger;
        }

        [Route("account")]
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            logger.LogTrace("GET: Account, Index");
            User user = await userManager.GetUserAsync(User);
            return View(user);
        }

        [Route("account/change-password")]
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel viewModel)
        {
            logger.LogTrace("POST: Account, ChangePassword");

            if (viewModel.NewPassword != viewModel.VerifyNewPassword)
            {
                ModelState.AddModelError("VerifyNewPassword", "The verified password wasn't the same as the new password.");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            User user = await userManager.GetUserAsync(User);
            var result = await userManager.ChangePasswordAsync(user, viewModel.OldPassword, viewModel.NewPassword);

            if (!result.Succeeded)
            {
                foreach (IdentityError err in result.Errors)
                {
                    if (err.Code == "PasswordMismatch")
                    {
                        ModelState.AddModelError("OldPassword", "The current password was invalid");
                    } else
                    {
                        ModelState.AddModelError("", err.Description);
                    }
                }
                return View(viewModel);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}