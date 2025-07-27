using MyPortfolio.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace MyPortfolio.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _sendGridApiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _toEmail = "marksmith.ms804@gmail.com";

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _sendGridApiKey = _configuration["SendGrid:ApiKey"] ?? string.Empty;
            _fromEmail = _configuration["SendGrid:FromEmail"] ?? string.Empty;
            _fromName = _configuration["SendGrid:FromName"] ?? "Portfolio Contact Form";
        }

        public async Task SendEmailAsync(ContactModel contactModel)
        {
            // Check if SendGrid is properly configured
            if (string.IsNullOrEmpty(_sendGridApiKey) || string.IsNullOrEmpty(_fromEmail))
            {
                throw new InvalidOperationException("SendGrid is not properly configured. Please check your API key and sender email settings.");
            }

            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(_toEmail, "Mark Smith");
            var subject = $"Portfolio Contact: {contactModel.Subject}";
            
            var plainTextContent = $"Name: {contactModel.Name}\n" +
                                 $"Email: {contactModel.Email}\n" +
                                 $"Subject: {contactModel.Subject}\n" +
                                 $"Message: {contactModel.Message}";
            
            var htmlContent = $"<strong>Name:</strong> {contactModel.Name}<br>" +
                            $"<strong>Email:</strong> {contactModel.Email}<br>" +
                            $"<strong>Subject:</strong> {contactModel.Subject}<br>" +
                            $"<strong>Message:</strong> {contactModel.Message}";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email. Status code: {response.StatusCode}. Response: {responseBody}");
            }
        }
    }
} 