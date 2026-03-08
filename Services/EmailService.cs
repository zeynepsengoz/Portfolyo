using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Portfolyo.Options;

namespace Portfolyo.Services;

public class EmailService
{
    private readonly SmtpOptions _smtp;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpOptions> smtpOptions, ILogger<EmailService> logger)
    {
        _smtp = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendContactAutoReplyAsync(string? toEmail, string? toName)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            return;

        if (string.IsNullOrWhiteSpace(_smtp.Host)
            || string.IsNullOrWhiteSpace(_smtp.Username)
            || string.IsNullOrWhiteSpace(_smtp.Password)
            || string.IsNullOrWhiteSpace(_smtp.FromEmail))
        {
            _logger.LogWarning("SMTP settings are missing. Auto reply email was skipped.");
            return;
        }

        try
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = _smtp.EnableSsl
            };

            var safeName = string.IsNullOrWhiteSpace(toName) ? "Merhaba" : toName;
            var subject = "Mesaj\u0131n\u0131z Al\u0131nd\u0131 - Te\u015Fekk\u00FCrler";
            var body =
                $"<p>Merhaba {WebUtility.HtmlEncode(safeName)},</p>" +
                "<p>\u0130leti\u015Fim formu \u00FCzerinden g\u00F6nderdi\u011Finiz mesaj\u0131 ald\u0131m. Te\u015Fekk\u00FCr ederim.</p>" +
                "<p>En k\u0131sa s\u00FCrede size d\u00F6n\u00FC\u015F yapaca\u011F\u0131m.</p>" +
                "<p>Sevgiler,<br/>Zeynep \u015Eeng\u00F6z</p>";

            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.FromEmail, _smtp.FromName),
                Subject = subject,
                IsBodyHtml = true,
                Body = body
            };

            mail.To.Add(toEmail);
            await client.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Auto reply email could not be sent to {Email}.", toEmail);
        }
    }
}
