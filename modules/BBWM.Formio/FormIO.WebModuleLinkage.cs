using BBWM.Menu;
using BBWM.Menu.DTO;

namespace BBWM.FormIO;

public class FormIOWebModuleLinkage : IMenuModuleLinkage
{
    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus)
    {
        rootMenus.OperationalAdmin.Children.Add(new MenuDTO
        {
            Label = "Forms",
            Icon = "receipt",
            Children = new List<MenuDTO> {
                new(Routes.FormIOList),
                new(Routes.FormIOInstancesExplorer),
                new(Routes.FormIOBuilder),
                new(Routes.FormIODataExplorer),
                new(Routes.FormIOCategory),
                new(Routes.FormioRequests),
                new(Routes.FormIOSurveyList),
                new(Routes.FormIOSurveyPending),                
                
            }
        });
    }
}
