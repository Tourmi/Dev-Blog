using Dev_Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class EmailViewModel
    {
        public string Title { get; set; }
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<Tag> SubscribedTags { get; set; }
    }
}
