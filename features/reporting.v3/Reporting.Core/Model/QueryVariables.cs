using System.Collections.ObjectModel;
using BBF.Reporting.Core.Model.Variables;

namespace BBF.Reporting.Core.Model;

public class QueryVariables
{
    public IEnumerable<EmittedVariable> Variables { get; set; } = new Collection<EmittedVariable>();
}