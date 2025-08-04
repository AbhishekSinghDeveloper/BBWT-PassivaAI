using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using BBWM.DbDoc.DTO;
using BBWM.DbDoc.Interfaces;
using BBWM.DbDoc.Model;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.DbDoc.Api;

[Route("api/dbdoc/column-type")]
[ReadWriteAuthorize(ReadWriteRoles = BBWM.Core.Roles.SuperAdminRole)]
public class ColumnTypesController : DataControllerBase<ColumnType, ColumnTypeDTO, ColumnTypeDTO, Guid>
{
    private readonly IColumnTypeService _columnTypeService;


    public ColumnTypesController(IDataService dataService, IColumnTypeService columnTypeService) :
        base(dataService, columnTypeService) => _columnTypeService = columnTypeService;


    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await _columnTypeService.GetAll(cancellationToken));

    [HttpPost("set-validation-metadata/{columnTypeId}")]
    public async Task<IActionResult> SetValidationMetadata(Guid columnTypeId, [FromBody] ColumnValidationMetadataDTO dto, CancellationToken ct) =>
        await DataService.Any<ColumnType>(query => query.Where(x => x.Id == columnTypeId), ct)
            ? Ok(await _columnTypeService.SetValidationMetadata(columnTypeId, dto, ct))
            : throw new EntityNotFoundException($"The column type with ID '{columnTypeId}' not found.");

    [HttpPost("delete-validation-metadata/{columnTypeId}")]
    public async Task<IActionResult> DeleteValidationMetadata(Guid columnTypeId, CancellationToken ct)
    {
        if (!await DataService.Any<ColumnType>(query => query.Where(x => x.Id == columnTypeId), ct))
            throw new EntityNotFoundException($"The column type with ID '{columnTypeId}' not found.");

        await _columnTypeService.DeleteValidationMetadata(columnTypeId, ct);
        return NoContent();
    }

    [HttpPost("set-view-metadata/{columnTypeId}")]
    public async Task<IActionResult> SetValidationMetadata(Guid columnTypeId, [FromBody] ColumnViewMetadataDTO dto, CancellationToken ct) =>
        await DataService.Any<ColumnType>(query => query.Where(x => x.Id == columnTypeId), ct)
            ? Ok(await _columnTypeService.SetViewMetadata(columnTypeId, dto, ct))
            : throw new EntityNotFoundException($"The column type with ID '{columnTypeId}' not found.");

    [HttpPost("delete-view-metadata/{columnTypeId}")]
    public async Task<IActionResult> DeleteViewMetadata(Guid columnTypeId, CancellationToken ct)
    {
        if (!await DataService.Any<ColumnType>(query => query.Where(x => x.Id == columnTypeId), ct))
            throw new EntityNotFoundException($"The column type with ID '{columnTypeId}' not found.");

        await _columnTypeService.DeleteViewMetadata(columnTypeId, ct);
        return NoContent();
    }
}
