using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dev_Blog.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly SignInManager<AspUser> signInManager;

        public AccountController(ILogger<HomeController> logger, SignInManager<AspUser> signInManager)
        {
            this.logger = logger;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public ActionResult LogIn(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }
            var result = await signInManager.PasswordSignInAsync(loginViewModel.Username, loginViewModel.Password, loginViewModel.RememberMe, true);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            loginViewModel.Password = null;

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Too many attempts, please try again in a few minutes.");
                return View(loginViewModel);
            }

            ModelState.AddModelError("", "Invalid username or password!");
            return View(loginViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LogOut(string returnUrl = null)
        {
            await signInManager.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

    }
}