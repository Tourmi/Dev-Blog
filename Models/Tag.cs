using Dev_Blog.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class Tag
    {
        public Tag()
        {
            Posts = new HashSet<PostTag>();
            Subscribers = new HashSet<SubscriberTag>();
        }

        private string name;
        [Key]
        [MaxLength(25)]
        [RegularExpression(@"^[-a-z0-9]+$", ErrorMessage = "Tag must have only lowercase alphanumeric characters or dashes")]
        public string Name { get { return name; } set { name = value?.ToLower(); } }

        public virtual ICollection<PostTag> Posts { get; set; }
        public virtual ICollection<SubscriberTag> Subscribers { get; set; }

        /// <summary>
        /// Generates PostTag objects for every tag specified, with the given post.
        /// 
        /// It will use the given context to search for existing tags. 
        /// If a tag doesn't exist, it is created and added to the PostTag object, 
        /// allowing EF to automatically create it on the next save
        /// </summary>
        /// <param name="context">The database context to use</param>
        /// <param name="post">The post to link in the PostTag objects</param>
        /// <param name="tags">The tags for which to create PostTag objects</param>
        /// <returns>A collection of PostTag objects</returns>
        public static ICollection<PostTag> ToPostTags(BlogDBContext context, Post post, ICollection<string> tags)
        {
            var dbTags = from tag in context.Tags
                         where tags.Contains(tag.Name)
                         select tag;
            var postTags = new HashSet<PostTag>();

            foreach (var stringTag in tags)
            {
                Tag tag = dbTags.Where(t => t.Name == stringTag).FirstOrDefault() ?? new Tag() { Name = stringTag };
                postTags.Add(new PostTag() { Post = post, Tag = tag });
            }

            return postTags;
        }
    }
}
