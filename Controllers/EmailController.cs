using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Utils;
using Dev_Blog.ViewModels.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Controllers
{
    public class EmailController : Controller
    {
        private ILogger<EmailController> logger;
        private BlogDBContext context;

        public EmailController(ILogger<EmailController> logger, BlogDBContext context)
        {
            this.logger = logger;
            this.context = context;
        }

        [HttpGet("/email/renderSubscriber")]
        public async Task<string> RenderSubscriberEmail(string token, string title, string posts)
        {
            logger.LogTrace($"GET: Email, RenderSubscriberEmail, token = {token}, title = {title}, posts = {posts}");

            List<Post> postList = new List<Post>();

            foreach (string postId in posts.Split(","))
            {
                if (long.TryParse(postId, out long id))
                {
                    var post = await context.Posts
                        .Include(p => p.Author)
                        .Include(p => p.Tags)
                        .Where(p => p.ID == id)
                        .FirstOrDefaultAsync();

                    if (post != null && post.Published)
                    {
                        postList.Add(post);
                    }
                }
            }

            PostPublishedEmailViewModel model = new PostPublishedEmailViewModel() { Title = title, Posts = postList };

            return await renderEmail("~/Views/Email/PostPublished.cshtml", model, token);
        }

        private async Task<string> renderEmail<TModel>(string emailView, TModel model, string token)
        {
            return await this.RenderEmailViewAsync(emailView, model, token);
        }
    }
}
