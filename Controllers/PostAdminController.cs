using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dev_Blog.Data;
using Dev_Blog.Models;
using Dev_Blog.Utils;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dev_Blog.Controllers
{
    [Authorize(Roles = "Admin, Author")]
    public class PostAdminController : Controller
    {
        private readonly ILogger<PostAdminController> logger;
        private readonly BlogDBContext context;
        private readonly IWebHostEnvironment env;

        public PostAdminController(ILogger<PostAdminController> logger, IWebHostEnvironment env, BlogDBContext context)
        {
            this.logger = logger;
            this.context = context;
            this.env = env;
        }

        // GET: PostAdmin
        [HttpGet]
        public async Task<ActionResult> Index(int? currPage)
        {
            logger.LogTrace("GET: PostAdmin, Index, currPage = {currPage}", currPage);
            const int postsPerPage = 30;

            IQueryable<Post> posts = context.Posts.OrderByDescending(p => p.DateCreated);

            if (!User.IsInRole("Admin"))
            {
                posts = posts.Where(p => p.Author.UserName == User.Identity.Name && p.DateDeleted == null);
            }

            if (currPage == null || currPage < 1)
            {
                currPage = 1;
            }

            PaginatedData<Post> result = new PaginatedData<Post>()
            {
                PageSize = postsPerPage,
                CurrentPage = currPage.Value,
                Count = await posts.CountAsync()
            };

            result.Data = posts.Skip((currPage.Value - 1) * postsPerPage).Take(postsPerPage);
            return View(result);
        }

        // GET: PostAdmin/Details/5
        [HttpGet]
        public ActionResult Details(long id)
        {
            logger.LogTrace("GET: PostAdmin, Details, id = {id}", id);
            Post post = getAllowedPost(id);

            if (post == null)
            {
                return postNotFound(id);
            }

            return View(post);
        }

        // GET: PostAdmin/Create
        [HttpGet]
        public ActionResult Create()
        {
            logger.LogTrace("GET: PostAdmin, Create");
            PostViewModel viewModel = new PostViewModel();
            return View(viewModel);
        }

        // POST: PostAdmin/Create
        [HttpPost]
        [RequestSizeLimit(Int32.MaxValue)]
        public ActionResult Create(PostViewModel viewModel)
        {
            logger.LogTrace("POST: PostAdmin, Create");
            validatePost(viewModel);

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            Post post = new Post
            {
                Author = context.Users.Where(a => a.Id == User.FindFirst(ClaimTypes.NameIdentifier).Value).First(),
                Content = viewModel.Content,
                Stub = viewModel.Stub,
                DateDeleted = null,
                DateModified = null,
                DatePublished = null,
                Title = viewModel.Title,
                IsAboutPage = false
            };

            post.Tags = Tag.ToPostTags(context, post, viewModel.Tags);
            context.Add(post);
            context.SaveChanges();
            string imagePostDirectory = getImagePostDirectory(post);
            string audioPostDirection = getAudioPostDirectory(post);
            Directory.CreateDirectory(Path.Combine(env.WebRootPath, imagePostDirectory));
            Directory.CreateDirectory(Path.Combine(env.WebRootPath, audioPostDirection));


            if (viewModel.Thumbnail != null)
            {
                setThumbnail(post, viewModel.Thumbnail);
            }
            int refIndex = 1;
            foreach (var file in viewModel.Files)
            {
                addFileAndUpdate(post, file, refIndex);
                refIndex++;
            }

            //Set the publish date to the post's creation date if an older date is specified. Leaves it as null if the post wasn't published yet.
            if (viewModel.Publish)
            {
                if (viewModel.Schedule)
                {
                    //TODO: Create scheduled task for the given date
                    post.DatePublished = viewModel.PublishDate;
                }
                else
                {
                    //TODO: Update subscribers
                    post.DatePublished = post.DateCreated;
                }
            }

            context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: PostAdmin/Edit/5
        [HttpGet]
        public ActionResult Edit(long id)
        {
            logger.LogTrace("GET: PostAdmin, Edit, id = {id}", id);
            Post post = getAllowedPost(id);

            if (post == null)
            {
                return postNotFound(id);
            }

            var viewModel = new PostViewModel()
            {
                Content = post.Content,
                Publish = post.DatePublished != null,
                Schedule = false,
                PublishDate = post.DatePublished < DateTime.Now ? null : post.DatePublished,
                AutoGenerateStub = false,
                Stub = post.Stub,
                Tags = post.Tags.Select(t => t.TagID).ToHashSet(),
                Title = post.Title
            };

            ViewData["FileCount"] = post.Files.Count + 1;

            return View(viewModel);
        }

        // POST: PostAdmin/Edit/5
        [HttpPost]
        [RequestSizeLimit(Int32.MaxValue)]
        public ActionResult Edit(long id, PostViewModel viewModel)
        {
            logger.LogTrace("GET: PostAdmin, Edit, id = {id}", id);
            if (viewModel.ID != id)
            {
                return postNotFound(id);
            }

            Post post = getAllowedPost(id);

            if (post == null)
            {
                return postNotFound(id);
            }

            DateTime now = DateTime.Now;
            try
            {
                validatePost(viewModel);

                if (!ModelState.IsValid)
                {
                    return View(viewModel);
                }

                post.Tags = Tag.ToPostTags(context, post, viewModel.Tags);
                post.DateModified = now;

                post.Content = viewModel.Content;
                post.Title = viewModel.Title;
                if (viewModel.Thumbnail != null)
                {
                    setThumbnail(post, viewModel.Thumbnail);
                }

                int refIndex = post.Files.Count + 1;
                foreach (var file in viewModel.Files)
                {
                    addFileAndUpdate(post, file, refIndex);
                    refIndex++;
                }

                if (viewModel.Publish)
                {
                    if (viewModel.Schedule)
                    {
                        //TODO: Create scheduled task for the given date
                        post.DatePublished = viewModel.PublishDate;
                    }
                    else
                    {
                        //TODO: Send email subscribers
                        post.DatePublished ??= now;
                    }
                }
                else
                {
                    post.DatePublished = null;
                }

                context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(viewModel);
            }
        }

        // GET: PostAdmin/Delete/5
        [HttpGet]
        public ActionResult Delete(long id)
        {
            logger.LogTrace("GET: PostAdmin, Delete, id = {id}", id);
            Post post = getAllowedPost(id);

            if (post == null)
            {
                return postNotFound(id);
            }

            return View(post);
        }

        // POST: PostAdmin/Delete/5
        [HttpPost]
        public ActionResult Delete(long id, Post post)
        {
            logger.LogTrace("POST: PostAdmin, Delete, id = {id}", id);
            if (post.ID != id)
            {
                ModelState.AddModelError("", "Wrong ID");
                return View(post);
            }

            post = getAllowedPost(id);

            if (post == null)
            {
                return postNotFound(id);
            }

            post.DateDeleted = DateTime.Now;
            context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public string PreviewContent(string content)
        {
            return MarkdownParser.ParseString(content);
        }

        /// <summary>
        /// Returns the PostNotFound view with a 404 error code.
        /// </summary>
        /// <param name="id">The id of the post that was tried to be reached</param>
        /// <returns></returns>
        private ActionResult postNotFound(long id)
        {
            Response.StatusCode = 404;
            return View(nameof(postNotFound), id);
        }

        /// <summary>
        /// Returns the specified post if the current user is allowed to access or modify it. Null otherwise
        /// </summary>
        /// <param name="id">The id of the post</param>
        /// <returns>The post if the user is allowed to access it. Null otherwise</returns>
        private Post getAllowedPost(long id)
        {
            Post post = context.Posts
                .Include(p => p.Tags)
                .Include(p => p.Files)
                .Where(p => p.ID == id)
                .FirstOrDefault();

            if (post == null)
            {
                return null;
            }

            if (!User.IsInRole("Admin"))
            {
                if (post.Deleted || post.Author.UserName != User.Identity.Name)
                {
                    return null;
                }
            }

            return post;
        }

        /// <summary>
        /// Validates the given post, if errors are present, updates the ModelState.
        /// Also takes care of automatically trimming fields
        /// </summary>
        /// <param name="viewModel">The post view model to update</param>
        private void validatePost(PostViewModel viewModel)
        {
            viewModel.Title = viewModel.Title?.Trim();
            viewModel.Content = viewModel.Content?.Trim();
            viewModel.Stub = viewModel.Stub?.Trim('-');

            if (context.Posts.Where(p => p.Stub == viewModel.Stub && p.ID != viewModel.ID).Any())
            {
                ModelState.AddModelError(nameof(PostViewModel.Stub), "The stub should be unique");
                viewModel.AutoGenerateStub = false;
            }

            foreach (string tag in viewModel.Tags)
            {
                if (!Regex.IsMatch(tag, @"^[-a-z0-9]+$"))
                {
                    ModelState.AddModelError(nameof(PostViewModel.Tags), "Tags should only contain lowercase alphanumeric characters or dashes");
                    break;
                }
            }

            foreach (var file in viewModel.Files)
            {
                if (!file.ContentType.Contains("image") && !file.ContentType.Contains("audio"))
                {
                    ModelState.AddModelError(nameof(PostViewModel.Files), "One or more of the files were invalid, only images and audio files are supported.");
                    break;
                }
            }

            if (viewModel.Thumbnail != null && !viewModel.Thumbnail.ContentType.Contains("image"))
            {
                ModelState.AddModelError(nameof(PostViewModel.Thumbnail), "The thumbnail can only be an image");
            }

            if (viewModel.PublishDate != null && viewModel.PublishDate < DateTime.Now)
            {
                ModelState.AddModelError(nameof(PostViewModel.PublishDate), "The publishing date needs to be in the future!");
            }
        }

        private void setThumbnail(Post post, IFormFile file)
        {
            string extension = file.FileName.Split(".").Last();

            post.ThumbnailPath = Path.Combine(getImagePostDirectory(post), $"thumbnail.{extension}");
            using var fileStream = new FileStream(Path.Combine(env.WebRootPath, post.ThumbnailPath), FileMode.Create);
            file.CopyTo(fileStream);
        }

        private void addFileAndUpdate(Post post, IFormFile file, int index)
        {
            BlogFile blogFile = new BlogFile()
            {
                HtmlType = file.ContentType,
                Post = post
            };
            //Creates the new path for the blog file, depending on if it's an audio file or an image file
            blogFile.Path = Path.Combine(blogFile.FileType == BlogFile.Type.Image ? getImagePostDirectory(post) : getAudioPostDirectory(post), file.FileName);

            string realPath = Path.Combine(env.WebRootPath, blogFile.Path);
            if (System.IO.File.Exists(realPath))
            {
                int copy = 1;
                while (System.IO.File.Exists(realPath + $"({copy})"))
                {
                    copy++;
                }
                realPath += $"({copy})";
            }

            using var fileStream = new FileStream(realPath, FileMode.CreateNew);
            file.CopyTo(fileStream);

            //Replaces relative paths that were used for previewing files with the new paths
            post.Content = Regex.Replace(post.Content, @$"\[{index}\]: .*", @$"[{index}]: /{blogFile.Path.Replace("\\", "/")}");

            post.Files.Add(blogFile);
        }

        private string getImagePostDirectory(Post post) => Path.Combine("img", "blogpost", post.ID.ToString());
        private string getAudioPostDirectory(Post post) => Path.Combine("audio", "blogpost", post.ID.ToString());
    }
}