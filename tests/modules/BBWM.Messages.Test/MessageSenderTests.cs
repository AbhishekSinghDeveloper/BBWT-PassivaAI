using Bogus;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace BBWM.Messages.Test;

public class MessageSenderTests
{
    public MessageSenderTests()
    {
    }

    private static (MessageSender, Mock<ISmtpClientWrapper>, Mock<ITwilioWrapper>) CreateMessageSender()
    {
        var emailSettingFake = new Faker<EmailSettings>();
        emailSettingFake.RuleFor(p => p.SMTP, s => s.Random.AlphaNumeric(7));
        emailSettingFake.RuleFor(p => p.UserName, s => s.Random.AlphaNumeric(7));
        emailSettingFake.RuleFor(p => p.Password, s => s.Random.AlphaNumeric(7));
        emailSettingFake.RuleFor(p => p.FromAddress, s => s.Random.AlphaNumeric(7));
        emailSettingFake.RuleFor(p => p.AdminAddress, s => s.Random.AlphaNumeric(7));
        emailSettingFake.RuleFor(p => p.TestEmailAddress, s => s.Random.AlphaNumeric(7));
        emailSettingFake.RuleFor(p => p.Port, s => 1234);
        emailSettingFake.RuleFor(p => p.TestMode, s => true);
        emailSettingFake.RuleFor(p => p.UseTestAddressForOutgoingEmails, s => s.Random.Bool());
        emailSettingFake.RuleFor(p => p.UseDefaultCredentials, s => s.Random.Bool());
        emailSettingFake.RuleFor(p => p.EnableSsl, s => s.Random.Bool());

        var emailOption = new Mock<IOptionsSnapshot<EmailSettings>>();
        emailOption.Setup(p => p.Value).Returns(emailSettingFake);

        var smsSettings = new Faker<SMSSettings>();
        smsSettings.RuleFor(p => p.ApiKey, s => s.Random.AlphaNumeric(7));
        smsSettings.RuleFor(p => p.AuthToken, s => s.Random.AlphaNumeric(7));
        smsSettings.RuleFor(p => p.AccountSid, s => s.Random.AlphaNumeric(7));
        smsSettings.RuleFor(p => p.ShortCode, s => s.Random.AlphaNumeric(7));

        var smsOption = new Mock<IOptionsSnapshot<SMSSettings>>();
        smsOption.Setup(p => p.Value).Returns(smsSettings);

        var smtpClient = MessageSenderFactory.CreateSmtpClientMock();
        var twilioWrapper = MessageSenderFactory.CreateTwilioWrapperMock();

        return (
            new MessageSender(smtpClient.Object, twilioWrapper.Object, emailOption.Object, smsOption.Object),
            smtpClient,
            twilioWrapper);
    }

    [Fact]
    public async Task Send_Email_Test()
    {
        // Arrange
        var (service, smtpClient, _) = CreateMessageSender();

        var formFile = new Mock<IFormFile>();
        var content = "testFileContest";
        var fileName = "test.pdf";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);

        writer.Write(content);
        writer.Flush();

        ms.Position = 0;

        formFile.Setup(p => p.OpenReadStream()).Returns(ms);
        formFile.Setup(p => p.FileName).Returns(fileName);
        formFile.Setup(p => p.Length).Returns(ms.Length);
        formFile.Setup(p => p.ContentType).Returns("application/pdf");

        List<IFormFile> list = new List<IFormFile>();
        list.Add(formFile.Object);

        string[] items = { "Item1", "Item2", "Item3", "Item4" };

        EmailBrandInfo emailBrandInfo = new EmailBrandInfo() { Body = @"<img.*?\ssrc=""data:(image/\w*);base64,([\S]*?)"".*?>" };

        var body = @"<img.*?\ssrc=""data:(image/\w*);base64,([\S]*?)"".*?>";

        // Act
        await service.SendEmail("testSubject", body, "fromSomeone", list.ToArray(), emailBrandInfo, items);

        // Assert
        smtpClient.Verify();
    }

    [Fact]
    public async Task Send_Sms_Test()
    {
        // Arrange
        var (service, _, twilioWrapper) = CreateMessageSender();

        // Act
        await service.SendSms("0", "testMessage");

        // Assert
        twilioWrapper.Verify();
    }
}
