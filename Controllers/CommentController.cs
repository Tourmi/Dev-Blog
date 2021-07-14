using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dev_Blog.Config;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Utils;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using reCAPTCHA.AspNetCore.Attributes;

namespace Dev_Blog.Controllers
{
    public class CommentController : Controller
    {
        private readonly ILogger<CommentController> logger;
        private readonly UserManager<User> userManager;
        private readonly BlogDBContext context;
        private readonly IOptions<ReCaptchaConfig> reCaptchaConfig;

        public CommentController(UserManager<User> userManager, BlogDBContext context, IOptions<ReCaptchaConfig> reCaptchaConfig, ILogger<CommentController> logger)
        {
            this.userManager = userManager;
            this.context = context;
            this.reCaptchaConfig = reCaptchaConfig;
            this.logger = logger;
        }

        [Route("comment/create")]
        [HttpGet]
        public async Task<ActionResult> Create(long postID, long? commentID)
        {
            logger.LogTrace("GET: Comment, Create, postID = {postID}, commentID = {commentID}", postID, commentID);
            var userTask = User.Identity.IsAuthenticated ? userManager.GetUserAsync(HttpContext.User) : null;

            CommentViewModel commentViewModel = new CommentViewModel
            {
                PostID = postID,
                CommentID = commentID
            };

            ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;

            if (userTask != null)
            {
                User user = await userTask;
                commentViewModel.Name = user.DisplayName;
                commentViewModel.Email = user.Email;
            }
            else
            {
                if (HttpContext.Request.Cookies["CommentName"] != null)
                {
                    commentViewModel.Name = HttpContext.Request.Cookies["CommentName"];
                }
                if (HttpContext.Request.Cookies["CommentEmail"] != null)
                {
                    commentViewModel.Email = HttpContext.Request.Cookies["CommentEmail"];
                }
            }

            return PartialView(commentViewModel);
        }

        [Route("comment/create")]
        [HttpPost]
        public async Task<ActionResult> Create(CommentViewModel viewModel)
        {
            logger.LogTrace("POST: Comment, Create");

            Post parentPost = await context.Posts.FindAsync(viewModel.PostID);

            if (parentPost == null || parentPost.Deleted || !parentPost.Published)
            {
                ModelState.AddModelError("", "The post being commented on does not exist.");
            }

            if (!ReCaptchaValidator.ReCaptchaPassed(reCaptchaConfig.Value.ReCaptchaSecretKey, viewModel.ReCaptchaResponse))
            {
                ModelState.AddModelError("", "The reCAPTCHA was invalid!");
            }

            if (context.Bans.Any(b => (b.EmailAddress == viewModel.Email || b.IpAddress == HttpContext.Connection.RemoteIpAddress.ToString()) && (b.EndDate == null || b.EndDate.Value > DateTime.Now)))
            {
                ModelState.AddModelError("", "Impossible to post comments. Your IP address may be blacklisted.");
            }

            Comment parentComment = null;
            if (viewModel.CommentID != null)
            {
                parentComment = await context.Comments.FindAsync(viewModel.CommentID);

                if (parentComment == null || parentComment.Deleted)
                {
                    ModelState.AddModelError("", "The comment being replied to does not exist.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["recaptcha-public-key"] = reCaptchaConfig.Value.ReCaptchaPublicKey;
                return View(viewModel);
            }

            User author = null;
            if (User.Identity.IsAuthenticated)
            {
                author = await userManager.GetUserAsync(HttpContext.User);
            }

            Comment comment = new Comment()
            {
                Post = parentPost,
                ParentComment = parentComment,
                Author = author,
                Content = viewModel.Content,
                Name = author?.DisplayName ?? viewModel.Name,
                Email = author?.Email ?? viewModel.Email,
                IpAddress = Request.Headers["x-forwarded-for"].ToString()
            };
            logger.LogInformation(string.Join("\n", Request.Headers.Select(e => e.Key + "========" + e.Value)));

            context.Add(comment);
            await context.SaveChangesAsync();

            return Redirect(Url.Action("Details", "Post", new { postStub = parentPost.Stub}, null, null, $"comment-{comment.ID}"));
        }

        [Route("comment/delete/{id}")]
        [HttpPost]
        [Authorize(Roles = "Admin, Author")]
        public ActionResult Delete(long id)
        {
            logger.LogTrace("POST: Comment, Delete, id = {id}", id);
            Comment comment = context.Comments.Include(c => c.Post).Where(c => c.ID == id).SingleOrDefault();
            if (comment == null)
            {
                return Redirect("/post/" + comment.Post.Stub);
            }
            if (!User.IsInRole("Admin"))
            {
                if (comment.Post.Author.UserName != User.Identity.Name)
                {
                    return Redirect("/post/" + comment.Post.Stub);
                }
            }
            DateTime now = DateTime.Now;
            comment.DateDeleted = now;
            comment.DateModified = now;

            context.SaveChanges();

            logger.LogInformation("Comment {ID} was deleted by user {User}", comment.ID, User.Identity.Name);
            return Redirect("/post/" + comment.Post.Stub);
        }
    }
}