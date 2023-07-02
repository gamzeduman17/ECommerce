using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ECommerce.Email
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(Options.SendGridKey);
            var message = new SendGridMessage()
            {
                From = new EmailAddress("gamzeduman1729@gmail.com", "Gmz"),
                Subject = subject,
                PlainTextContent = htmlMessage,
                HtmlContent = htmlMessage
            };
            message.AddTo(new EmailAddress(email));
            try
            {
                return client.SendEmailAsync(message);
            }
            catch(Exception)
            {
                throw;
            }
            return null;
        }

       public EmailOptions Options { get; set; }
        public EmailSender(IOptions<EmailOptions> emailOptions)
        {
            Options=emailOptions.Value;
        }
    }
}
