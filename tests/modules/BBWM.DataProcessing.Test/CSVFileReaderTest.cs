using BBWM.DataProcessing.FileReaders;

namespace BBWM.DataProcessing.Test;

public class CsvFileReaderTest : DataImportReaderTest
{
    protected override Stream GetContentStream()
    {
        return File.OpenRead("modules/BBWM.DataProcessing.Test/Content/data.csv");
    }

    protected override IDataImportReader GetReader()
    {
        return new CSVFileReader();
    }
}
