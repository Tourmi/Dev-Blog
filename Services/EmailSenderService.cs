using Dev_Blog.Config;
using Dev_Blog.Data;
using Dev_Blog.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Dev_Blog.Services
{
    public class EmailSenderService
    {
        public EmailSenderConfig Options { get; }

        public EmailSenderService(IOptions<EmailSenderConfig> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public async Task SendEmailAsync(EmailModel model)
        {
            using var smtp = new SmtpClient
            {
                Credentials = new NetworkCredential()
                {
                    UserName = Options.EmailSenderUsername,
                    Password = Options.EmailSenderPassword
                },
                Host = Options.EmailSenderSmtpHost,
                Port = Options.EmailSenderSmtpPort,
                EnableSsl = Options.EmailSenderSmtpEnableSSL,
                Timeout = 100000
            };

            MailMessage message = new MailMessage(new MailAddress(Options.EmailSenderEmail, Options.EmailSenderDisplayName), new MailAddress(model.To))
            {
                Subject = model.Subject,
                Body = model.Body,
                IsBodyHtml = true
            };

            foreach (var filepath in model.Attachments)
            {
                message.Attachments.Add(new Attachment(File.OpenRead(filepath), Path.GetFileName(filepath)));
            }

            await smtp.SendMailAsync(message);
        }
    }
}
