using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using static Dev_Blog.Models.Subscriber;

namespace Dev_Blog.ViewModels
{
    public class SubscriptionViewModel
    {
        public SubscriptionViewModel()
        {
            Tags = new string[0];
        }

        public string Token { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Subscribe to all posts")]
        public bool SubscribedToAll { get; set; }
        [Required]
        [Display(Name = "Email frequency", Description = "Emails will never be sent more often than the frequency specified, to avoid spam.")]
        public EmailFrequency MaximumEmailFrequency { get; set; }

        [Display(Name = "Subscribe to", Description = "Choose the specific tags you wish to subscribe to.")]
        public string[] Tags { get; set; }
        [Required]
        [HiddenInput]
        public string ReCaptchaResponse { get; set; }
    }
}
