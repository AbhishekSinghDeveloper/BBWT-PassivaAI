using Microsoft.AspNetCore.Http;

namespace BBWM.Messages;

public interface IEmailSender
{
    Task SendEmail(string subject, string body, string from = null, IFormFile[] attachments = null, EmailBrandInfo brandInfo = null, params string[] to);
}
