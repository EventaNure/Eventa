using Eventa.Infrastructure.Options;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Eventa.Infrastructure.Services
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly SendGridEmailOptions _options;
        public SendGridEmailSender(IOptions<SendGridEmailOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SendGridClient(_options.Key);

            var from = new EmailAddress(_options.Email, "Eventa");

            var to = new EmailAddress(email, string.Empty);

            var messge = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlMessage);

            await client.SendEmailAsync(messge);
        }
    }
}
