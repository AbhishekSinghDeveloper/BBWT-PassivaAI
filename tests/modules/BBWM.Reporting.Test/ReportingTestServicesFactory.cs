using AutoMapper;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Services;
using BBWM.Core.Test;
using BBWM.Core.Test.Utils;
using BBWM.DbDoc.DbSchema.SchemaModels;
using BBWM.DbDoc.DbSchema.SchemaReaders.ContextModels;
using BBWM.DbDoc.Enums;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;
using BBWM.DbDoc.Tests;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Model;
using BBWM.Reporting.Services;

using BBWT.Data;
using Bogus;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Collections.Immutable;
using System.Runtime.Serialization;
using System.Security.Claims;

using ClaimTypes = System.Security.Claims.ClaimTypes;

namespace BBWM.Reporting.Test;

public class ReportingTestServicesFactory
{
    public const string InMemoryTableId = "DataContext.Orders";
    public const string InMemoryColumnIdNumber = "DataContext.Orders.Id";
    public const string InMemoryColumnIdDate = "DataContext.Orders.RequiredDate";
    public const string InMemoryColumnIdBool = "DataContext.Orders.IsPaid";
    public const string InMemoryColumnIdString = "DataContext.Orders.Title";

    private static int _counter = 0;

    private DbDocTestServicesFactory _dbDocTestServicesFactory;


    public IMapper Mapper { get; private set; }

    public IHttpContextAccessor HttpContextAccessor { get; private set; }

    public User CurrentUser { get; private set; }

    public IDataContext InMemoryContext { get; private set; }

    public IDataContext SqlLiteContext => _dbDocTestServicesFactory.SqlLiteContext;

    public IDataService DataService { get; private set; }

    public IDbDocService DbDocService => _dbDocTestServicesFactory.DbDocService;

    //TODO: change DbDocStaticDataService to DatabaseSchemaManager
    public IDbDocStaticDataService DbDocStaticDataService => _dbDocTestServicesFactory.DbDocStaticDataService;

    public IReportService InMemoryReportService { get; private set; }

    public IReportService SqlLiteReportService { get; private set; }

    public ISectionService InMemorySectionService { get; private set; }

    public IQueryBuilderService InMemoryQueryBuilderService { get; private set; }

    public IViewBuilderService InMemoryViewBuilderService { get; private set; }


    public async Task CreateInMemoryReportingServices()
    {
        if (Mapper is null)
            Mapper = AutoMapperConfig.CreateMapper();

        if (InMemoryContext is null)
            InMemoryContext = SutDataHelper.CreateEmptyContext<DataContext>();

        if (CurrentUser is null)
        {
            CurrentUser = new User();
            await InMemoryContext.Set<User>().AddAsync(CurrentUser, CancellationToken.None);
            await InMemoryContext.SaveChangesAsync(CancellationToken.None);
        }

        if (HttpContextAccessor is null)
            HttpContextAccessor = ServicesFactory
                .GetHttpContextAccessor(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, CurrentUser.Id) });

        if (DataService is null)
            DataService = new DataService(InMemoryContext, Mapper);

        var dbDocStaticDataService = CreateDbDocStaticDataService();

        if (InMemoryQueryBuilderService is null)
            InMemoryQueryBuilderService = new QueryBuilderService(InMemoryContext, Mapper, DataService, null, dbDocStaticDataService);

        if (InMemoryViewBuilderService is null)
            InMemoryViewBuilderService = new ViewBuilderService(InMemoryContext, Mapper, DataService, null, dbDocStaticDataService);

        if (InMemoryReportService is null)
        {
            InMemoryReportService = new ReportService(
                InMemoryContext,
                Mapper,
                DataService,
                HttpContextAccessor,
                Core.Membership.Test.ServicesFactory.GetUserManager((DataContext)InMemoryContext),
                new UserService(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, InMemoryContext, null, Mapper),
                null);
        }

        if (InMemorySectionService is null)
        {
            InMemorySectionService = new SectionService(
                InMemoryContext,
                Mapper,
                HttpContextAccessor,
                InMemoryQueryBuilderService,
                InMemoryViewBuilderService,
                null);
        }
    }

    public async Task CreateSqlLiteReportServices()
    {
        _dbDocTestServicesFactory = new DbDocTestServicesFactory();
        await _dbDocTestServicesFactory.CreateDbDocServices();

        CurrentUser = new User();
        await SqlLiteContext.Set<User>().AddAsync(CurrentUser, CancellationToken.None);
        await SqlLiteContext.SaveChangesAsync(CancellationToken.None);
        var contextAccessor = ServicesFactory.GetHttpContextAccessor(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, CurrentUser.Id) });

        var mapper = AutoMapperConfig.CreateMapper();
        SqlLiteReportService = new ReportService(
            SqlLiteContext,
            mapper,
            new DataService(InMemoryContext, mapper),
            contextAccessor,
            Core.Membership.Test.ServicesFactory.GetUserManager((DataContext)SqlLiteContext),
            new UserService(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, SqlLiteContext, null, mapper),
            null);
    }

    public async Task DeleteServices()
    {
        if (InMemoryContext != null)
        {
            InMemoryContext.Dispose();
            InMemoryContext = null;
        }

        await _dbDocTestServicesFactory.DeleteServices();
        SqlLiteReportService = null;
        InMemoryReportService = null;
        InMemorySectionService = null;
    }


    public Report CreateReport() =>
        new ()
        {
            Name = $"Report Name {_counter++}",
            Access = "Authenticated",
            CreatedBy = CurrentUser.Id,
            CreatedOn = DateTime.UtcNow,
            IsDraft = false,
            UpdatedBy = CurrentUser.Id,
            UpdatedOn = DateTime.UtcNow,
            UrlSlug = $"report-url-slug-{_counter++}",
        };

    public Section CreateSection() =>
        new ()
        {
            Title = $"Report Section Title {_counter++}",
            Description = "Report description",
        };

    public Report CreateFullReport()
    {
        var report = CreateReport();

        var section = CreateSection();
        report.Sections.Add(section);

        var query = new Query
        {
            DbDocFolderId = Guid.NewGuid(),
            QueryTables = new List<QueryTable>
            {
                new QueryTable
                {
                    DbDocTableId = InMemoryTableId,
                    Columns = new List<QueryTableColumn>
                    {
                        new QueryTableColumn { DbDocColumnId = InMemoryColumnIdNumber },
                        new QueryTableColumn { DbDocColumnId = InMemoryColumnIdDate },
                        new QueryTableColumn { DbDocColumnId = InMemoryColumnIdBool },
                        new QueryTableColumn { DbDocColumnId = InMemoryColumnIdString }
                    }
                }
            }
        };

        query.RootFilterSet = new QueryFilterSet
        {
            ConditionalOperator = QueryConditionalOperator.And,
            Query = query,
            ParentQuery = query,
            QueryFilters = new List<QueryFilter>
            {
                new QueryFilter
                {
                    Value = 3,
                    Value2 = 5,
                    QueryTableColumn = query.QueryTables[0].Columns[0],
                    QueryRule = new QueryRule
                    {
                        Name = $"Query Rule {_counter++}",
                        Code = QueryRuleCode.Between,
                        RuleTypes = new List<QueryRuleType>()
                        {
                            new QueryRuleType { Type = QueryRuleDataType.Numeric }
                        }
                    }
                }
            },
            ChildSets = new List<QueryFilterSet>
            {
                new QueryFilterSet
                {
                    ConditionalOperator = QueryConditionalOperator.Or,
                    Query = query,
                    QueryFilters = new List<QueryFilter>
                    {
                        new QueryFilter
                        {
                            Value = DateTime.UtcNow,
                            QueryTableColumn = query.QueryTables[0].Columns[1],
                            QueryRule = new QueryRule
                            {
                                Name = $"Query Rule {_counter++}",
                                Code = QueryRuleCode.Less,
                                RuleTypes = new List<QueryRuleType>()
                                {
                                    new QueryRuleType { Type = QueryRuleDataType.Datetime }
                                }
                            }
                        },
                        new QueryFilter
                        {
                            Value = "a",
                            QueryTableColumn = query.QueryTables[0].Columns[2],
                            QueryRule = new QueryRule
                            {
                                Name = $"Query Rule {_counter++}",
                                Code = QueryRuleCode.Contains,
                                RuleTypes = new List<QueryRuleType>()
                                {
                                    new QueryRuleType { Type = QueryRuleDataType.String }
                                }
                            }
                        }
                    },
                    ChildSets = new List<QueryFilterSet>
                    {
                        new QueryFilterSet
                        {
                            ConditionalOperator = QueryConditionalOperator.Or,
                            Query = query,
                            QueryFilters = new List<QueryFilter>
                            {
                                new QueryFilter
                                {
                                    Value = 1,
                                    QueryTableColumn = query.QueryTables[0].Columns[0],
                                    QueryRule = new QueryRule
                                    {
                                        Name = $"Query Rule {_counter++}",
                                        Code = QueryRuleCode.Equals,
                                        RuleTypes = new List<QueryRuleType>()
                                        {
                                            new QueryRuleType { Type = QueryRuleDataType.String },
                                            new QueryRuleType { Type = QueryRuleDataType.Numeric },
                                            new QueryRuleType { Type = QueryRuleDataType.Datetime },
                                            new QueryRuleType { Type = QueryRuleDataType.Boolean }
                                        }
                                    }
                                },
                                new QueryFilter
                                {
                                    Value = 10,
                                    QueryTableColumn = query.QueryTables[0].Columns[0],
                                    QueryRule = new QueryRule
                                    {
                                        Name = $"Query Rule {_counter++}",
                                        Code = QueryRuleCode.LessOrEqual,
                                        RuleTypes = new List<QueryRuleType>()
                                        {
                                            new QueryRuleType { Type = QueryRuleDataType.Numeric }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
        section.Query = query;

        section.View = new View
        {
            GridView = new GridView
            {
                DefaultSortColumn = query.QueryTables[0].Columns[1],
                DefaultSortOrder = SortOrder.Desc,
                ViewColumns = new List<GridViewColumn>
                {
                    new GridViewColumn
                    {
                        Header = InMemoryColumnIdNumber,
                        QueryTableColumn = query.QueryTables[0].Columns[0],
                        SortOrder = 0,
                        Visible = false
                    },
                    new GridViewColumn
                    {
                        Header = InMemoryColumnIdDate,
                        QueryTableColumn = query.QueryTables[0].Columns[1],
                        SortOrder = 1
                    },
                    new GridViewColumn
                    {
                        Header = InMemoryColumnIdBool,
                        QueryTableColumn = query.QueryTables[0].Columns[2],
                        SortOrder = 2,
                        Sortable = false
                    },
                    new GridViewColumn
                    {
                        Header = InMemoryColumnIdString,
                        QueryTableColumn = query.QueryTables[0].Columns[3],
                        SortOrder = 3,
                        Sortable = false
                    }
                }
            },
            Filters = new List<FilterControl>
            {
                new FilterControl
                {
                    SortOrder = 0,
                    InputType = InputType.Number,
                    Name = $"Filter control name {_counter}",
                    HintText = $"Filter control hint text {_counter++}",
                    UserCanChangeOperator = false,
                    QueryFilterBindings = new List<QueryFilterBinding>
                    {
                        new QueryFilterBinding
                        {
                            BindingType = QueryFilterBindingType.FilterControl,
                            QueryFilter = query.RootFilterSet.QueryFilters[0]
                        }
                    }
                },
                new FilterControl
                {
                    SortOrder = 1,
                    InputType = InputType.Calendar,
                    Name = $"Filter control name {_counter}",
                    HintText = $"Filter control hint text {_counter++}",
                    UserCanChangeOperator = true,
                    QueryFilterBindings = new List<QueryFilterBinding>
                    {
                        new QueryFilterBinding
                        {
                            BindingType = QueryFilterBindingType.FilterControl,
                            QueryFilter = query.RootFilterSet.ChildSets[0].QueryFilters[0]
                        }
                    }
                },
                new FilterControl
                {
                    SortOrder = 2,
                    InputType = InputType.Text,
                    Name = $"Filter control name {_counter}",
                    HintText = $"Filter control hint text {_counter++}",
                    UserCanChangeOperator = true,
                    QueryFilterBindings = new List<QueryFilterBinding>
                    {
                        new QueryFilterBinding
                        {
                            BindingType = QueryFilterBindingType.FilterControl,
                            QueryFilter = query.RootFilterSet.ChildSets[0].QueryFilters[1]
                        }
                    }
                }
            }
        };

        return report;
    }

    public Folder CreateFullFolder() =>
        new ()
        {
            ChangedOn = DateTime.Now,
            Name = $"DBDoc folder {_counter++}",
            Owners = ModuleLinkage.DbDocFolderOwnerName,
            Tables = new List<TableMetadata>
            {
                new TableMetadata
                {
                    TableId = InMemoryTableId,
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata
                        {
                            ColumnId = InMemoryColumnIdNumber
                        },
                        new ColumnMetadata
                        {
                            ColumnId = InMemoryColumnIdDate
                        },
                        new ColumnMetadata
                        {
                            ColumnId = InMemoryColumnIdBool
                        },
                        new ColumnMetadata
                        {
                            ColumnId = InMemoryColumnIdString
                        }
                    }
                }
            }
        };


    // TODO: Rework to DatabaseSchemaManager
    private static DbDocStaticDataService CreateDbDocStaticDataService()
    {
        var dbDocStaticDataService = (DbDocStaticDataService)FormatterServices.GetUninitializedObject(typeof(DbDocStaticDataService));
        var dbDocStaticDataServiceType = dbDocStaticDataService.GetType();

        dbDocStaticDataServiceType.GetProperty(nameof(dbDocStaticDataService.TablesStaticData))
            .SetValue(dbDocStaticDataService, new Dictionary<string, SchemaTable>
            {
                [InMemoryTableId] = new SchemaTable
                {
                    TableName = "Orders"
                }
            }.ToImmutableSortedDictionary());
        dbDocStaticDataServiceType.GetProperty(nameof(dbDocStaticDataService.ColumnsStaticData))
            .SetValue(dbDocStaticDataService, new Dictionary<string, SchemaColumn>
            {
                [InMemoryColumnIdNumber] = new SchemaColumn
                {
                    ColumnName = "Id",
                    AllowNull = false,
                    ClrTypeGroup = ClrTypeGroup.Numeric,
                    IsPrimaryKey = true,
                    IsIndex = true,
                },
                [InMemoryColumnIdDate] = new SchemaColumn
                {
                    ColumnName = "RequiredDate",
                    AllowNull = true,
                    ClrTypeGroup = ClrTypeGroup.Date,
                    IsPrimaryKey = false,
                    IsIndex = false,
                },
                [InMemoryColumnIdBool] = new SchemaColumn
                {
                    ColumnName = "IsPaid",
                    AllowNull = true,
                    ClrTypeGroup = ClrTypeGroup.Bool,
                    IsPrimaryKey = false,
                    IsIndex = false,
                },
                [InMemoryColumnIdString] = new SchemaColumn
                {
                    ColumnName = "Title",
                    AllowNull = true,
                    ClrTypeGroup = ClrTypeGroup.String,
                    IsPrimaryKey = false,
                    IsIndex = false,
                }
            }.ToImmutableSortedDictionary());

        return dbDocStaticDataService;
    }
}
