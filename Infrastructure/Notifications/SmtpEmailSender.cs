using Application.Abstractions.Notifications;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Notifications;

/// <summary>使用 MailKit 透過 SMTP 發送郵件。</summary>
public sealed class SmtpEmailSender(
    EmailSettings options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message.To);
        ArgumentException.ThrowIfNullOrWhiteSpace(message.Subject);

        var settings = options;

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(
            string.IsNullOrWhiteSpace(settings.FromName) ? settings.FromEmail : settings.FromName,
            settings.FromEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;

        var bodyPart = message.IsHtml
            ? new TextPart("html") { Text = message.Body }
            : new TextPart("plain") { Text = message.Body };
        mimeMessage.Body = bodyPart;

        using var client = new SmtpClient();

        await client.ConnectAsync(settings.Host, settings.Port, settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
        await client.AuthenticateAsync(settings.FromEmail, settings.Password, ct);
        await client.SendAsync(mimeMessage, ct);
        await client.DisconnectAsync(true, ct);

        logger.LogInformation("已發送郵件至 {To}，主旨：{Subject}", message.To, message.Subject);
    }
}
