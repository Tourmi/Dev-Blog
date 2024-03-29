﻿using Dev_Blog.Config;
using Dev_Blog.Models;
using Dev_Blog.Utils;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Dev_Blog.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> logger;
        private readonly SignInManager<User> signInManager;
        private readonly IOptions<ReCaptchaConfig> reCaptchaConfig;

        public LoginController(ILogger<LoginController> logger, SignInManager<User> signInManager, IOptions<ReCaptchaConfig> reCaptchaConfig)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.reCaptchaConfig = reCaptchaConfig;
        }

        [HttpGet("/login")]
        public ActionResult LogIn(string returnUrl = null)
        {
            logger.LogTrace("GET: Login, LogIn");
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;
            return View();
        }

        [HttpPost("/login")]
        public async Task<IActionResult> LogIn(LoginViewModel loginViewModel, string returnUrl = null)
        {
            logger.LogTrace("POST: Login, LogIn");
            returnUrl ??= Url.Content("~/");

            if (!ReCaptchaValidator.ReCaptchaPassed(reCaptchaConfig.Value.ReCaptchaSecretKey, loginViewModel.ReCaptchaResponse))
            {
                ModelState.AddModelError("", "The reCAPTCHA was invalid!");
            }

            if (!ModelState.IsValid)
            {
                ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;
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
                logger.LogWarning("Attempt to log into locked account {Username} with ip {IP}", loginViewModel.Username, Request.Headers["x-real-ip"].ToString());
                ModelState.AddModelError("", "Too many attempts, please try again in a few minutes.");
                return View(loginViewModel);
            }

            logger.LogWarning("Invalid login attempt for username {Username} and ip {IP}", loginViewModel.Username, Request.Headers["x-real-ip"].ToString());
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