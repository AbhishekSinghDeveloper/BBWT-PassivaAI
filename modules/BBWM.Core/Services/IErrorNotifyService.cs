namespace BBWM.Core.Services;

public interface IErrorNotifyService
{
    Task NotifyOnException(Exception exception);
}
