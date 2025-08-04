using BBWM.Core.Data;

namespace BBWT.Data;

/// <summary>
/// An interface of the main DB context of the project.
/// 
/// Approach 1: This interface can be used to explicitely define models DB sets in IDataContext
/// E.g. <code>DbSet&lt;Invoice&gt; Invoices {get; set;}</code>
/// and in the <see cref="DataContextBase"/> class. Therefore Entity Framework will register the defined DB sets automatically
/// (in particular, to update the database via migrations). Then in the business services the DB sets can be called:
/// <code>_dataContext.Invoices.Select(...)</code>.
/// 
/// Approach 2: This interface can be left empty and then DB models can be registered by Entity Framework in
/// DataContextBase.OnModelCreating() method by simply adding <code>builder.Entity&lt;Invoice&gt;();</code> code line.
/// Then in the business services the DB sets can be called:
/// <code>_dataContext.Set&lt;Invoice&gt;().Select(...)</code>.
/// </summary>
public interface IDataContext : IDbContext
{
    // DbSet<Invoice> Invoices {get; set}
}
