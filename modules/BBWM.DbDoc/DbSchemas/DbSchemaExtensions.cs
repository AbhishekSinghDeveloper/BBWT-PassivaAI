using BBWM.DbDoc.DbGraph;
using BBWM.DbDoc.DbGraph.Algorithms;
using BBWM.DbDoc.DbSchemas.SchemaModels;

namespace BBWM.DbDoc.DbSchemas
{
    public static class DbSchemaExtensions
    {
        public static DbGraph.DbGraph ToTablesGraph(this DbSchema dbSchema)
        {
            var graph = new DbGraph.DbGraph();

            foreach (var table in dbSchema.Tables.Values)
            {
                var primaryKey = dbSchema.GetTableColumns(table.TableId)
                    .FirstOrDefault(x => x.IsPrimaryKey == true);

                graph.AddVertex(table.TableId, new DbTableInfo
                {
                    Table = table,
                    PrimaryKeyColumn = primaryKey,
                });
            }

            foreach (var table in dbSchema.Tables.Values)
            {
                var tableColumns = dbSchema.GetTableColumns(table.TableId);

                foreach (var tableRef in tableColumns.SelectMany(x => x.TableReferences))
                {
                    if (tableRef.TargetTableId == table.TableId)
                    {
                        var sourceTableColumn = tableColumns.Single(x => x.ColumnId == tableRef.TargetColumnId);
                        var targetTableColumn = dbSchema.GetTableColumns(tableRef.SourceTableId)
                            .Single(x => x.ColumnId == tableRef.SourceColumnId);

                        if (targetTableColumn is not null)
                            graph.AddEdge(tableRef.TargetTableId, tableRef.SourceTableId,
                                new DbTablesRelation
                                {
                                    StartTableColumn = sourceTableColumn,
                                    EndTableColumn = targetTableColumn,
                                    IsRequired = tableRef.IsRequired
                                }, 1);
                    }
                    else
                    {
                        var sourceTableColumn = tableColumns.Single(x => x.ColumnId == tableRef.SourceColumnId);
                        var targetTableColumn = dbSchema.GetTableColumns(tableRef.TargetTableId)
                            .Single(x => x.ColumnId == tableRef.TargetColumnId);

                        if (targetTableColumn is not null)
                            graph.AddEdge(tableRef.SourceTableId, tableRef.TargetTableId,
                                new DbTablesRelation
                                {
                                    StartTableColumn = sourceTableColumn,
                                    EndTableColumn = targetTableColumn,
                                    IsRequired = tableRef.IsRequired
                                }, 1);
                    }
                }
            }

            return graph;
        }

        public static GraphDijkstra ToDijkstra(this DbSchema dbSchema, string startTableId) =>
            new(dbSchema.ToTablesGraph(), startTableId);
    }
}
