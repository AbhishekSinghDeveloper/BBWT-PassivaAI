namespace BBWM.Core.Exceptions;

public class InvalidConfigurationSectionException : ConfigurationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:BBWM.Core.Exceptions.InvalidConfigurationSectionException"></see> class with a specified section name.
    /// </summary>
    /// <param name="sectionName">The section name.</param>
    public InvalidConfigurationSectionException(string sectionName)
        : base($"The configuration section '{sectionName}' is invalid.") { }
}
