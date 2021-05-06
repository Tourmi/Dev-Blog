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
    public class EmailSender
    {
        public EmailSenderConfig Options { get; }

        public EmailSender(IOptions<EmailSenderConfig> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage, bool formatLinks = true)
        {
            await SendEmailAsync(new EmailModel { To = email, Subject = subject, Body = htmlMessage }, formatLinks);
        }

        public async Task SendEmailAsync(EmailModel model, bool formatLinks = true)
        {
            if (formatLinks)
            {
                model.Body = model.Body.Replace("href=\"", "href=\"https://tourmi.dev");
                model.Body = model.Body.Replace("src=\"", "src=\"https://tourmi.dev/");
                model.Body = model.Body.Replace("\\", "/");
            }

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
            //MailMessage message = new MailMessage(new MailAddress(Options.EmailSenderEmail, Options.EmailSenderDisplayName), new MailAddress(model.To))
            MailMessage message = new MailMessage(new MailAddress(Options.EmailSenderEmail, Options.EmailSenderDisplayName), new MailAddress("domino_b10@hotmail.com"))
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
