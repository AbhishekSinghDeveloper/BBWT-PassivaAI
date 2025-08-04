using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Widget.Html.DTO;
using BBF.Reporting.Widget.Html.Interfaces;
using BBF.Reporting.Widget.Grid.DTO;
using BBF.Reporting.QueryBuilder.DTO;
using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModuleLinker;
using Microsoft.Extensions.DependencyInjection;
using BBF.Reporting.Widget.Grid.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBF.Reporting.Widget.Chart.Interfaces;
using BBF.Reporting.Widget.Chart.DTO;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using BBF.Reporting.Widget.ControlSet.DTO;
using System.Text.Json.Nodes;
using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Dashboard.DbModel;
using BBF.Reporting.Dashboard.Interfaces;
using BBF.Reporting.Dashboard.DTO;
using BBF.Reporting.Widget.Chart.Enums;
using BBWM.Core.Membership.Interfaces;
using Microsoft.EntityFrameworkCore;
using BBF.Reporting.QueryBuilder.Interfaces;

namespace BBWM.Demo.ReportingV3;

public class ReportingV3DemoModuleLinkage : IRouteRolesModuleLinkage, IInitialDataModuleLinkage
{
    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope)
        => new() { new PageInfoDTO(Routes.ReportingV3, AggregatedRoles.Authenticated) };

    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        // We suppose that reporting demo data only seeded once and then if it's removed by user, it won't
        // be recovered by app on start. There can be another option for user - to recovery it manually from
        // the reporting feature demo page.
        if (!includingOnceSeededData) return;

        #region Seed RB3 demo records

        var context = serviceScope.ServiceProvider.GetService<IDbContext>();
        var userInitializeService = serviceScope.ServiceProvider.GetService<IUserInitializeService>();

        // Seeding reporting demo user
        await userInitializeService.CreateInitialUser(InitialUsers.ReportAdmin, Roles.SystemAdminRole);

        var defOwnerUserId = context.Set<User>()
            .Where(user => user.Email == InitialUsers.ReportAdmin.Email)
            .Select(user => user.Id)
            .First();

        // seeding dashboard
        await SeedDashboard(serviceScope, context, defOwnerUserId);

        // seeding named widgets
        await SeedHtmlWidget(serviceScope, context, defOwnerUserId);
        await SeedControlSetWidget(serviceScope, context, defOwnerUserId);
        await SeedTableWidget(serviceScope, context, defOwnerUserId);
        await SeedChartWidget(serviceScope, context, defOwnerUserId);

        #endregion
    }

    private static async Task SeedHtmlWidget(IServiceScope serviceScope, IDbContext context, string defOwnerUserId)
    {
        const string widgetHtmlCode = "html-Demo-What-is-Page-Load-Time";

        if (await context.Set<WidgetSource>().AllAsync(source => source.Code != widgetHtmlCode))
        {
            var widgetHtmlBuilderService = serviceScope.ServiceProvider.GetService<IWidgetHtmlBuilderService>();

            await widgetHtmlBuilderService.Create(new HtmlDTO
            {
                WidgetSource = new WidgetSourceDTO
                {
                    Code = widgetHtmlCode,
                    Name = "Demo | What is Page Load Time?",
                    Title = "Demo | What is Page Load Time?"
                },
                InnerHtml = "<h2><strong>Overview</strong></h2>" +
                            "<p>Page load time is a web performance metric that directly impacts user engagement " +
                            "and a business’s bottom line. It indicates how long it takes for a page to fully load " +
                            "in the browser after a user clicks a link or makes a request.</p>" +
                            "<p>There are many different factors that affect page load time. The speed at which a " +
                            "page loads depends on the hosting server, amount of bandwidth in transit, and web page " +
                            "design – as well as the number, type, and weight of elements on the page. Other factors " +
                            "include user location, device, and browser type.</p>" +
                            "<h2><strong>How page load time works</strong></h2>" +
                            "<p>The “stopwatch” begins when a user makes a request and ends when the entire content " +
                            "of the page is displayed on the requesting browser. Below is a typical request-response " +
                            "cycle with various steps that contribute to page load time:</p>" +
                            "<ol><li>User enters a URL, submits a form, or clicks on a hyperlink</li>" +
                            "<li>Browser makes a request to the server through the network</li>" +
                            "<li>The request is processed by the web server</li>" +
                            "<li>Web server sends the response back to the browser</li>" +
                            "<li>Browser starts receiving the requested page (known as&nbsp;" +
                            "<a href=\"https://blog.stackpath.com/time-to-first-byte/\" rel=\"noopener noreferrer\" " +
                            "target=\"_blank\" style=\"color: rgb(0, 0, 0);\">time to first byte</a>)</li>" +
                            "<li>Browser parses, loads, and renders the page content</li>" +
                            "<li>The entire requested page becomes available on the browser</li></ol>"
            }, defOwnerUserId);
        }
    }

    private static async Task SeedControlSetWidget(IServiceScope serviceScope, IDbContext context, string defOwnerUserId)
    {
        const string widgetControlSetCode = "control-set-Demo-Loading-Time-Filters";

        if (await context.Set<WidgetSource>().AllAsync(source => source.Code != widgetControlSetCode))
        {
            var widgetControlSetBuilderService =
                serviceScope.ServiceProvider.GetService<IWidgetControlSetBuilderService>();

            await widgetControlSetBuilderService.Create(new ControlSetViewDTO
            {
                WidgetSource = new WidgetSourceDTO
                {
                    Code = widgetControlSetCode,
                    Name = "Demo | Loading Time Filters",
                    Title = "Demo | Loading Time Filters"
                },
                Items =
                {
                    new ControlSetViewItemDTO
                    {
                        DataType = BBF.Reporting.Core.Enums.DataType.Numeric,
                        Name = "Loading Time ID",
                        HintText = "ID bigger than",
                        InputType = BBF.Reporting.Core.Enums.InputType.Number,
                        ValueEmitType = BBF.Reporting.Widget.ControlSet.Enums.ControlValueEmitType.Standalone,
                        VariableName = "loading_time_id",
                        ExtraSettings = JsonNode.Parse("{}")!
                    }
                }
            }, defOwnerUserId);
        }
    }

    private static async Task SeedTableWidget(IServiceScope serviceScope, IDbContext context, string defOwnerUserId)
    {
        const string widgetTableCode = "table-Demo-Page-Loading-Time";

        if (await context.Set<WidgetSource>().AllAsync(source => source.Code != widgetTableCode))
        {
            // seeding widget's query
            var sqlBuilderQueryService = serviceScope.ServiceProvider.GetService<IRqbService>();
            var sqlQueryBuild = await sqlBuilderQueryService.Create(new SqlQueryBuildDTO
            {
                QuerySource = new QuerySourceDTO(),
                SqlCode = "SELECT * FROM LoadingTime\r\n where id > #loading_time_id"
            }, defOwnerUserId);

            // seeding widget
            var widgetGridBuilderService = serviceScope.ServiceProvider.GetService<IWidgetGridBuilderService>();
            await widgetGridBuilderService.Create(new GridViewDTO
            {
                QuerySourceId = sqlQueryBuild.QuerySourceId,
                WidgetSource = new WidgetSourceDTO
                {
                    Code = widgetTableCode,
                    Name = "Demo | Page Loading Time",
                    Title = "Demo | Page Loading Time"
                }
            }, defOwnerUserId);
        }
    }

    private static async Task SeedChartWidget(IServiceScope serviceScope, IDbContext context, string defOwnerUserId)
    {
        const string widgetChartCode = "chart-Demo-Loading-Time-Chart";

        if (await context.Set<WidgetSource>().AllAsync(source => source.Code != widgetChartCode))
        {
            // seeding widget's query
            var sqlBuilderQueryService = serviceScope.ServiceProvider.GetService<IRqbService>();
            var sqlQueryBuild = await sqlBuilderQueryService.Create(new SqlQueryBuildDTO
            {
                QuerySource = new QuerySourceDTO(),
                SqlCode = "SELECT * FROM LoadingTime"
            }, defOwnerUserId);

            // seeding widget
            var widgetChartBuilderService = serviceScope.ServiceProvider.GetService<IWidgetChartBuilderService>();
            var qpf = serviceScope.ServiceProvider.GetService<IQueryProviderFactory>();
            var qp = qpf.GetQueryProvider(sqlQueryBuild.QuerySourceId);
            var qSchema = await qp!.GetQuerySchema(sqlQueryBuild.QuerySourceId, default);

            var columns = new List<ChartBuildColumnDTO>
            {
                new()
                {
                    ColumnPurpose = ColumnPurpose.Series,
                    ChartAlias = "Route",
                },
                new()
                {
                    ColumnPurpose = ColumnPurpose.AxisX,
                    ChartAlias = "DateTime",
                },
                new()
                {
                    ColumnPurpose = ColumnPurpose.AxisY,
                    ChartAlias = "Time",
                },
            };

            columns.ForEach(chart => chart.QueryAlias = qSchema.Columns.FirstOrDefault(column =>
                column.ColumnName.Equals(chart.ChartAlias, StringComparison.OrdinalIgnoreCase))!.QueryAlias);

            await widgetChartBuilderService.Create(new ChartBuildDTO
            {
                QuerySourceId = sqlQueryBuild.QuerySourceId,
                WidgetSource = new WidgetSourceDTO
                {
                    Code = widgetChartCode,
                    Name = "Demo | Loading Time Chart",
                    Title = "Demo | Loading Time Chart"
                },
                Columns = columns,
                ChartSettingsJson = "{\r\n \"type\": \"line\",\r\n \"width\": \"40vw\",\r\n \"height\": \"60vh\" }"
            }, defOwnerUserId);
        }
    }

    private static async Task SeedDashboard(IServiceScope serviceScope, IDbContext context, string defOwnerUserId)
    {
        const string dashboardCode = "Demo-Article";

        if (await context.Set<Dashboard>().AllAsync(dashboard => dashboard.UrlSlug != dashboardCode))
        {
            var dashboardBuilderService = serviceScope.ServiceProvider.GetService<IDashboardBuilderService>();

            var dashboard = await dashboardBuilderService.Create(new DashboardBuildDTO
                {
                    DisplayName = true,
                    Name = "Demo | Article",
                    UrlSlug = dashboardCode,
                },
                defOwnerUserId);

            var contents = new List<string>()
            {
                "<h3><strong>Principle 1: Decentralization</strong></h3>" +
                "<p>One of the main benefits of blockchain technology is its decentralized nature. This allows " +
                "for trustless transactions and eliminates needing a third party to verify transactions. Who can " +
                "benefit from this? For example, businesses that may not have the resources to process transactions " +
                "on their own.</p>" +
                "<p>Separating the network from the miners allows for a more democratized system. This makes it more " +
                "resistant to centralized control and allows for a more egalitarian distribution of wealth.</p>" +
                "<p>It also has other benefits, such as increased resilience to attacks and increased privacy. " +
                "Blockchain decentralization can improve the trustworthiness and understanding of transactions by " +
                "allowing users to control their data.</p>" +
                "<p>Ultimately, the decentralization of the blockchain is one of its key advantages over traditional " +
                "systems. It allows for greater freedom and transparency in transactions, as well as improved security " +
                "and efficiency.</p>",
                "<h3><strong>Principle 2: Immutability</strong></h3>" +
                "<p>The second principle is immutability. This means that all data and transactions on the " +
                "blockchain are permanent and unchangeable. It ensures that information is accurate and secure, " +
                "and it prevents fraudulent activities from taking place. Once a transaction is recorded on the " +
                "blockchain, it cannot be changed or tampered with. By ensuring that all transactions are permanent, " +
                "the blockchain achieves its key goal of trustworthiness and security.</p><p>Immutability also has " +
                "other benefits. It can help to reduce the cost of transactions, as there is no need for third-party " +
                "verification or reconciliation.</p>" +
                "<p>Overall, the immutability of the blockchain allows for greater trustworthiness and transparency " +
                "in transactions, as well as reduced costs and risks.</p>",
                "<h3><strong>Principle 3: Transparency</strong></h3>" +
                "<p>Transparency is another key feature of the blockchain. Allowing users to see all of the information " +
                "associated with a transaction can improve the trustworthiness and understanding of transactions.</p>" +
                "<p>For example, let’s say you are a business owner and want to sell your product online. You would " +
                "need to provide your customer with accurate information about the product, such as the ingredients " +
                "and manufacturing process. With blockchain, you could upload the product’s data onto the network and " +
                "let customers see this information directly.</p><p>This increased transparency can have several " +
                "benefits for businesses. For example, it can help build trust between companies and their customers. " +
                "It can also lead to higher customer engagement rates, as customers feel more connected to the " +
                "businesses they are interacting with.</p>",
                "<h3><strong>Principle 4: Security</strong></h3>" +
                "<p>One of the principal advantages of blockchain technology is its security. Allowing users to see all " +
                "the information associated with transaction security can improve the trustworthiness and understanding " +
                "of transactions. By making every transaction visible on a public ledger, all parties involved are " +
                "assured that their actions remain traceable and transparent.</p><p>In order to understand how " +
                "blockchain security works, it is vital first to understand the concept of a blockchain database. " +
                "A blockchain database is simply a database that is stored on the network and is accessed using " +
                "distributed ledger technology. This means that every node in the network has access to the same " +
                "database.</p>" +
                "<p>When you make a transaction on a blockchain network, you are not actually sending money to " +
                "another person. Instead, you create a new block on the chain and add it to the ledger. To verify " +
                "your transaction, miners must inspect your block and determine whether or not it meets certain " +
                "conditions.</p>" +
                "<p>If your block passes inspection, it is added to the chain and recorded as part of the public record. " +
                "This means that everyone with access to the blockchain database can see this information. As long as " +
                "at least 51% of all miners verify blocks, your transaction will be accepted by the network and " +
                "completed.</p>" +
                "<p>This verification process ensures that transactions are secure because only authorized parties " +
                "have access to the chain database and can make valid transactions. Anyone who tries to tamper with or " +
                "falsify transactions on a blockchain network will quickly find themselves unable to complete any tasks " +
                "on the network and become an obvious target for attackers.</p>",
                "<h3><strong>Principle 5: Scalability</strong></h3>" +
                "<p>Blockchain scalability is the capacity of a blockchain system to accommodate an increased volume " +
                "of transactions without compromising its overall intricacy. The three main factors that affect " +
                "scalability are execution, storage, and consensus.</p><p>Execution refers to the speed at which " +
                "transactions are processed on the blockchain network. This is determined by several factors, including " +
                "the number of nodes involved in the network, their processing power, and the bandwidth available.</p>" +
                "<p>Storage refers to the amount of data that can be stored on a blockchain network. The size of blocks " +
                "and the frequency at which they are generated play a crucial role in determining their volume.</p>" +
                "<p>Consensus refers to the agreement of all nodes involved in a blockchain system about which " +
                "transaction is valid. Nodes can reach an agreement by voting or by using a proof-of-work algorithm.</p>" +
                "<p>Scalability is one of the significant advantages of blockchain technology. By allowing multiple " +
                "nodes to access the network at once, blockchain can handle a much larger number of transactions " +
                "than traditional systems.</p>",
                "<h3><strong>Principle 6: Privacy</strong></h3>" +
                "<p>Privacy verifies the transparency of data and transactions on the network, providing users with a " +
                "degree of trustworthiness. All information is accessible to everyone; therefore, it cannot be " +
                "misrepresented. It also allows for greater accountability since everyone involved with a blockchain " +
                "project is publicly visible.</p>" +
                "<p>However, it is essential to note that privacy is not guaranteed forever. Eventually, all blockchain " +
                "data will be publicly available, so it is important to take precautions to protect user privacy. " +
                "One of the options is to use strong cryptography and ensure that all network nodes are reliable and " +
                "honest. Additionally, a private blockchain network may be more beneficial than a public one.</p>"
            };

            // adding dashboard widgets
            var widgetHtmlBuilderService = serviceScope.ServiceProvider.GetService<IWidgetHtmlBuilderService>();
            var widgets = new List<HtmlDTO>();
            foreach (var content in contents)
            {
                widgets.Add(await widgetHtmlBuilderService.Create(new HtmlDTO
                {
                    WidgetSource = new WidgetSourceDTO(),
                    InnerHtml = content
                }, defOwnerUserId));
            }

            var rowIndex = 0;
            var columnIndex = 0;

            dashboard.Widgets = widgets.ConvertAll(html =>
            {
                var widget = new DashboardBuildWidgetDTO
                {
                    WidgetSourceId = html.WidgetSourceId,
                    RowIndex = rowIndex,
                    ColumnIndex = columnIndex
                };

                if (rowIndex == columnIndex)
                {
                    rowIndex++;
                    columnIndex = 0;
                }
                else columnIndex++;

                return widget;
            });

            await dashboardBuilderService.Update(dashboard);
        }
    }
}