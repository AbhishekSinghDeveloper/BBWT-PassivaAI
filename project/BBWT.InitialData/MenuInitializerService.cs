using BBWM.Core;
using BBWM.Core.ModuleLinker;
using BBWM.Menu;
using BBWM.Menu.DTO;

namespace BBWT.InitialData;

public partial class MenuInitializerService : IMenuInitializerService
{
    private readonly IMenuDataProvider _menuDataProvider;
    private readonly IFooterMenuDataProvider _footerMenuDataProvider;

    // Root labels
    private const string LabelOperationalAdmin = "Operational Admin";
    private const string LabelSecurityAdmin = "Security Admin";
    private const string LabelTechnicalAdmin = "Technical Admin";

    public MenuInitializerService(
        IMenuDataProvider menuDataProvider,
        IFooterMenuDataProvider footerMenuDataProvider)
    {
        _menuDataProvider = menuDataProvider;
        _footerMenuDataProvider = footerMenuDataProvider;
    }

    public async Task EnsureInitialData()
    {
        // seed Menu
        var menuItems = await _menuDataProvider.GetAll();
        if (!menuItems.Any())
        {
            var menu = GenerateMenuTree();

            #region Link modules menus
            var rootMenus = new MenuLinkageRootMenus
            {
                Home = menu.FirstOrDefault(o => o.Label == CoreRoutes.Home.Title),
                OperationalAdmin = menu.FirstOrDefault(o => o.Label == LabelOperationalAdmin),
                SecurityAdmin = menu.FirstOrDefault(o => o.Label == LabelSecurityAdmin),
                TechnicalAdmin = menu.FirstOrDefault(o => o.Label == LabelTechnicalAdmin)
            };

            var linkers = ModuleLinker.GetInstances<IMenuModuleLinkage>();
            linkers.ForEach(o => o.CreateInitialMenuItems(menu, rootMenus));
            #endregion

            SetMenuIndexes(menu);

            await _menuDataProvider.AddRange(menu);
        }

        // seed Footer Menu Items
        var footerMenuItems = await _footerMenuDataProvider.GetAll();
        if (!footerMenuItems.Any())
        {
            await _footerMenuDataProvider.AddRange(GenerateFooterMenuItems());
        }
    }

    private void SetMenuIndexes(ICollection<MenuDTO> menu)
    {
        var i = 0;
        foreach (var o in menu)
        {
            o.Index = i++;
            SetMenuIndexes(o.Children);
        }
    }

    /// <summary>
    /// This is the core structure of the main menu with main root nodes. All other default
    ///  menu items are collected from modules (BBWM.*) by the module linker. The linker searches for
    ///  IMenuModuleLinkage interface-based classes.
    /// </summary>
    /// <returns>List of menu items (menu tree).</returns>
    public static List<MenuDTO> GenerateMenuTree()
    {
        var menu = new List<MenuDTO> {
                new MenuDTO(CoreRoutes.Home, "home"),
                new MenuDTO
                {
                    Label = LabelOperationalAdmin,
                    Icon = "assignment_ind",
                    Children = new List<MenuDTO> {
                        new MenuDTO(BBWM.SystemSettings.Routes.SystemConfiguration, "settings"),
                        new MenuDTO(BBWM.Core.Membership.Routes.Users, "person"),
                        new MenuDTO(BBWM.Core.Membership.Routes.Roles, "card_membership"),
                        new MenuDTO(BBWM.Core.Membership.Routes.Organizations, "business")
                    }
                },
                new MenuDTO
                {
                    Label = LabelSecurityAdmin,
                    Icon = "verified_user",
                    Children = new List<MenuDTO>{
                        new MenuDTO(BBWM.Core.Membership.Routes.RoutesAccess, "lock"),
                        new MenuDTO(BBWM.Core.Membership.Routes.LoginAudit, "people")
                    }
                },
                new MenuDTO
                {
                    Label = LabelTechnicalAdmin,
                    Icon = "build",
                    Children = new List<MenuDTO> { }
                }
            };

        return menu;
    }

    public static FooterMenuItemDTO[] GenerateFooterMenuItems()
    {
        return new[]
        {
                new FooterMenuItemDTO { Name = "Terms & Conditions", RouterLink = "/app/static/terms-and-conditions", OrderNo = 1 },
                new FooterMenuItemDTO { Name = "Privacy Policy", RouterLink = "/app/static/privacy-policy", OrderNo = 2 },
                new FooterMenuItemDTO { Name = "Contact Us", RouterLink = "/app/static/contact-us", OrderNo = 3 },
                new FooterMenuItemDTO { Name = "Report a Problem", RouterLink = "/app/report-problem", OrderNo = 4 },
            };
    }
}
