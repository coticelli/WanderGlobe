using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace WanderGlobe.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Log the email for now, since this is a development environment
            _logger.LogInformation($"Email sent to: {email}, Subject: {subject}, Message: {htmlMessage}");
            
            // In a real application, you would implement actual email sending logic here
            // using services like SendGrid, Amazon SES, SMTP, etc.
            
            // Return a completed task since we're not actually sending emails
            return Task.CompletedTask;
        }
    }
}
