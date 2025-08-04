
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BBWM.Messages;

/// <summary>
/// Wrapping the features of Twilio we want to use in <see cref="MessageSender"/>, it makes testing easier/doable.
/// </summary>
public interface ITwilioWrapper
{
    void Init(string username, string password, string accountSid);
    Task<MessageResource> CreateAsync(PhoneNumber to, PhoneNumber from, string message);
}

public class TwilioWrapper : ITwilioWrapper
{
    public Task<MessageResource> CreateAsync(PhoneNumber to, PhoneNumber from, string message)
        => MessageResource.CreateAsync(to, from: from, body: message);

    public void Init(string username, string password, string accountSid)
        => TwilioClient.Init(username, password, accountSid);
}
