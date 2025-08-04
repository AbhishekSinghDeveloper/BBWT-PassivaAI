using BBWM.Core.Data;
using BBWM.Core.ModuleLinker;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.StaticPages;

public class DataModuleLinkage : IInitialDataModuleLinkage
{
    private const string lipsumPlaceholderText =
        "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et" +
        " dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip" +
        " ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore" +
        " eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia" +
        " deserunt mollit anim id est laborum";

    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        if (includingOnceSeededData)
        {
            var context = serviceScope.ServiceProvider.GetService<IDbContext>();
            if (!context.Set<StaticPage>().Any())
            {
                context.Set<StaticPage>().AddRange(GenerateStaticPages());
                await context.SaveChangesAsync();
            }
        }
    }

    private static StaticPage[] GenerateStaticPages()
    {
        var lastUpdated = new DateTime(2018, 10, 16);

        return new[]
        {
                new StaticPage
                {
                    Alias = "terms-and-conditions",
                    Heading = "Terms & Conditions",
                    Contents = System.Web.HttpUtility.HtmlEncode(lipsumPlaceholderText),
                    ContentPreview = "Lorem ipsum dolor si...",
                    LastUpdated = lastUpdated
                },
                new StaticPage
                {
                    Alias = "privacy-policy",
                    Heading = "Privacy Policy",
                    Contents = System.Web.HttpUtility.HtmlEncode(
                        "You are strictly required to keep all information about Blueberry confidential during your" +
                        " employment and afterwards, with the exception of information that has been published" +
                        " publicly by Blueberry. You must only access systems and information, including reports" +
                        " and paper documents to which you are authorised as well as use systems and information" +
                        " only for the purposes for which you have been authorised. You are strictly required to" +
                        " keep your login details and passwords secret and secure at all times. You must not allow" +
                        " anyone else to use your account to gain access to any company system or information. You" +
                        " must not disclose confidential or sensitive information, including personal and corporate" +
                        " data to anyone without the permission of the information owner. You must ensure that" +
                        " sensitive information is protected from view by unauthorised individuals, including your" +
                        " colleagues or coworkers. You must protect information from unauthorised access, disclosure," +
                        " modification, destruction or interference."),
                    ContentPreview = "You are strictly req...",
                    LastUpdated = lastUpdated
                },
                new StaticPage
                {
                    Alias = "contact-us",
                    Heading = "Contact Us",
                    Contents = System.Web.HttpUtility.HtmlEncode(lipsumPlaceholderText),
                    ContentPreview = "Lorem ipsum dolor si...",
                    LastUpdated = lastUpdated
                }
            };
    }
}
