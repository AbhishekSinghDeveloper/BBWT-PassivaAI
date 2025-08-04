namespace BBWM.AggregatedLogs;

internal class ClientException : Exception
{
    public string ClientStackTrace { get; private set; }

    public ClientException(string message, string stackTrace) : base(message)
    {
        ClientStackTrace = stackTrace;
    }

    public override string ToString() => $"{Message} {ClientStackTrace}";
}
