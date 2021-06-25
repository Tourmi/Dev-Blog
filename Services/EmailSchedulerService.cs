using Dev_Blog.Controllers;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Utils;
using Dev_Blog.ViewModels;
using Dev_Blog.ViewModels.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dev_Blog.Services
{
    public class EmailSchedulerService : IDisposable
    {
        private readonly ILogger<EmailSchedulerService> logger;
        private readonly IServiceScopeFactory scopeFactory;

        private readonly ConcurrentQueue<EmailModel> emailQueue;
        private readonly ConcurrentQueue<EmailModel> priorityEmailQueue;

        private readonly Timer dequeueEmailTimer;
        private readonly Timer subscriptionTimer;

        public EmailSchedulerService(ILogger<EmailSchedulerService> logger, IServiceScopeFactory scopeFactory)
        {
            this.logger = logger;
            this.scopeFactory = scopeFactory;
            emailQueue = new ConcurrentQueue<EmailModel>();
            priorityEmailQueue = new ConcurrentQueue<EmailModel>();

            dequeueEmailTimer = new Timer(dequeueEmail, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            subscriptionTimer = new Timer(queueSubscriptionEmails, null, TimeSpan.FromSeconds(10), TimeSpan.FromHours(1));
            logger.LogInformation("Email scheduler started");
        }

        public void EnqueuePriorityEmail(EmailModel email)
        {
            priorityEmailQueue.Enqueue(email);
        }

        public void EnqueueEmail(EmailModel email)
        {
            emailQueue.Enqueue(email);
        }

        private async void dequeueEmail(object state)
        {
            if (!priorityEmailQueue.TryDequeue(out var email) && !emailQueue.TryDequeue(out email))
            {
                return;
            }

            using var scope = scopeFactory.CreateScope();
            var emailSender = scope.ServiceProvider.GetRequiredService<EmailSenderService>();

            if (email.Type == EmailModel.EmailType.Subscription)
            {
                using var context = scope.ServiceProvider.GetRequiredService<BlogDBContext>();
                var now = DateTime.Now;

                var sub = await context.Subscribers.Where(s => s.Email == email.To).FirstAsync();
                sub.LastEmailDate = now;
                await context.SaveChangesAsync();
            }

            await emailSender.SendEmailAsync(email);
            logger.LogTrace("Dequeued email of type: {0}", email.Type);
        }

        private async void queueSubscriptionEmails(object state)
        {
            using var scope = scopeFactory.CreateScope();

            using var context = scope.ServiceProvider.GetRequiredService<BlogDBContext>();

            var now = DateTime.Now;

            var subscribers = context.Subscribers
                .Where(s => s.ValidatedEmail)
                .Where(s => s.MaximumEmailFrequency == Subscriber.EmailFrequency.AsSoonAsPossible && s.LastEmailDate < now
                         || s.MaximumEmailFrequency == Subscriber.EmailFrequency.Daily && s.LastEmailDate.AddDays(1) < now
                         || s.MaximumEmailFrequency == Subscriber.EmailFrequency.Weekly && s.LastEmailDate.AddDays(7) < now
                         || s.MaximumEmailFrequency == Subscriber.EmailFrequency.Monthly && s.LastEmailDate.AddDays(30) < now)
                .Include(s => s.SubscribedTo)
                .ToList();


            foreach (var subscriber in subscribers)
            {
                var newPosts = context.Posts
                    .Where(p => p.DatePublished != null && p.DatePublished < now && p.DatePublished > subscriber.LastEmailDate)
                    .OrderByDescending(p => p.Views)
                    .Include(p => p.Tags)
                    .Include(p => p.Author);

                var localNewPosts = newPosts.AsEnumerable()
                    .Where(p => subscriber.SubscribedToAll || p.Tags.Select(t => t.TagID).Intersect(subscriber.SubscribedTo.Select(st => st.TagID)).Any());

                if (!localNewPosts.Any()) continue;

                string title = "New posts since last time!";
                var properties = new Dictionary<string, string>
                {
                    ["posts"] = string.Join(",", localNewPosts.Take(10).Select(p => p.ID.ToString()).ToList()),
                    ["title"] = title
                };

                string body = await ControllerExtensions.RenderEmailViewAsync(scope, "/email/renderSubscriber", properties, subscriber.ValidationToken);

                var emailModel = new EmailModel(subscriber.Email, title, body, EmailModel.EmailType.Subscription);
                EnqueueEmail(emailModel);
            }
        }

        public void Dispose()
        {
            logger.LogInformation("Email scheduler stopped");

            dequeueEmailTimer.Dispose();
            subscriptionTimer.Dispose();
        }
    }
}
