using System;
using System.Collections.Generic;
using System.Text;

namespace CapstoneProject.Infrastructure.Services
{
    using CapstoneProject.Application.Interface.IService;
    using Microsoft.Extensions.Configuration;
    using System.Net;
    using System.Net.Mail;

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var emailSettings = _config.GetSection("EmailSettings");

            var client = new SmtpClient(emailSettings["Host"], int.Parse(emailSettings["Port"]))
            {
                Credentials = new NetworkCredential(emailSettings["Email"], emailSettings["Password"]),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["Email"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true 
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}
