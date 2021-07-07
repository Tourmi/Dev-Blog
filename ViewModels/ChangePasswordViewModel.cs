using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DisplayName("Current password")]
        public string OldPassword { get;set; }
        [Required]
        [MinLength(8)]
        [DisplayName("New password")]
        public string NewPassword { get; set; }
        [Required]
        [DisplayName("Verify new password")]
        public string VerifyNewPassword { get; set; }
    }
}
