using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using MimeKit;
using MimeKit.Utils;

using System.Text;
using System.Text.RegularExpressions;

using Twilio.Types;

namespace BBWM.Messages;

public class MessageSender : IEmailSender, ISmsSender
{
    private readonly EmailSettings _settings;
    private readonly ISmtpClientWrapper _client;
    private readonly ITwilioWrapper _twilio;
    private readonly IOptionsSnapshot<SMSSettings> _smsSettings;

    public MessageSender(
        ISmtpClientWrapper client,
        ITwilioWrapper twilio,
        IOptionsSnapshot<EmailSettings> emailSettings,
        IOptionsSnapshot<SMSSettings> smsSettings)
    {
        _settings = emailSettings.Value;
        _client = client;
        _twilio = twilio;
        _smsSettings = smsSettings;
    }

    public async Task SendEmail(string subject, string body, string from = null, IFormFile[] attachments = null, EmailBrandInfo brandInfo = null, params string[] to)
    {
        if (_settings is null) return;

        var emailMessage = BuildEmail(subject, body, from, attachments, brandInfo, to);

        var port = _settings.Port == 0 ? _settings.DefaultPort : _settings.Port;
        await _client.ConnectAsync(_settings.SMTP, port, _settings.EnableSsl);

        if (!string.IsNullOrEmpty(_settings.UserName) && !string.IsNullOrEmpty(_settings.Password))
        {
            await _client.AuthenticateAsync(_settings.UserName, _settings.Password);
        }

        await _client.SendAsync(emailMessage);
        await _client.DisconnectAsync(true);
    }

    private MimeMessage BuildEmail(string subject, string body, string from, IFormFile[] attachments, EmailBrandInfo brandInfo, params string[] to)
    {
        var emailMessage = new MimeMessage();
        var toAddresses = new List<MailboxAddress>();
        var testMessage = new StringBuilder();

        if (_settings.TestMode)
        {
            testMessage.Append("<h5>**TEST MODE**</h5>");
            testMessage.AppendFormat("<div>This email should have gone to {0} but in test mode, it is not sent to the intended recipient.</div><br/>", string.Join(',', to));
            toAddresses.Add(MailboxAddress.Parse(_settings.TestEmailAddress));
        }

        // to addresses
        if (!to.Any() && !toAddresses.Any())
        {
            toAddresses.Add(MailboxAddress.Parse(_settings.AdminAddress));
        }
        else
        {
            toAddresses.AddRange(
                to.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => MailboxAddress.Parse(o.Trim())));
        }

        // from address
        from = string.IsNullOrWhiteSpace(from) ? _settings.FromAddress : from;

        emailMessage.From.Add(MailboxAddress.Parse(from));
        emailMessage.To.AddRange(toAddresses);
        emailMessage.Subject = subject;

        var builder = new BodyBuilder();
        var htmlBody = ConvertBase64Image($"{testMessage} {body}", builder);

        if (brandInfo is not null && !string.IsNullOrEmpty(brandInfo.Body))
        {
            var brandingHtml = ConvertBase64Image(brandInfo.Body, builder);
            htmlBody += brandingHtml;
        }

        builder.HtmlBody = htmlBody;

        #region attachments
        if (attachments is not null)
        {
            foreach (var file in attachments)
            {
                using (var stream = file.OpenReadStream())
                {
                    builder.Attachments.Add(file.FileName, StreamToByteArray(stream), ContentType.Parse(file.ContentType));
                }
            }
        }
        #endregion

        emailMessage.Body = builder.ToMessageBody();
        return emailMessage;
    }

    private static string ConvertBase64Image(string htmlBody, BodyBuilder builder)
    {
        do
        {
            var pattern = @"<img.*?\ssrc=""data:(image/\w*);base64,([\S]*?)"".*?>";
            var regex = new Regex(pattern);

            var matches = regex.Match(htmlBody);

            if (matches.Success)
            {
                foreach (Match match in matches.Captures)
                {
                    var type = match.Groups[1].Value;
                    var fromBase64 = match.Groups[2].Value;
                    var data = Convert.FromBase64String(fromBase64);
                    var stream = new MemoryStream(data);
                    var image = ContentType.TryParse(type, out var contentType)
                        ? MimeEntity.Load(contentType, stream)
                        : MimeEntity.Load(stream);
                    builder.LinkedResources.Add(image);
                    image.ContentId = MimeUtils.GenerateMessageId();

                    fromBase64 = fromBase64.Replace(@"\", @"\\");
                    fromBase64 = fromBase64.Replace("+", @"\+");
                    fromBase64 = fromBase64.Replace("*", @"\*");
                    fromBase64 = fromBase64.Replace("^", @"\^");
                    fromBase64 = fromBase64.Replace(".", @"\.");
                    fromBase64 = fromBase64.Replace("$", @"\$");
                    fromBase64 = fromBase64.Replace("?", @"\?");

                    var replacePattern = $@"src=""data:image/\w*;base64,{fromBase64}""";
                    var replaceRegex = new Regex(replacePattern);

                    htmlBody = replaceRegex.Replace(htmlBody, $@"src=""cid:{image.ContentId}""");
                }
            }
            else
            {
                break;
            }
        }
        while (true);

        return htmlBody;
    }

    public async Task SendSms(string number, string message)
    {
        var settings = _smsSettings.Value;
        _twilio.Init(settings.ApiKey, settings.AuthToken, settings.AccountSid);

        var phoneNumber = number.StartsWith("0") ? "+44" + number.Substring(1) : number;

        var from = new PhoneNumber(settings.ShortCode);
        var to = new PhoneNumber(phoneNumber);

        await _twilio.CreateAsync(to, from, message);
    }

    private static byte[] StreamToByteArray(Stream stream)
    {
        var buffer = new byte[16 * 1024];

        using var ms = new MemoryStream();
        int read;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
    }
}
