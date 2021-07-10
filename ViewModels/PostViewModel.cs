using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class PostViewModel
    {
        public long ID { get; set; }
        [Required]
        [StringLength(80, MinimumLength = 1)]
        public string Title { get; set; }
        [Required]
        [StringLength(80, MinimumLength = 1)]
        [RegularExpression(@"^[-a-z0-9]+$", ErrorMessage = "The stub should only contain lowercase alphanumerics and dashes")]
        public string Stub { get; set; }
        [DisplayName("Automatically generate stub")]
        public bool AutoGenerateStub { get; set; } = true;
        [Required]
        public string Content { get; set; }
        public bool Publish { get; set; } = true;
        public bool Schedule { get; set; } = false;
        public bool IsAboutPage { get; set; }
        [DisplayName("Publish on")]
        public DateTime? PublishDate { get; set; }
        public ICollection<string> Tags { get; set; } = new HashSet<string>();
        public IFormFile Thumbnail { get; set; }
        public IFormFileCollection Files { get; set; } = new FormFileCollection();
    }
}
