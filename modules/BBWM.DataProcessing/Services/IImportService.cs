using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;

namespace BBWM.DataProcessing.Services;

public interface IImportService
{
    DataImportConfig CreateSettings(ImportDataModel importDataModel, MemoryStream memoryStream, string userId);
    Task<DataImportResultDTO> Import(DataImportConfig dataImportConfig, CancellationToken cancellationToken);
}
