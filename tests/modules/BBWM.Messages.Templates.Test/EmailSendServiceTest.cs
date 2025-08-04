using BBWM.Messages.Test;

using Bogus;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace BBWM.Messages.Templates.Test;

public class EmailSendServiceTest
{
    [Fact]
    public async Task SendEmailTest()
    {
        // Arrange
        var smsSetting = new Mock<IOptionsSnapshot<SMSSettings>>();
        var faker = new Faker();
        var brandInfo = new EmailBrandInfo() { Body = @$"<img src=""{GetTestImage()}"">" };

        IFormFile[] attachments = GetAttachments();

        var emailsTest = GetEmailSettings();
        var emailSettings = new Mock<IOptionsSnapshot<EmailSettings>>();
        emailSettings.Setup(x => x.Value).Returns(emailsTest);

        var smtpClient = MessageSenderFactory.CreateSmtpClientMock();
        var twilioWrapper = MessageSenderFactory.CreateTwilioWrapperMock();

        var service = new MessageSender(smtpClient.Object, twilioWrapper.Object, emailSettings.Object, smsSetting.Object);

        // Act
        await service.SendEmail(faker.Random.Words(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile[]>(), It.IsAny<EmailBrandInfo>());

        // Assert
        smtpClient.Verify();
    }

    [Fact]
    public async void SendEmail_Admin_Test()
    {
        // Arrange
        var smsSetting = new Mock<IOptionsSnapshot<SMSSettings>>();
        var faker = new Faker();

        var emailsTest = GetEmailSettings();
        emailsTest.TestMode = false;
        var emailSettings = new Mock<IOptionsSnapshot<EmailSettings>>();
        emailSettings.Setup(x => x.Value).Returns(emailsTest);

        var smtpClient = MessageSenderFactory.CreateSmtpClientMock();
        var twilioWrapper = MessageSenderFactory.CreateTwilioWrapperMock();

        var service = new MessageSender(smtpClient.Object, twilioWrapper.Object, emailSettings.Object, smsSetting.Object);

        // Act
        await service.SendEmail(faker.Random.Words(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IFormFile[]>(), It.IsAny<EmailBrandInfo>());

        // Assert
        smtpClient.Verify();
    }


    private static EmailSettings GetEmailSettings()
    {
        return new EmailSettings()
        {
            SMTP = "any",
            Port = 8025,
            FromAddress = "noreply-test@test.com",
            AdminAddress = "test@test.test",
            UserName = "username",
            Password = "password",
            EnableSsl = false,
            TestMode = true,
            TestEmailAddress = "test@test.test",
        };
    }

    private static string GetTestImage()
    {
        return "data:image/bmp;base64,Qk2GAQAAAAAAADYAAAAoAAAACQAAAAwAAAABABgAAAAAAFABAAAAAAAAAAAAAAAAAAAAAAAA////////////////////////////////////AP///////////////////////////////////wD///////////////////////////////////8A////////////////////////////////////AP///////////////////////////////////wD///////////////////////////////////8A////////////////////////////////////AP///////////////////////////////////wD///////////////////////////////////8A////////////////////////////////////AP///////////////////////////////////wD///////////////////////////////////8A";
    }

    private static IFormFile[] GetAttachments()
    {
        var fileMock = new Mock<IFormFile>();
        //Setup mock file using a memory stream
        var content = "testFileContent";
        var fileName = "test.pdf";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
        fileMock.Setup(_ => _.FileName).Returns(fileName);
        fileMock.Setup(_ => _.Length).Returns(ms.Length);
        fileMock.Setup(x => x.ContentType).Returns("application/pdf");

        var file = fileMock.Object;

        List<IFormFile> result = new List<IFormFile>();
        result.Add(file);
        return result.ToArray();
    }
}
