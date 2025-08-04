using BBWM.Menu.DTO;

namespace BBWM.Menu;

public interface IMenuModuleLinkage
{
    void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus);
}

public class MenuLinkageRootMenus
{
    public MenuDTO Home { get; set; }
    public MenuDTO OperationalAdmin { get; set; }
    public MenuDTO SecurityAdmin { get; set; }
    public MenuDTO TechnicalAdmin { get; set; }
}
