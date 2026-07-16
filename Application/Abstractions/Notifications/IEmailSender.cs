namespace Application.Abstractions.Notifications;

/// <summary>郵件發送抽象介面。</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

/// <summary>郵件內容。</summary>
public sealed record EmailMessage(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true);
