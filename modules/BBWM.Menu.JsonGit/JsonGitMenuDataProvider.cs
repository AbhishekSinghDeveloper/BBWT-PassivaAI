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

public class JsonGitMenuDataProvider : IMenuDataProvider
{
    private static List<MenuDTO> _menu = null;

    private readonly string _jsonPath;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IGitLabService _gitLabService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _hostingEnvironment;

    private const string LastUpdatedFormat = "dd/MM/yyyy HH:mm:ss";

    public JsonGitMenuDataProvider(
        IWebHostEnvironment hostingEnvironment,
        IGitLabService gitLabService,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IOptionsSnapshot<MenuSettings> options)
    {
        _gitLabService = gitLabService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
        _jsonPath = $"{hostingEnvironment.ContentRootPath}/{options.Value.Path}";
        _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        InitMenu(_jsonPath, _jsonSerializerOptions);
    }

    public async Task<List<MenuDTO>> GetAll(CancellationToken cancellationToken = default)
    {
        return GetWithChilds(_menu).ToList();
    }

    public async Task<bool> UpdateRange(IEnumerable<MenuDTO> menu, CancellationToken cancellationToken = default)
    {
        var menuWithChilds = GetWithChilds(_menu).ToList();
        foreach (var item in menu)
        {
            var oldItem = menuWithChilds.FirstOrDefault(i => i.Id == item.Id);
            if (oldItem is not null)
            {
                if (oldItem.ParentId > 0)
                {
                    DeleteFromParent(oldItem.Id, oldItem.ParentId);
                }
                else
                {
                    _menu.Remove(oldItem);
                }
            }

            CheckChilds(item);
            if (item.ParentId > 0)
            {
                AppendToNewParent(item);
            }
            else
            {
                _menu.Add(item);
            }
        }

        return await ApplyChanges(cancellationToken);
    }

    public async Task<int> Create(MenuDTO item, CancellationToken cancellationToken = default)
    {
        SetMenuId(item, GetMaxId());
        CheckChilds(item);
        if (item.ParentId > 0)
        {
            AppendToNewParent(item);
        }
        else
        {
            _menu.Add(item);
        }

        if (await ApplyChanges(cancellationToken))
        {
            return item.Id;
        }

        return 0;
    }

    public async Task AddRange(IEnumerable<MenuDTO> items, CancellationToken cancellationToken = default)
    {
        var maxId = GetMaxId();
        foreach (var item in items)
        {
            maxId = SetMenuId(item, maxId);
            CheckChilds(item);
            if (item.ParentId > 0)
            {
                AppendToNewParent(item);
            }
            else
            {
                _menu.Add(item);
            }
        }

        await ApplyChanges(cancellationToken);
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
    {
        var item = GetWithChilds(_menu).FirstOrDefault(i => i.Id == id);
        if (item is not null)
        {
            if (item.ParentId > 0)
            {
                DeleteFromParent(id, item.ParentId);
            }
            else
            {
                _menu.Remove(item);
            }

            return await ApplyChanges(cancellationToken);
        }

        return false;
    }

    public int GetMaxIndex(int? parentId = null) =>
        GetWithChilds(_menu).Where(x => x.ParentId == parentId).Select(x => x.Index).DefaultIfEmpty(-1).Max();

    private async Task<bool> ApplyChanges(CancellationToken cancellationToken = default)
    {
        try
        {
            var awsPostDTO = new AwsMenuPostDTO
            {
                LastUpdated = GetUTCFormattedDateTime(),
                Menu = SortMenuItemChildren(_menu)
            };

            var jsonString = JsonSerializer.Serialize(awsPostDTO, _jsonSerializerOptions);

            await File.WriteAllTextAsync(_jsonPath, jsonString);
            if (!_hostingEnvironment.IsDevelopment())
                await SendJsonToGit(jsonString, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private List<MenuDTO> SortMenuItemChildren(List<MenuDTO> menulist)
    {
        menulist = menulist.OrderBy(p => p.Index).ToList();

        foreach (var menuItem in menulist)
        {
            if (menuItem.Children.Count > 1)
            {
                menuItem.Children = SortMenuItemChildren(menuItem.Children.ToList());
            }
        }

        return menulist;
    }

    private void CheckChilds(MenuDTO item)
    {
        if (item.Children is not null)
        {
            foreach (var child in item.Children)
            {
                child.ParentId = item.Id;
                CheckChilds(child);
            }
        }
    }

    private void AppendToNewParent(MenuDTO item)
    {
        var newParent = GetWithChilds(_menu).FirstOrDefault(i => i.Id == item.ParentId);
        if (newParent is not null)
        {
            var children = newParent.Children.ToList();
            var newIndex = children.FindIndex(c => c.Id == item.Id);
            if (newIndex > -1)
            {
                children[newIndex] = item;
            }
            else
            {
                children.Add(item);
            }
            newParent.Children = children;
        }
    }

    private void DeleteFromParent(int id, int? parentId)
    {
        var oldParent = GetWithChilds(_menu).FirstOrDefault(i => i.Id == parentId);
        if (oldParent is not null)
        {
            oldParent.Children = oldParent.Children.Where(c => c.Id != id).ToList();
        }
    }

    private static void InitMenu(string jsonPath, JsonSerializerOptions jsonSerializerOptions)
    {
        if (_menu is null)
        {
            AwsMenuPostDTO menuPost;

            try
            {
                var jsonString = File.ReadAllText(jsonPath);
                menuPost = JsonSerializer.Deserialize<AwsMenuPostDTO>(jsonString, jsonSerializerOptions);
            }
            catch
            {
                menuPost = null;
            }

            _menu = (menuPost?.Menu ?? new List<MenuDTO>()).ToList();
        }
    }

    private async Task SendJsonToGit(string json, CancellationToken cancellationToken = default)
    {
        var email = (await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User))?.Email;
        if (string.IsNullOrEmpty(email))
            throw new ConflictException("User's email address not found.");

        await _gitLabService.Push("data/menu/main-menu", json, email, cancellationToken);
    }

    private int SetMenuId(MenuDTO menuItem, int maxId)
    {
        menuItem.Id = ++maxId;
        if (menuItem.Children is not null && menuItem.Children.Any())
        {
            foreach (var child in menuItem.Children)
            {
                maxId = SetMenuId(child, maxId);
            }
        }

        return maxId;
    }

    private int GetMaxId(IEnumerable<MenuDTO> menu = null)
    {
        var maxId = 0;
        var usedMenu = menu ?? _menu;

        foreach (var item in usedMenu)
        {
            var newId = item.Children is not null && item.Children.Any() ?
                GetMaxId(item.Children) :
                item.Id;
            maxId = Math.Max(maxId, newId);
        }

        return maxId;
    }

    private IEnumerable<MenuDTO> GetWithChilds(IEnumerable<MenuDTO> items)
    {
        return items.Concat(items.SelectMany(item => GetWithChilds(item.Children)));
    }

    private static string GetUTCFormattedDateTime() => DateTime.UtcNow.ToString(LastUpdatedFormat, CultureInfo.InvariantCulture);
}