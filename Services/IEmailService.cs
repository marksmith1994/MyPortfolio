using MyPortfolio.Models;

namespace MyPortfolio.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(ContactModel contactModel);
    }
} 