using BBWM.Core.Data;
using BBWM.Core.Services;
using BBWM.SystemSettings;

using Microsoft.EntityFrameworkCore;

using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BBWM.Messages.Templates;

public class EmailTemplateService : IEmailTemplateService
{
    private static readonly Regex TagExtractor = new Regex(@"(\$\w+)", RegexOptions.Compiled);
    private static readonly Regex ChildTemplateExtractor = new Regex(@"(\$&lt;(\w+)&gt;)", RegexOptions.Compiled);
    private readonly IDbContext _context;
    private readonly IDataService _dataService;
    private readonly ISettingsService _settingsService;

    public EmailTemplateService(
        IDbContext context,
        IDataService dataService,
        ISettingsService settingsService)
    {
        _dataService = dataService;
        _settingsService = settingsService;
        _context = context;
    }

    public Task<EmailTemplateDTO> Create(EmailTemplateDTO dto, CancellationToken ct)
        => _dataService.Create<EmailTemplate, EmailTemplateDTO>(BeforeSave(dto), ct);

    public Task<EmailTemplateDTO> Update(EmailTemplateDTO dto, CancellationToken ct)
        => _dataService.Update<EmailTemplate, EmailTemplateDTO>(BeforeSave(dto), ct);

    private EmailTemplateDTO BeforeSave(EmailTemplateDTO dto)
    {
        ValidateDto(dto);
        return FixupDto(dto);
    }

    private void ValidateDto(EmailTemplateDTO dto)
    {
        if (!CheckEmailTemplateCode(dto.Code, dto.Id))
        {
            var valResult = new ValidationResult("Duplicated EmailTemplate Code", new[] { nameof(EmailTemplateDTO.Code) });
            throw new ValidationException(valResult, null, dto);
        }
    }

    private static EmailTemplateDTO FixupDto(EmailTemplateDTO dto)
    {
        // Fix Quill setting null body when it's empty
        dto.Body ??= "";
        // Fix issue with Quill Editor wrapping everything in <p> tags, causing duplicate spacing.
        dto.Body = dto.Body.Replace("<p><br></p>", "<br/>")
            .Replace(" target=\"_blank\"", string.Empty);

        // #231111 Replace everything not in specific UTF-8 character set ranges
        dto.Body = Regex.Replace(dto.Body, @"[^\u0020-\u007E]", string.Empty);
        return dto;
    }

    public Task<EmailTemplateDTO> Get(int id, CancellationToken ct = default)
        => _dataService.Get<EmailTemplate, EmailTemplateDTO>(id, ct);

    public Task<EmailTemplateDTO> GetByCode(string code, CancellationToken ct = default)
        => _dataService.Get<EmailTemplate, EmailTemplateDTO>(q => q.Where(o => o.Code == code), ct);

    public void BuildEmail(EmailTemplateDTO template, NameValueCollection tagValues)
    {
        template.From = BuildStringFromTemplate(template.From, tagValues);
        template.Subject = BuildStringFromTemplate(template.Subject, tagValues);
        template.Body = BuildStringFromTemplate(template.Body, tagValues, true);
    }

    public bool CheckEmailTemplateCode(string code, int? id) =>
        _context.Set<EmailTemplate>().All(t => t.Id == id || t.Code != code);

    public string CreateBrand(string logoUrl) =>
        @"<div style=""width: 100%; height: 50px; background-size: cover; background-image: url('" + logoUrl + @"')'""></div>";


    private string BuildStringFromTemplate(string templateString, NameValueCollection tagValues, bool includeChildTemplates = false)
    {
        var result = templateString;
        var tagValuesList = tagValues ?? new NameValueCollection();

        var originTagValues = new NameValueCollection(tagValuesList);

        PrepareSystemTags(tagValuesList);

        var tags = GetTagList(templateString);
        foreach (var tag in tags)
        {
            var tagValue = tagValuesList[tag];
            result = result.Replace(tag, tagValue);
        }

        if (!includeChildTemplates) return result;

        var childTemplateNames = GetChildTemplates(templateString).Distinct().ToArray();
        if (!childTemplateNames.Any()) return result;

        var childTemplates = _context.Set<EmailTemplate>()
            .Where(t => childTemplateNames.Contains(t.Code, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(t => t.Code, t => t.Body);
        if (childTemplates.Count != childTemplateNames.Length) return result;

        foreach (var childTemplate in childTemplates)
        {
            // only one level allowed for now
            result = result.Replace($"$&lt;{childTemplate.Key}&gt;", BuildStringFromTemplate(childTemplate.Value, originTagValues));
        }

        return result;
    }

    private void PrepareSystemTags(NameValueCollection tagValues)
    {
        if (tagValues["$AppName"] is null)
        {
            tagValues["$AppName"] = _settingsService.GetSettingsSection<ProjectSettings>()?.Name ?? "BBWT3";
        }

        if (tagValues["$DateTime"] is null)
        {
            tagValues["$DateTime"] = DateTimeOffset.UtcNow.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.GetCultureInfo("en-GB"));
        }
    }

    private static IEnumerable<string> GetChildTemplates(string source)
    {
        if (string.IsNullOrEmpty(source)) yield break;

        var matches = ChildTemplateExtractor.Matches(source);
        foreach (Match match in matches)
        {
            if (match.Groups.Count == 3)
            {
                yield return match.Groups[2].Value;
            }
        }
    }

    private static IEnumerable<string> GetTagList(string source)
    {
        string tag;
        var tags = new List<string>();

        if (string.IsNullOrEmpty(source)) return tags.ToArray();

        var matches = TagExtractor.Matches(source);
        foreach (Match match in matches)
        {
            if (match.Groups.Count != 2) continue;

            tag = match.Groups[1].Value;
            if (!tags.Contains(tag))
            {
                tags.Add(tag);
            }
        }

        return tags.ToArray();
    }
}
