using BBWM.Demo.Northwind.Model;

namespace BBWM.Demo.SimulateError;

public class SimulateErrorService : ISimulateErrorService
{
    private readonly IDemoDataContext _context;

    public SimulateErrorService(IDemoDataContext context)
    {
        _context = context;
    }

    public void SimulateSQLError()
    {
        var ent = _context.Set<Product>().FirstOrDefault(x => x.Id == 1);
        if (ent is not null) ent.Id = 2;
        _context.SaveChanges();
    }
}
