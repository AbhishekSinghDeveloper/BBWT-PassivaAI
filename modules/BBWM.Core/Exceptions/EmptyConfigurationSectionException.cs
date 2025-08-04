namespace BBWM.Core.Exceptions;

/// <summary>
/// The exception that is thrown when the configuration's section is empty.
/// </summary>
public sealed class EmptyConfigurationSectionException : ConfigurationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:BBWM.Core.Exceptions.EmptyConfigurationSectionException"></see> class with a specified section name.
    /// </summary>
    /// <param name="sectionName">The section name.</param>
    public EmptyConfigurationSectionException(string sectionName)
        : base($"The configuration section '{sectionName}' is empty.") { }
}
