using BBWM.Core.Extensions;
using BBWM.DbDoc.Enums;
using BBWM.DbDoc.Interfaces;
using System.Xml;

namespace BBWM.DbDoc.Services;

public class DbDocAnonymizeService : IDbDocAnonymizeService
{
    private readonly IDbDocFolderService dbDocFolderService;

    public DbDocAnonymizeService(IDbDocFolderService dbDocFolderService) =>
        this.dbDocFolderService = dbDocFolderService;

    public async Task<byte[]> GetAnonymizationXml(Guid folderId, CancellationToken ct)
    {
        var folder = await dbDocFolderService.GetFolder(folderId, ct);

        using (var memoryStream = new MemoryStream())
        {
            using (var writer = XmlWriter.Create(memoryStream,
                new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Auto }))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("configuration");
                writer.WriteAttributeString("jdbcurl", string.Empty);

                foreach (var table in folder.Tables)
                {
                    writer.WriteStartElement("table");
                    writer.WriteAttributeString("name", table.StaticData.TableName);

                    if (table.Anonymization == AnonymizationAction.Anonymize)
                    {
                        foreach (var column in table.Columns.Where(x => x.AnonymizationRule != null))
                        {
                            writer.WriteStartElement("column");
                            writer.WriteAttributeString("name", column.StaticData.ColumnName);
                            writer.WriteAttributeString("type",
                                column.AnonymizationRule != null
                                    ? ((AnonymizationRule)column.AnonymizationRule).ToEnumValueString()
                                    : "");
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
            }

            return memoryStream.ToArray();
        }
    }
}
