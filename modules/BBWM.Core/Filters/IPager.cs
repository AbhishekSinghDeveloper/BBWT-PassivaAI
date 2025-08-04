namespace BBWM.Core.Filters;

public interface IPager
{
    int? Skip { get; set; }
    int? Take { get; set; }
}
