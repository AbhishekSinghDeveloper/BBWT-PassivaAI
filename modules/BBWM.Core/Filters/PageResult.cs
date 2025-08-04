namespace BBWM.Core.Filters;

public class PageResult<TEntity> where TEntity : class
{
    public int Total { get; set; }

    public IEnumerable<TEntity> Items { get; set; }
}
