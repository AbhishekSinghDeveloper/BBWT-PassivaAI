
using MailKit;

using MimeKit;

using Moq;

using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BBWM.Messages.Test;

public static class MessageSenderFactory
{
    public static Mock<ITwilioWrapper> CreateTwilioWrapperMock()
    {
        var twilioWrapper = new Mock<ITwilioWrapper>();

        twilioWrapper.Setup(w => w.Init(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        twilioWrapper
            .Setup(w => w.CreateAsync(It.IsAny<PhoneNumber>(), It.IsAny<PhoneNumber>(), It.IsAny<string>()))
            .Returns(Task.FromResult<MessageResource>(default))
            .Verifiable();

        return twilioWrapper;
    }

    public static Mock<ISmtpClientWrapper> CreateSmtpClientMock()
    {
        var smtpClient = new Mock<ISmtpClientWrapper>();
        smtpClient
            .Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        smtpClient
            .Setup(c => c.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        smtpClient
            .Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        smtpClient
            .Setup(c => c.SendAsync(It.IsAny<MimeMessage>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>()))
            .Returns(Task.FromResult("FINAL FREE-FORM RESPONSE FROM SERVER"))
            .Verifiable();

        smtpClient
            .Setup(c => c.DisconnectAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        return smtpClient;
    }
}
