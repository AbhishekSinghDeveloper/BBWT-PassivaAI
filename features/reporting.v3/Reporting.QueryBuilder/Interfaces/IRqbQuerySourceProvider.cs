using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.QueryBuilder.Model;

namespace BBF.Reporting.QueryBuilder.Interfaces;

public interface IRqbQuerySourceProvider : IQuerySourceProvider
{
    Task<SqlQueryValidateResult> ValidateSqlCode(int? tableSetId, string sqlCode, CancellationToken ct = default);

    Task<SqlQueryValidateResult> ValidateSchemaCompatibility(Guid querySourceId, string oldSqlCode, string newSqlCode,
        CancellationToken ct = default);
}