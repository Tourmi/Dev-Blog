using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev_Blog.Config;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUglify.Helpers;

namespace Dev_Blog.Controllers
{
    public class PostController : Controller
    {

        private readonly ILogger<PostController> logger;
        private readonly BlogDBContext context;
        private readonly IWebHostEnvironment env;
        private readonly IOptions<ReCaptchaConfig> reCaptchaConfig;

        public PostController(ILogger<PostController> logger, IWebHostEnvironment env, BlogDBContext context, IOptions<ReCaptchaConfig> reCaptchaConfig)
        {
            this.logger = logger;
            this.context = context;
            this.env = env;
            this.reCaptchaConfig = reCaptchaConfig;
        }

        [Route("")]
        [Route("Post")]
        public async Task<IActionResult> Index(string tags, string author, int? currPage)
        {
            logger.LogTrace("GET: Post, Index");
            const int postsPerPage = 20;
            IQueryable<Post> posts = getValidPosts()
                .Include(p => p.Author)
                .Include(p => p.Tags);

            if (!String.IsNullOrWhiteSpace(tags))
            {
                var tagArray = tags.Split(",");

                posts = posts.Where(p => p.Tags.Select(t => t.TagID).Where(t => tagArray.Contains(t)).Any());
            }
            if (!String.IsNullOrWhiteSpace(author))
            {
                posts = posts.Where(p => p.Author.DisplayName.ToLower() == author.ToLower());
            }

            posts = posts.OrderByDescending(p => p.DatePublished);

            if (currPage == null || currPage < 1)
            {
                currPage = 1;
            }

            PaginatedData<Post> result = new PaginatedData<Post>()
            {
                Count = await posts.CountAsync(),
                PageSize = postsPerPage,
                CurrentPage = currPage.Value
            };

            posts = posts.Skip((currPage.Value - 1) * postsPerPage).Take(postsPerPage);

            result.Data = posts;

            return View(result);
        }

        [Route("Post/{postStub}")]
        public async Task<IActionResult> Details(string postStub)
        {
            logger.LogTrace("GET: Post, Details, {stub}", postStub);
            postStub = postStub.ToLower();

            ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;

            Post post = await getValidPosts()
                .Where(p => p.Stub == postStub)
                .Include(p => p.Author)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return postNotFound(postStub);
            }

            await incrementViewCount(post.ID);

            return View(post);
        }

        /// <summary>
        /// Returns the PostNotFound view with a 404 error code.
        /// </summary>
        /// <param name="stub">The stub of the post that was tried to be reached</param>
        /// <returns></returns>
        private ActionResult postNotFound(string stub)
        {
            Response.StatusCode = 404;
            return View(nameof(postNotFound), stub);
        }

        private IQueryable<Post> getValidPosts()
        {
            var posts = context.Posts.Where(p => p.DateDeleted == null && p.DatePublished <= DateTime.Now);

            return posts;
        }

        private async Task incrementViewCount(long postId)
        {
            int tries = 0;
            bool success = false;
            do
            {
                try
                {
                    Post p = await context.Posts.Where(p => p.ID == postId).FirstAsync();
                    p.Views++;
                    await context.SaveChangesAsync();
                    success = true;
                }
                catch
                {
                    tries++;
                    if (tries > 10)
                    {
                        //If we tried too many times, give up on updating the view count
                        return;
                    }
                }
            } while (!success);
        }
    }
}