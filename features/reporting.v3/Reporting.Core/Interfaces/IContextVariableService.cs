namespace BBF.Reporting.Core.Interfaces;

public interface IContextVariableService
{
    string? GetVariableValue(string variableName);

    IEnumerable<string> GetVariableNames();
}