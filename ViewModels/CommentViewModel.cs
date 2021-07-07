using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class CommentViewModel
    {
        public CommentViewModel()
        {
        }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        [RegularExpression(@"^[-_a-zA-Z0-9]+$", ErrorMessage = "The name should only contain alphanumeric characters, dashes and underscores.")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [HiddenInput]
        public string ReCaptchaResponse { get; set; }

        [Required]
        [MaxLength(5000)]
        public string Content { get; set; }

        [Required]
        [HiddenInput]
        public long PostID { get; set; }

        [HiddenInput]
        public long? CommentID { get; set; }
    }
}
