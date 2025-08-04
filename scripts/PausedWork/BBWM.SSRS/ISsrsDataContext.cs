using Microsoft.EntityFrameworkCore;
using BBWM.Core.Data;

namespace BBWM.SSRS
{
    public interface ISsrsDataContext: IDbContext
    {   
        DbSet<Catalog> Catalog { get; set; }
    }
}