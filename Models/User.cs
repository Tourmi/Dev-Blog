using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class User : IdentityUser
    {
        public User()
        {
            Posts = new HashSet<Post>();
            Comments = new HashSet<Comment>();
        }

        [Required]
        [MinLength(1)]
        [MaxLength(30)]
        [RegularExpression(@"^[-_a-zA-Z0-9]+$", ErrorMessage = "The name should only contain alphanumeric characters, dashes and underscores.")]
        public string DisplayName { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
