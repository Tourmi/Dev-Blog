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
using Microsoft.Extensions.Options;
using reCAPTCHA.AspNetCore.Attributes;

namespace Dev_Blog.Controllers
{
    public class CommentController : Controller
    {
        private UserManager<User> userManager;
        private BlogDBContext context;
        private IOptions<ReCaptchaConfig> reCaptchaConfig;

        public CommentController(UserManager<User> userManager, BlogDBContext context, IOptions<ReCaptchaConfig> reCaptchaConfig)
        {
            this.userManager = userManager;
            this.context = context;
            this.reCaptchaConfig = reCaptchaConfig;
        }

        public async Task<ActionResult> Create(long postID, long? commentID)
        {
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

        // POST: Comment/Create
        [HttpPost]
        public async Task<ActionResult> Create(CommentViewModel viewModel)
        {
            if (!ReCaptchaValidator.ReCaptchaPassed(reCaptchaConfig.Value.ReCaptchaSecretKey, viewModel.ReCaptchaResponse))
            {
                ModelState.AddModelError("", "The reCAPTCHA was invalid!");
                return LocalRedirect(Request.GetEncodedUrl());
            }

            Post parentPost = await context.Posts.FindAsync(viewModel.PostID);

            if (parentPost == null || parentPost.Deleted || !parentPost.Published)
            {
                ModelState.AddModelError("", "The post being commented on does not exist.");
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

            User author = null;

            if (User.Identity.IsAuthenticated)
            {
                author = await userManager.GetUserAsync(HttpContext.User);
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            Comment comment = new Comment()
            {
                Post = parentPost,
                ParentComment = parentComment,
                Author = author,
                Content = viewModel.Content,
                Name = author?.DisplayName ?? viewModel.Name,
                Email = author?.Email ?? viewModel.Email,
                IpAddress = HttpContext.Connection.RemoteIpAddress.ToString()
            };

            context.Add(comment);
            await context.SaveChangesAsync();

            return Redirect("/post/" + parentPost.Stub + "#comment-" + comment.ID);
        }

        // POST: Comment/Delete/5
        [HttpPost]
        [Authorize(Roles = "Admin, Author")]
        public ActionResult Delete(long id)
        {
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

            return Redirect("/post/" + comment.Post.Stub);
        }
    }
}