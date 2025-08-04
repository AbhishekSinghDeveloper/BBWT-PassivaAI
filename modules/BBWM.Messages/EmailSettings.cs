namespace BBWM.Messages;

public class EmailSettings
{
    #region Default Credentials

    public string DefaultAccountName { get; } = "defaultaccountname";
    public string DefaultPassword { get; } = "";
    public string DefaultSmtp { get; } = "app.specific.smtp.address";
    public int DefaultPort { get; } = 25;

    #endregion

    /// <summary>
    /// SMTP server address
    /// </summary>
    public string SMTP { get; set; }

    /// <summary>
    /// SMTP port
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Enable SSL
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// Admin Address
    /// </summary>
    public string AdminAddress { get; set; }

    /// <summary>
    /// From Address
    /// </summary>
    public string FromAddress { get; set; }

    /// <summary>
    /// Use Default credentials
    /// </summary>
    public bool UseDefaultCredentials { get; set; }

    /// <summary>
    /// Use Test Address For Outgoing Emails 
    /// </summary>
    public bool UseTestAddressForOutgoingEmails { get; set; }

    /// <summary>
    /// Test Email address
    /// </summary>
    public string TestEmailAddress { get; set; }

    public bool TestMode { get; set; }
}
