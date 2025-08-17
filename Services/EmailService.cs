using MyPortfolio.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace MyPortfolio.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _gmailAddress;
        private readonly string _gmailPassword;
        private readonly string _toEmail = "marksmith.ms804@gmail.com";

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _gmailAddress = _configuration["Email:GmailAddress"] ?? string.Empty;
            _gmailPassword = _configuration["Email:GmailPassword"] ?? string.Empty;
        }

        public async Task SendEmailAsync(ContactModel contactModel)
        {
            // Check if Gmail is properly configured
            if (string.IsNullOrEmpty(_gmailAddress) || string.IsNullOrEmpty(_gmailPassword))
            {
                throw new InvalidOperationException("Email service is not properly configured. Please check your Gmail settings in appsettings.json.");
            }

            try
            {
                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_gmailAddress, _gmailPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_gmailAddress, "Portfolio Contact Form"),
                    Subject = $"Portfolio Contact: {contactModel.Subject}",
                    Body = $"Name: {contactModel.Name}\n" +
                           $"Email: {contactModel.Email}\n" +
                           $"Subject: {contactModel.Subject}\n" +
                           $"Message: {contactModel.Message}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(_toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}");
            }
        }
    }
} 