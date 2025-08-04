using BBWM.Core.Data;
using BBWM.Core.DTO;
using BBWM.Core.Filters;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web.ModelBinders;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Core.Web;

/// <summary>
/// Base Data Controller.
/// </summary>
/// <remarks>
/// <para>
///Business controller inherits the base data controller. This is required because the automation
///supposes we want CRUD/paging API methods with minimum coding efforts.
///But we override methods in the business controller if needed (for example to changes roles/permissions).
///Base data controller is the only sophisticated part of CRUD/paging automation model. It takes
///responsibility for triggering either the default service implementing CRUD/paging or the business
///service (data handler). What it gives – with basic CRUD/paging, without customization all you
///need is to add an empty controller. It will trigger the default service. No business service,
///no dependency injections mapping.
/// </para>
///<para>
///As soon as the business service implements, for example, the Create method’s interface, the base
///controller will trigger it. You don’t need to study how the base controller works and what’s hidden
///behind it, you should only know a shortlist of interfaces to implement on the services layer:
///<see cref="IEntityCreate{TDTO}"/>,
///<see cref="IEntityUpdate{TDTO}"/>,
///<see cref="IEntityGet{TDTO, TKey}"/>,
///<see cref="IEntityDelete{TKey}"/>,
///<see cref="IEntityPage{TDTO}"/>,
///<see cref="IEntityQuery{TEntity}"/>.
///</para>
///<para>
///The base data controller is generic. It defines Model, DTO and DB context classes. You can choose
///what context the controller works with. Otherwise, you would need to create an extra class to pass
///the context for dependency injection.
///</para>
///<para>
///How and when does the base controller triggers the service layer? 
///As long as the business service implements an interface of CRUD/paging method, it remains as priority
///handler for the controller to call the handling method. Otherwise, if not implemented then default
///method of default data service (DataService) is called. The base controller’s
///constructor receives either DataService as a parameter or/and Business Service which customizes
///CRUD/paging or both. Why is it so? It allows to achieve these goals:
///<list type="number">
///<item>
///Business service can have a custom implementation of CRUD/paging interfaces instead of
///inheriting from base CRUD/paging service.
///</item>
///<item>
///Ability to use a default implementation (DataService) without creating a custom business service.
///</item>
///</list>
///</para>
///<para>
///Steps for developer to set up the controller:
///<list type="number">
///<item>Inherit business controller from DataControllerBase.</item>
///<item>Pass business service to the constructor (if customizes CRUD/paging).</item>
///<item>Pass default data service to the constructor (if doesn’t customize).</item>
///<item>Pass default data service to the constructor (if doesn’t customize).</item>
///<item>Set generic part of the DataControllerBase controller and define Model, DTO, paged list DTO and DB context(optional).</item>
///</list>
///</para>
/// <para><see href="https://wiki.bbconsult.co.uk/display/BLUEB/Grid+Pages+Automation">See Wiki page</see></para>
///</remarks>
/// <typeparam name="TContext">Database context type.</typeparam>
/// <typeparam name="TEntity">Entity type.</typeparam>
/// <typeparam name="TDTO">Entity's DTO type.</typeparam>
/// <typeparam name="TPageDTO">Paged list entity's DTO type.</typeparam>
/// <typeparam name="TKey">ID field type of entity.</typeparam>
[ApiController]
public abstract class DataControllerBase<TContext, TEntity, TDTO, TPageDTO, TKey> : ControllerBase
    where TContext : IDbContext
    where TDTO : class, IDTO<TKey>
    where TPageDTO : class, IDTO<TKey>
    where TKey : IEquatable<TKey>
    where TEntity : class, IEntity<TKey>
{
    /// <value>
    /// Default CRUD/paging data service.
    /// </value>
    protected IDataService<TContext> DataService { get; }

    /// <value>
    /// Handler data service to customize CRUD/paging.
    /// </value>
    private IDataHandler DataHandler { get; }

    /// <summary>
    /// If dataHandler is passed, it handles all CRUD/paging interfaces it implements.
    /// Otherwise, dataServices handles data processing using default implementation.
    /// </summary>
    /// <param name="dataService">Default CRUD/paging data service.</param>
    /// <param name="dataHandler">Handler data service to customize CRUD/paging.</param>
    protected DataControllerBase(IDataService<TContext> dataService, IDataHandler dataHandler = null)
    {
        DataService = dataService;
        DataHandler = dataHandler;
    }

    /// <summary>
    /// dataHandler is fully responsible for handling CRUD/paging interfaces.
    /// If it doesn't implement an interface for the called method (e.g.
    /// doesn't implement <see cref="IEntityCreate{TDTO}"/> when API Create() is called)
    /// then exception is thrown.
    /// </summary>
    /// <param name="dataHandler">Handler data service to customize CRUD/paging.</param>
    protected DataControllerBase(IDataHandler dataHandler)
        => DataHandler = dataHandler;

    /// <summary>
    /// Gets entity by ID.
    /// </summary>
    /// <remarks>
    /// if data handler (<see cref="DataHandler"/>) implements <see cref="IEntityGet{TDTO, TKey}"/> interface
    /// then it's called, otherwise Get() of default data service (<see cref="DataService"/>) is used.
    /// <para>
    /// Also, if data handler doesn't implement IEntityGet but implements <see cref="IEntityQuery{TEntity}"/>
    /// then default Get() will modify the query using IEntityQuery implementation.
    /// </para>
    /// <para>This method can be overridden to customize logic / to modify API method role and permissions / to modify API method's route.</para>
    /// </remarks>
    /// <param name="id">ID of requested entity.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>DTO of requested entity.</returns>
    [HttpGet, Route("{id}")]
    public virtual async Task<IActionResult> Get([HashedKeyBinder] TKey id, CancellationToken ct = default)
    {
        if (DataHandler is IEntityGet<TDTO, TKey> methodHandler)
            return Ok(await methodHandler.Get(id, ct));

        return Ok(await DataService.Get<TEntity, TDTO, TKey>(id, DataHandler as IEntityQuery<TEntity>, ct));
    }

    /// <summary>
    /// Creates entity by DTO.
    /// </summary>
    /// <remarks>
    /// if data handler (<see cref="DataHandler"/>) implements <see cref="IEntityCreate{TDTO}"/> interface
    /// then it's called, otherwise Create() of default data service (<see cref="DataService"/>) is used.
    /// <para>This method can be overridden to customize logic / to modify API method role and permissions / to modify API method's route.</para>
    /// </remarks>
    /// <param name="dto">DTO of entity for creating.</param>
    /// <param name="modelHashingService">A service to hash an ID of created entity which is used on client-side.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>DTO of created entity.</returns>
    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TDTO dto,
        [FromServices] IModelHashingService modelHashingService, CancellationToken ct = default)
    {
        if (DataHandler is IEnityValidate<TDTO, TKey> validator)
        {
            validator.Validate(dto);
        }

        var result = (DataHandler is IEntityCreate<TDTO> methodHandler) ?
            await methodHandler.Create(dto, ct) :
            await DataService.Create<TEntity, TDTO>(dto, ct);

        return Request.CreatedResult<TDTO, TKey>(result, modelHashingService);
    }

    /// <summary>
    /// Updates entity by DTO.
    /// </summary>
    /// <remarks>
    /// if data handler (<see cref="DataHandler"/>) implements <see cref="IEntityUpdate{TDTO}"/> interface
    /// then it's called, otherwise Update() of default data service (<see cref="DataService"/>) is used.
    /// <para>This method can be overridden to customize logic / to modify API method role and permissions / to modify API method's route.</para>
    /// </remarks>
    /// <param name="dto">DTO of entity for updating.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>DTO of updated entity.</returns>
    [HttpPut, Route("{id}")]
    public virtual async Task<IActionResult> Update([FromBody] TDTO dto, CancellationToken ct = default)
    {
        if (DataHandler is IEnityValidate<TDTO, TKey> validator)
        {
            validator.Validate(dto);
        }

        if (DataHandler is IEntityUpdate<TDTO> methodHandler)
            return Ok(await methodHandler.Update(dto, ct));

        return Ok(await DataService.Update<TEntity, TDTO, TKey>(dto, ct));
    }

    /// <summary>
    /// Deletes entity by ID.
    /// </summary>
    /// <remarks>
    /// if data handler (<see cref="DataHandler"/>) implements <see cref="IEntityDelete{TKey}"/> interface
    /// then it's called, otherwise Delete() of default data service (<see cref="DataService"/>) is used.
    /// <para>This method can be overridden to customize logic / to modify API method role and permissions / to modify API method's route.</para>
    /// </remarks>
    /// <param name="id">ID of entity.</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpDelete, Route("{id}")]
    public virtual async Task<IActionResult> Delete([HashedKeyBinder] TKey id, CancellationToken ct = default)
    {
        if (DataHandler is IEntityDelete<TKey> methodHandler)
            await methodHandler.Delete(id, ct);
        else
            await DataService.Delete<TEntity, TKey>(id, ct);

        return Ok(id);
    }

    /// <summary>
    /// Gets a paged list of entities by query command.
    /// </summary>
    /// <remarks>
    /// if data handler (<see cref="DataHandler"/>) implements <see cref="IEntityPage{TPageDTO}"/> interface
    /// then it's called, otherwise GetPage() of default data service (<see cref="DataService"/>) is used.
    /// <para>
    /// Also, if data handler doesn't implement IEntityPage but implements <see cref="IEntityQuery{TEntity}"/>
    /// then default GetPage() will modify the query using IEntityQuery implementation.
    /// </para>
    /// <para>This method can be overridden to customize logic / to modify API method role and permissions / to modify API method's route.</para>
    /// </remarks>
    /// <param name="command">A query command with settings that determine the requested paged list.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of DTOs of entities.</returns>
    [HttpGet, Route("page")]
    public virtual async Task<IActionResult> GetPage([FromQuery] QueryCommand command, CancellationToken ct = default)
    {
        //agreed max no of returned records
        const int maxNumberRecordsToTake = 100;
        if (command.Take is null or > maxNumberRecordsToTake) command.Take = maxNumberRecordsToTake;

        if (DataHandler is IEntityPage<TPageDTO> methodHandler)
            return Ok(await methodHandler.GetPage(command, ct));

        return Ok(await DataService.GetPage<TEntity, TPageDTO>(
            command, DataHandler as IEntityQuery<TEntity>, ct, false));
    }
}

/// <summary>
/// Base Data Controller.
/// Class variation for default database context.
/// </summary>
/// <inheritdoc/>
public abstract class DataControllerBase<TEntity, TDTO, TPageDTO, TKey> : DataControllerBase<IDbContext, TEntity, TDTO, TPageDTO, TKey>
    where TDTO : class, IDTO<TKey>
    where TPageDTO : class, IDTO<TKey>
    where TKey : IEquatable<TKey>
    where TEntity : class, IEntity<TKey>
{
    /// <inheritdoc/>
    protected DataControllerBase(IDataService<IDbContext> dataService, IDataHandler dataHandler = null)
        : base(dataService, dataHandler)
    {
    }

    /// <inheritdoc/>
    protected DataControllerBase(IDataHandler dataHandler) : base(null, dataHandler) { }
}

/// <summary>
/// Base Data Controller.
/// Class variation for default database context and entities with integer ID field.
/// </summary>
/// <inheritdoc/>    
public abstract class DataControllerBase<TEntity, TDTO, TPageDTO> : DataControllerBase<IDbContext, TEntity, TDTO, TPageDTO, int>
    where TDTO : class, IDTO
    where TPageDTO : class, IDTO
    where TEntity : class, IEntity
{
    /// <inheritdoc/>
    protected DataControllerBase(IDataService<IDbContext> dataService, IDataHandler dataHandler = null)
        : base(dataService, dataHandler)
    {
    }

    /// <inheritdoc/>
    protected DataControllerBase(IDataHandler dataHandler) : base(null, dataHandler) { }
}
