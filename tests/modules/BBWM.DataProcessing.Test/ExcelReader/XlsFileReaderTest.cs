using BBWM.DataProcessing.FileReaders;
using BBWM.DataProcessing.Test.ExcelReader;

namespace BBWM.DataProcessing.Test;

public class XlsFileReaderTest : ExcelReaderBase
{
    protected override Stream GetContentStream()
    {
        return File.OpenRead("modules/BBWM.DataProcessing.Test/Content/data.xls");
    }

    protected override IDataImportReader GetReader()
    {
        return new XlsFileReader();
    }
}
