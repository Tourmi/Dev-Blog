using Dev_Blog.Config;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Services;
using Dev_Blog.Utils;
using Dev_Blog.ViewModels;
using Dev_Blog.ViewModels.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ILogger logger;
        private readonly EmailSchedulerService emailScheduler;
        private readonly BlogDBContext context;
        private readonly IOptions<ReCaptchaConfig> reCaptchaConfig;

        public SubscriptionController(BlogDBContext context, EmailSchedulerService emailScheduler, IOptions<ReCaptchaConfig> reCaptchaConfig, ILogger<SubscriptionController> logger)
        {
            this.context = context;
            this.emailScheduler = emailScheduler;
            this.logger = logger;
            this.reCaptchaConfig = reCaptchaConfig;
        }

        [HttpGet("/subscription/validate")]
        public IActionResult Validate(string email)
        {
            logger.LogTrace("GET: Subscription, ValidateEmail, email = {email}", email);
            ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;

            ViewData["Email"] = email;
            return View();
        }

        [HttpPost("/subscription/validate")]
        public async Task<IActionResult> Validate(string token, string email, string reCaptchaResponse)
        {
            logger.LogTrace("POST: Subscription, ValidateEmail, token = {token}, email = {email}", token, email);
            if (!ReCaptchaValidator.ReCaptchaPassed(reCaptchaConfig.Value.ReCaptchaSecretKey, reCaptchaResponse))
            {
                ModelState.AddModelError("", "The reCAPTCHA was invalid!");
                ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;
                return View();
            }
            return await ValidateEmail(token.Trim());
        }

        /// <summary>
        /// This is to validate an email address from a link.
        /// Should only be used for validation via an email
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("/subscription/validate/{token}")]
        public async Task<IActionResult> ValidateEmail(string token)
        {
            logger.LogTrace("GET: Subscription, ValidateEmail, token = {token}", token);

            var subscriber = context.Subscribers.Where(s => s.ValidationToken == token).SingleOrDefault();
            if (subscriber == null)
            {
                ViewData["Message"] = "Could not validate the email. Is the token correct?";
                return View("Error");
            }
            subscriber.ValidatedEmail = true;

            await context.SaveChangesAsync();

            return View("ValidationSuccess");
        }

        [HttpGet("/subscription")]
        public IActionResult Create()
        {
            logger.LogTrace("GET: Subscription, Create");
            ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;

            SubscriptionViewModel viewModel = new SubscriptionViewModel
            {
                MaximumEmailFrequency = Models.Subscriber.EmailFrequency.Weekly,
                SubscribedToAll = true
            };
            return View(viewModel);
        }

        [HttpPost("/subscription")]
        public async Task<IActionResult> Create(SubscriptionViewModel viewModel)
        {
            logger.LogTrace("POST: Subscription, Create");

            if (!ReCaptchaValidator.ReCaptchaPassed(reCaptchaConfig.Value.ReCaptchaSecretKey, viewModel.ReCaptchaResponse))
            {
                ModelState.AddModelError("", "The reCAPTCHA was invalid!");
            }

            if (string.IsNullOrWhiteSpace(viewModel.Email))
            {
                ModelState.AddModelError("Email", "An email is required!");
            }

            if (!viewModel.SubscribedToAll && !viewModel.Tags.Any())
            {
                ModelState.AddModelError("SubscribedTo", "Please subscribe to at least a tag, or subscribe to everything!");
            }

            if (!ModelState.IsValid)
            {
                ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;
                return View(viewModel);
            }

            Subscriber existing = await context.Subscribers.Where(s => s.Email == viewModel.Email.ToLower()).SingleOrDefaultAsync();

            if (existing != null)
            {
                if (existing.ValidatedEmail)
                {
                    ModelState.AddModelError("Email", "You're already subscribed!");

                    return View(viewModel);
                }

                context.Subscribers.Remove(existing);
                await context.SaveChangesAsync();
            }

            string token = Guid.NewGuid().ToString();

            Subscriber sub = new Subscriber
            {
                ValidationToken = token,
                Email = viewModel.Email.ToLower(),
                MaximumEmailFrequency = viewModel.MaximumEmailFrequency,
                SubscribedToAll = viewModel.SubscribedToAll,
                ValidatedEmail = false,
                LastEmailDate = DateTime.Now
            };

            await context.Subscribers.AddAsync(sub);
            await context.SaveChangesAsync();

            foreach (var tag in viewModel.Tags)
            {
                string lowerTag = tag.ToLower();
                var t = await context.Tags.Where(t => t.Name == lowerTag).FirstOrDefaultAsync();
                if (t == null) continue;
                SubscriberTag subTag = new SubscriberTag()
                {
                    Tag = t
                };

                sub.SubscribedTo.Add(subTag);
            }

            await context.SaveChangesAsync();
            await sendValidationEmail(token, sub.Email);

            return LocalRedirect($"/subscription/validate?email={viewModel.Email}");
        }

        [HttpGet("/subscription/{token}")]
        public async Task<IActionResult> Edit(string token)
        {
            logger.LogTrace("GET: Subscription, Edit, token = {token}", token);

            var sub = await context.Subscribers
                .Where(s => s.ValidationToken == token)
                .Include(s => s.SubscribedTo)
                .SingleOrDefaultAsync();

            ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;

            SubscriptionViewModel viewModel = new SubscriptionViewModel()
            {
                Token = sub.ValidationToken,
                Email = sub.Email,
                SubscribedToAll = sub.SubscribedToAll,
                Tags = sub.SubscribedTo.Select(st => st.TagID).ToArray()
            };

            return View(viewModel);
        }

        [HttpPost("/subscription/{token}")]
        public async Task<IActionResult> Edit(string token, SubscriptionViewModel viewModel)
        {
            logger.LogTrace("POST: Subscription, Edit, token = {token}", token);

            if (!ReCaptchaValidator.ReCaptchaPassed(reCaptchaConfig.Value.ReCaptchaSecretKey, viewModel.ReCaptchaResponse))
            {
                ModelState.AddModelError("", "The reCAPTCHA was invalid!");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var sub = await context.Subscribers.FirstOrDefaultAsync(s => s.ValidationToken == token);

            if (sub == null || sub.Email != viewModel.Email)
            {
                ModelState.AddModelError("", "The email validation token was invalid");
            }

            sub.SubscribedToAll = viewModel.SubscribedToAll;
            sub.MaximumEmailFrequency = viewModel.MaximumEmailFrequency;
            sub.SubscribedTo.Clear();

            foreach (var tag in viewModel.Tags)
            {
                string lowerTag = tag.ToLower();
                var t = await context.Tags.Where(t => t.Name == lowerTag).FirstOrDefaultAsync();
                if (t == null) continue;

                SubscriberTag subTag = new SubscriberTag()
                {
                    Subscriber = sub,
                    Tag = t
                };

                sub.SubscribedTo.Add(subTag);
            }

            await context.SaveChangesAsync();

            return View();
        }

        [HttpGet("/subscription/unsubscribe/{token}")]
        public async Task<IActionResult> Unsubscribe(string token)
        {
            logger.LogTrace("POST: Subscription, Unsubscribe, token = {token}", token);

            var subscriber = await context.Subscribers.Where(s => s.ValidationToken == token).Include(st => st.SubscribedTo).SingleOrDefaultAsync();
            if (subscriber != null)
            {
                context.RemoveRange(subscriber.SubscribedTo);
                context.Remove(subscriber);
            }

            await context.SaveChangesAsync();

            return View();
        }


        private async Task sendValidationEmail(string token, string email)
        {
            ValidateEmailViewModel viewModel = new ValidateEmailViewModel()
            {
                Token = token
            };

            string body = await this.RenderEmailViewAsync("~/Views/Email/ConfirmEmail.cshtml", viewModel, token);
            emailScheduler.EnqueuePriorityEmail(new EmailModel(email, "Confirm your email address!", body, EmailModel.EmailType.ValidateEmail));
        }
    }
}
