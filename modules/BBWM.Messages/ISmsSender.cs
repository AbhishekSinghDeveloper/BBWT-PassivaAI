namespace BBWM.Messages;

public interface ISmsSender
{
    Task SendSms(string number, string message);
}
