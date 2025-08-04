using BBWM.Menu;
using BBWM.Menu.DTO;

namespace BBWM.Demo.ModuleLinkage;

public class MenuModuleLinkage : IMenuModuleLinkage
{
    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus)
    {
        #region Insert root demo items

        if (rootMenus.Home is not null)
        {
            var rootDemoItems = new List<MenuDTO> {
                new() {
                    Label = "Demo Pages",
                    Icon = "list",
                    Children = new List<MenuDTO> {
                        new() {
                            Label = "Northwind",
                            Icon = "list",
                            Children =
                            new List<MenuDTO> {
                                new(Northwind.Routes.Customers),
                                new(Northwind.Routes.Employees),
                                new(Northwind.Routes.Orders),
                                new(Northwind.Routes.OrderDetails),
                                new(Northwind.Routes.Products),
                            }
                        },

                        new() {
                            Label = "Master - Detail",
                            Icon= "list",
                            Children = new List<MenuDTO> {
                                new(Routes.GridMasterDetail.Page),
                                new(Routes.GridMasterDetail.Inline),
                                new(Routes.GridMasterDetail.Popup),
                            }
                        },
                        new(ComplexData.Routes.ComplexTest, "computer"),
                        new(Routes.GridLocal),
                        new(Routes.GridFilter),
                        new(Routes.IdHashing),
                        new() {
                            Label = "Guidelines",
                            Icon = "book",
                            Children = new List<MenuDTO> {
                                new(Routes.Guidelines.General),
                                new(Routes.Guidelines.Basic),
                                new(Routes.Guidelines.Headings),
                                new(Routes.Guidelines.Lists),
                                new(Routes.Guidelines.Inputs),
                                new(Routes.Guidelines.Buttons),
                                new(Routes.Guidelines.Calendar),
                                new(Routes.Guidelines.Disabled),
                                new(Routes.Guidelines.Links),
                                new(Routes.Guidelines.Search),
                                new(Routes.Guidelines.Tabs),
                                new(Routes.Guidelines.Panels),
                                new(Routes.Guidelines.Dialogs),
                                new(Routes.Guidelines.Grids),
                                new(Routes.Guidelines.Tree),
                                new(Routes.Guidelines.Pdf)
                            }
                        },
                        new(Routes.DisabledControls),
                        new(SimulateError.Routes.SimulateError, "error"),
                        new(Routes.Raygun, "extension"),
                        new(Routes.Culture, "language")
                    }
                },
                new() {
                    Label = "Feature Demo Pages",
                    Icon = "list",
                    Children =  new List<MenuDTO> {
                        new(ReportingV3.Routes.ReportingV3),
                        new(DataImport.Routes.DataImport, "import_export"),
                        new(Routes.Impersonation, "people_outline"),
                        new() {
                            Label = "Access Control",
                            Icon= "security",
                            Children = new List<MenuDTO> {
                                new(Security.Routes.SecurityReadMeFirst),
                                new(Security.Routes.SecurityAnyAuthenticated),
                                new(Security.Routes.SecurityGroups, "group_work"),
                                new(Security.Routes.SecurityGroupA),
                                new(Security.Routes.SecurityGroupB),
                                new(Security.Routes.SecurityNote1),
                                new(Security.Routes.SecurityNote2)
                            }
                        },
                        new(Routes.ImageUploader, "photo_library"),
                        new(S3FileManager.Routes.S3FileManager, "people"),
                        new(OData.Routes.OData, "people"),
                        new(EmbedMSWord.Routes.EmbedMSWord)
                    }
                },
                new() {
                    Label = "Themes",
                    Icon = "palette",
                    Children =
                     new List<MenuDTO> {
                        new() {
                            Label = "Ultima",
                            Icon = "palette",
                            Children =
                             new List<MenuDTO> {
                                new() { Icon = "brush", Label = "Ultima Blue", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Blue Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Blue Grey", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Blue Grey Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Brown", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Brown Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Cyan", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Cyan Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Dark Blue", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Dark Blue Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Dark Green", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Dark Green Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Green", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Green Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Grey", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Grey Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Indigo", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Indigo Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Purple Amber", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Purple Amber Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Purple Cyan", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Purple Cyan Compact", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Teal", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Ultima Teal Compact", CustomHandler = "theme" }
                             }
                        },
                        new() {
                            Label = "Verona",
                            Icon = "palette",
                            Children =
                             new List<MenuDTO> {
                                new() { Icon = "brush", Label = "Verona Beach", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Blue", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Blue Grey", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Celestial", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Cosmic", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Couple", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Dark", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Flow", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Fly", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Green", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Lawrencium", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Nepal", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Purple", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Rose", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Stellar", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Teal", CustomHandler = "theme" },
                                new() { Icon = "brush", Label = "Verona Turquoise", CustomHandler = "theme" }
                             }
                        }
                     }
                }
            };

            menu.AddRange(rootDemoItems);
        }
        #endregion
    }
}
