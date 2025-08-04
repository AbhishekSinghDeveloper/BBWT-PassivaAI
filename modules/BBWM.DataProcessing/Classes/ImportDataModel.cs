using BBWM.DataProcessing.DTO;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.DataProcessing.Classes;

public class ImportDataModel
{
    private string _fileName;

    [FromForm]
    public DataImportConfigDTO Config { get; set; }

    public IFormFile File { get; set; }

    public string FileName
    {
        get => _fileName ??= File?.FileName;
        set => _fileName = value;
    }
}
