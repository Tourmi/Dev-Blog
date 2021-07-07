﻿using System;
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
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> logger;
        private readonly SignInManager<User> signInManager;

        public LoginController(ILogger<LoginController> logger, SignInManager<User> signInManager)
        {
            this.logger = logger;
            this.signInManager = signInManager;
        }

        [HttpGet("/login")]
        public ActionResult LogIn(string returnUrl = null)
        {
            logger.LogTrace("GET: Login, LogIn");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("/login")]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel, string returnUrl = null)
        {
            logger.LogTrace("POST: Login, LogIn");
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(loginViewModel);
            }
            var result = await signInManager.PasswordSignInAsync(loginViewModel.Username, loginViewModel.Password, loginViewModel.RememberMe, true);
            if (result.Succeeded)
            {
                logger.LogInformation("Successful log in for user {Username}", loginViewModel.Username);
                return LocalRedirect(returnUrl);
            }

            loginViewModel.Password = null;

            if (result.IsLockedOut)
            {
                logger.LogWarning("Attempt to log into locked account {Username} with ip {IP}", loginViewModel.Username, HttpContext.Connection.RemoteIpAddress.ToString());
                ModelState.AddModelError("", "Too many attempts, please try again in a few minutes.");
                return View(loginViewModel);
            }

            logger.LogWarning("Invalid login attempt for username {Username} and ip {IP}", loginViewModel.Username, HttpContext.Connection.RemoteIpAddress.ToString());
            ModelState.AddModelError("", "Invalid username or password!");
            return View(loginViewModel);
        }

        [HttpPost("/logout")]
        public async Task<IActionResult> LogOut(string returnUrl = null)
        {
            logger.LogTrace("POST: Login, LogOut");
            returnUrl ??= Url.Content("~/");
            await signInManager.SignOutAsync();

            return LocalRedirect(returnUrl);
        }

    }
}