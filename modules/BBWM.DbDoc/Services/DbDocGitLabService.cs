using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Model;
using BBWM.DbDoc.Core.Classes;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using BBWM.GitLab;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWM.DbDoc.Services;

public class DbDocGitLabService : IDbDocGitLabService
{
    private const string GitLabUrl = "data/dbdoc/dbdoc";

    private readonly bool _isDevelopment;
    private readonly IDbContext _context;
    private readonly string _currentUserEmail;
    private readonly IGitLabService _gitlabService;
    private readonly string _jsonPath;

    public DbDocGitLabService(
        IHostEnvironment environment,
        IHttpContextAccessor _accessor,
        IDbContext context,
        UserManager<User> userManager,
        IGitLabService gitLabService,
        IOptions<DbDocSettings> dbDocSettings)
    {
        _context = context;
        _gitlabService = gitLabService;

        _isDevelopment = environment.IsDevelopment();
        _jsonPath = Path.Join(environment.ContentRootPath, dbDocSettings.Value.FilePath);
        _currentUserEmail = _accessor.HttpContext?.User == null ? string.Empty : userManager.GetUserAsync(_accessor.HttpContext.User)?.Result.Email;
    }

    public async Task SendCurrentDbDocStateToGit(CancellationToken ct = default) => await SendCurrentDbDocStateToGit(false, ct);

    public async Task SendCurrentDbDocStateToGit(bool isIninialization = false, CancellationToken ct = default)
    {
        if (_isDevelopment) return;

        if (!isIninialization && string.IsNullOrEmpty(_currentUserEmail))
            throw new ConflictException("User's email address not found.");

        var oldJsonFileContent = GetJsonFileContent();
        var newContent = JsonSerializer.Serialize(new DbDocJsonStructure
        {
            ColumnTypes = await _context.Set<ColumnType>()
                    .Include(x => x.ValidationMetadata)
                    .Include(x => x.ViewMetadata)
                    .ThenInclude(x => x.GridColumnView)
                    .ToListAsync(ct),
            Folders = await _context.Set<Folder>()
                    .Include(x => x.Tables)
                    .ThenInclude(x => x.Columns)
                    .ThenInclude(x => x.ColumnType)
                    .ThenInclude(x => x.ValidationMetadata)
                    .Include(x => x.Tables)
                    .ThenInclude(x => x.Columns)
                    .ThenInclude(x => x.ColumnType)
                    .ThenInclude(x => x.ViewMetadata)
                    .ThenInclude(x => x.GridColumnView)
                    .Include(x => x.Tables)
                    .ThenInclude(x => x.Columns)
                    .ThenInclude(x => x.ValidationMetadata)
                    .Include(x => x.Tables)
                    .ThenInclude(x => x.Columns)
                    .ThenInclude(x => x.ViewMetadata)
                    .ThenInclude(x => x.GridColumnView)
                    .ToListAsync(ct)
        }, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        });

        if (newContent != oldJsonFileContent)
        {
            await _gitlabService.Push(
                GitLabUrl,
                newContent,
                isIninialization ? "DbDocInitialization" : _currentUserEmail,
                ct);
        }
    }

    private string GetJsonFileContent() => File.Exists(_jsonPath) ? File.ReadAllText(_jsonPath) : string.Empty;
}