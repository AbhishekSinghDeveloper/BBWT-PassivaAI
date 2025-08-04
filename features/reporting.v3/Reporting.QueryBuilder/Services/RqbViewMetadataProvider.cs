using System.Text.RegularExpressions;
using BBF.Reporting.Core.Model;
using BBF.Reporting.QueryBuilder.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Extensions;
using BBWM.DbDoc.Model;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.QueryBuilder.Services;

public class RqbViewMetadataProvider : IRqbViewMetadataProvider
{
    private readonly IDbContext _context;
    private readonly IRqbQuerySourceProvider _provider;

    public RqbViewMetadataProvider(IDbContext dbContext, IRqbQuerySourceProvider provider)
    {
        _context = dbContext;
        _provider = provider;
    }

    public async Task<ViewMetadata> GetViewMetadata(Guid querySourceId, CancellationToken ct)
    {
        var querySchema = await _provider.GetQuerySchema(querySourceId, ct);

        return new ViewMetadata
        {
            Columns = querySchema.Columns.Select(column => new ViewMetadataColumn
            {
                QueryAlias = column.QueryAlias,
                Title = ParseDbColumnTitlePresentation(column.ColumnName)
            })
        };
    }

    public async Task<IEnumerable<CustomColumnType>> GetCustomColumnTypes(CancellationToken ct = default)
        => await _context.Set<ColumnType>()
            .Include(type => type.ViewMetadata)
            .ThenInclude(metadata => metadata.GridColumnView)
            .Select(type => new CustomColumnType
            {
                Id = type.Id,
                Name = type.Name,
                Mask = type.ViewMetadata.GridColumnView.Mask,
            })
            .ToListAsync(ct);

    private static string ParseDbColumnTitlePresentation(string columnName)
    {
        var regex = new Regex("(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9*]+|[a-z]+)");
        var words = regex.Matches(columnName).Select(match => match.Value);

        // Search for abbreviations and make them uppercase if they appear in the colum name.
        var abbrList = new[] { "id", "ip", "http", "https", "xml", "xls" };
        words = words.Select(word => abbrList.Contains(word.ToLowerInvariant()) ? word.ToUpperInvariant() : word);

        return string.Join(" ", words).ToTitlePhrase();
    }
}