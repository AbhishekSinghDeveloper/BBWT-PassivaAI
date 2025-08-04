namespace BBWM.AWS;

public class AwsCheckPermissionsResult
{
    public bool Success { get; }
    public string Message { get; set; }

    public AwsCheckPermissionsResult(string message, bool success = true)
    {
        Message = message;
        Success = success;
    }
}
