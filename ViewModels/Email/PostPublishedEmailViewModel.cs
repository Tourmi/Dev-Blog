using Dev_Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels.Email
{
    public class PostPublishedEmailViewModel
    {
        public string Title { get; set; }
        public IEnumerable<Post> Posts { get; set; }
    }
}
