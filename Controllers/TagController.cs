using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dev_Blog.Data;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dev_Blog.Controllers
{
    public class TagController : Controller
    {
        private readonly ILogger<TagController> logger;
        private readonly BlogDBContext context;

        public TagController(BlogDBContext context, ILogger<TagController> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        /// <summary>
        /// Searches for tags using the given term, and returns a Select2 json result
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [Route("/tag/search")]
        public string Search(string term, bool allowNewTags = false)
        {
            logger.LogTrace("GET: Tag, Search, term = {term}, allowNewTags = {allowNewTags}", term, allowNewTags);
            term = term?.ToLower()?.Trim()?.Trim('-') ?? "";
            term = Regex.Replace(term, @"\s", "-");

            TagSearchResults tagSearchResults = new TagSearchResults();
            var results = (from tag in context.Tags
                           where tag.Name.Contains(term)
                           orderby tag.Posts.Count() descending
                           select tag).Take(8);
            foreach (var result in results)
            {
                tagSearchResults.AddResult(result.Name, result.Name);
            }
            //If the tag doesn't exist already, and we're allowed to suggest new tags
            if (allowNewTags && !String.IsNullOrEmpty(term) && User.IsInRole("Author") && !results.Where(t => t.Name == term).Any())
            {
                var group = new TagSearchResults.Result
                {
                    Text = "Add new tag:"
                };
                group.Children.Add(new TagSearchResults.Result()
                {
                    Text = term.ToLower(),
                    Id = term.ToLower()
                });

                tagSearchResults.AddGroup(group);
            }
            return JsonConvert.SerializeObject(tagSearchResults);
        }
    }
}