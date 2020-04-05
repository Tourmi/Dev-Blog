using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Models
{
    public class AspUser : IdentityUser
    {
        public long UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

    }
}
