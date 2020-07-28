using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dev_Blog.Data;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Dev_Blog.Controllers
{
    public class TagController : Controller
    {

        private readonly BlogDBContext context;

        public TagController(BlogDBContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Searches for tags using the given term, and returns a Select2 json result
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public string Search(string term, bool allowNewTags = true)
        {
            term = term?.ToLower()?.Trim()?.Trim('-') ?? "";
            term = Regex.Replace(term, @"\s", "-");

            TagSearchResults tagSearchResults = new TagSearchResults();
            var results = (from tag in context.Tags
                          where tag.Name.Contains(term)
                          select tag).Take(5);
            foreach (var result in results)
            {
                tagSearchResults.AddResult(result.Name, result.Name);
            }
            //If the tag doesn't exist already, and we're allowed to suggest new tags
            if (allowNewTags && !String.IsNullOrEmpty(term) && !results.Where(t => t.Name == term).Any())
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