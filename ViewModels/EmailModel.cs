using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.ViewModels
{
    public class EmailModel
    {
        public EmailModel()
        {
            Attachments = new List<string>();
        }

        public EmailModel(string to, string subject, string body, EmailType type) : this()
        {
            To = to;
            Subject = subject;
            Body = body;
            Type = type;
        }

        public enum EmailType
        {
            None,
            Subscription,
            PasswordReset,
            ValidateEmail
        }

        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public EmailType Type { get; set; }
        public ICollection<string> Attachments { get; set; }
    }
}
