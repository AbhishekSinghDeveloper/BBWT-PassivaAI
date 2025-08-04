using BBWM.DataProcessing.FileReaders;
using BBWM.DataProcessing.Test.ExcelReader;

namespace BBWM.DataProcessing.Test;

public class XlsxFileReaderTest : ExcelReaderBase
{
    protected override Stream GetContentStream()
    {
        return File.OpenRead("modules/BBWM.DataProcessing.Test/Content/data.xlsx");
    }

    protected override IDataImportReader GetReader()
    {
        return new XlsxFileReader();
    }
}
