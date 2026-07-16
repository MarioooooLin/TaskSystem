namespace Infrastructure.Notifications;

/// <summary>SMTP 郵件設定。</summary>
public sealed class EmailSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    /// <summary>後台站台對外基底 URL，用於產生邀請信連結。</summary>
    public string BaseUrl { get; set; } = string.Empty;
}
