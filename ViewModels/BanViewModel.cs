using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class BanViewModel
    {
        [MaxLength(40)]
        public string IpAddress { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
