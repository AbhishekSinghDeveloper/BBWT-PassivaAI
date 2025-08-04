namespace BBWM.Core.Audit;

/// <summary>
/// Audit definition
/// </summary>
public class ChangeLogItemDTO
{
    public int Id { get; set; }

    public string PropertyName { get; set; }

    public string OldValue { get; set; }

    public string NewValue { get; set; }
}
