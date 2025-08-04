using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Model;
using BBWM.GitLab;
using BBWM.Menu.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Globalization;
using System.Text.Json;

namespace BBWM.Menu.JsonGit;

public class JsonGitFooterMenuDataProvider : IFooterMenuDataProvider
{
    private static List<FooterMenuItemDTO> _menu;

    private readonly string _jsonPath;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IGitLabService _gitLabService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _hostingEnvironment;

    private const string LastUpdatedFormat = "dd/MM/yyyy HH:mm:ss";

    public JsonGitFooterMenuDataProvider(
        IWebHostEnvironment hostingEnvironment,
        IGitLabService gitLabService,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IOptionsSnapshot<FooterMenuSettings> options)
    {
        _gitLabService = gitLabService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
        _jsonPath = $"{hostingEnvironment.ContentRootPath}/{options.Value.Path}";
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        InitMenu(_jsonPath, _jsonSerializerOptions);
    }

    public async Task<bool> Exists(int id, CancellationToken cancellationToken = default) =>
        await Task.FromResult(_menu.Exists(x => x.Id == id));

    public async Task<List<FooterMenuItemDTO>> GetAll(CancellationToken cancellationToken = default) =>
        await Task.FromResult(_menu);

    public async Task<FooterMenuItemDTO> Get(int id, CancellationToken cancellationToken = default) =>
        await Task.FromResult(_menu.FirstOrDefault(i => i.Id == id));

    public async Task<FooterMenuItemDTO> GetByLink(string routerLink, CancellationToken cancellationToken = default) =>
        await Task.FromResult(_menu.FirstOrDefault(i => i.RouterLink.Equals(routerLink, StringComparison.OrdinalIgnoreCase)));

    public async Task<IEnumerable<FooterMenuItemDTO>> AddRange(IEnumerable<FooterMenuItemDTO> menu, CancellationToken cancellationToken = default)
    {
        var maxId = _menu.Select(m => m.Id).DefaultIfEmpty().Max();

        var items = menu as FooterMenuItemDTO[] ?? menu.ToArray();
        foreach (var item in items)
        {
            item.Id = ++maxId;
        }
        _menu.AddRange(items);

        await ApplyChanges(cancellationToken);

        return items;
    }

    public async Task<IEnumerable<FooterMenuItemDTO>> UpdateRange(IEnumerable<FooterMenuItemDTO> menu, CancellationToken cancellationToken = default)
    {
        var items = menu as FooterMenuItemDTO[] ?? menu.ToArray();

        foreach (var item in items)
        {
            var index = _menu.FindIndex(i => i.Id == item.Id);
            if (index > -1)
            {
                _menu[index] = item;
            }
        }

        await ApplyChanges(cancellationToken);

        return items;
    }

    public async Task<FooterMenuItemDTO> Save(FooterMenuItemDTO menu, CancellationToken cancellationToken = default)
    {
        if (menu.Id > 0)
        {
            var index = _menu.FindIndex(i => i.Id == menu.Id);
            if (index > -1)
            {
                _menu[index] = menu;
            }
            else
            {
                _menu.Add(menu);
            }
        }
        else
        {
            menu.Id = _menu.Select(m => m.Id).DefaultIfEmpty().Max() + 1;
            _menu.Add(menu);
        }

        await ApplyChanges(cancellationToken);

        return menu;
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        var item = _menu.FirstOrDefault(i => i.Id == id);
        if (item is null)
            throw new ObjectNotExistsException("Menu item doesn't exist.");

        _menu.Remove(item);
        await ApplyChanges(cancellationToken);
    }

    private static void InitMenu(string jsonPath, JsonSerializerOptions jsonSerializerOptions)
    {
        if (_menu is null)
        {
            AwsFooterMenuItemPostDTO footerPost;

            try
            {
                var jsonString = File.ReadAllText(jsonPath);
                footerPost = JsonSerializer.Deserialize<AwsFooterMenuItemPostDTO>(jsonString, jsonSerializerOptions);
            }
            catch
            {
                footerPost = null;
            }

            _menu = (footerPost?.Items ?? new FooterMenuItemDTO[] { }).ToList();
        }
    }

    private async Task ApplyChanges(CancellationToken cancellationToken)
    {
        try
        {
            var awsPostDTO = new AwsFooterMenuItemPostDTO
            {
                LastUpdated = GetUTCFormattedDateTime(),
                Items = _menu.ToArray()
            };

            var jsonString = JsonSerializer.Serialize(awsPostDTO, _jsonSerializerOptions);

            await File.WriteAllTextAsync(_jsonPath, jsonString, cancellationToken);
            if (!_hostingEnvironment.IsDevelopment())
                await SendJsonToGit(jsonString, cancellationToken);
        }
        catch (Exception)
        {
            //TODO: temp. commented to investigate deploy issue
            //throw new ConflictException(ex.Message);
        }
    }

    private async Task SendJsonToGit(string json, CancellationToken cancellationToken = default)
    {
        var email = (await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User))?.Email;
        if (string.IsNullOrEmpty(email))
            throw new ConflictException("User's email address not found.");

        await _gitLabService.Push("data/menu/footer-menu", json, email, cancellationToken);
    }

    private static string GetUTCFormattedDateTime() => DateTime.UtcNow.ToString(LastUpdatedFormat, CultureInfo.InvariantCulture);
}