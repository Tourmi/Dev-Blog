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

        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public ICollection<string> Attachments { get; set; }
    }
}
