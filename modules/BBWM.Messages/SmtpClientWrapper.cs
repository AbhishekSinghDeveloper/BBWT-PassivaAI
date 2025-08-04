using MailKit;
using MailKit.Net.Smtp;

using MimeKit;

namespace BBWM.Messages;

/// <summary>
/// Wrapping the features of MailKit's SMTP client we want to use in <see cref="MessageSender"/>,
/// it makes testing easier/doable.
/// </summary>
public interface ISmtpClientWrapper
{
    Task ConnectAsync(string host, int port, bool useSsl, CancellationToken cancellationToken = default);
    Task AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default);
    Task<string> SendAsync(MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress progress = null);
    Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default);
}

public class SmtpClientWrapper : SmtpClient, ISmtpClientWrapper
{ }
