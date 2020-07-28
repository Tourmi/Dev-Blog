using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dev_Blog.Config
{
    public class EmailSenderConfig
    {
        public string EmailSenderPassword { get; set; }
        public string EmailSenderUsername { get; set; }
        public string EmailSenderDisplayName { get; set; }
        public string EmailSenderEmail { get; set; }
        public string EmailSenderSmtpHost { get; set; }
        public int EmailSenderSmtpPort { get; set; }
        public bool EmailSenderSmtpEnableSSL { get; set; }
    }
}
