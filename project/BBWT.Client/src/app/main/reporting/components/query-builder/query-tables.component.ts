import { Component, Input, OnInit, ViewChild, ViewEncapsulation } from "@angular/core";

import { ConfirmationService, MessageService, SelectItem, TreeNode } from "primeng/api";
import { TreeTable } from "primeng/treetable";

import { DbDocFolderService, IFolder, ITableMetadata } from "@main/dbdoc";
import { QueryBuilderComponent } from "./query-builder.component";
import { Message } from "@bbwt/classes";
import { IQueryBuilderController } from "../../interfaces";
import { IQueryTable, IQueryTableColumn, IQueryTableJoin } from "../../reporting-models";
import { SectionService } from "../../services/section.service";
import { IQueryableTableSource } from "../../model/queryable-table-sources-models";
import { ReportService } from "../../services/report.service";


interface IJoinRelation {
    queryTableJoinId?: string;
    sourceQueryTableColumnId?: string;
    sourceDbDocColumnId?: string;
    destinationQueryTable?: IQueryTable;
    destinationQueryTableId?: string;
    destinationQueryTableColumnId?: string;
    destinationDbDocTableId?: string;
    destinationDbDocColumnId?: string;
}

@Component({
    selector: "query-tables",
    templateUrl: "./query-tables.component.html",
    styleUrls: ["./query-tables.component.scss"],
    encapsulation: ViewEncapsulation.None
})
export class QueryTablesComponent implements OnInit {
    @Input() loading: boolean;

    _activeEditingJoin: IJoinRelation;
    _activeEditingJoinFromSideExistingQueryTableColumn: boolean;
    _activeEditingJoinFromQueryTableColumnOptions: SelectItem[];
    _activeEditingJoinFromSourceColumnOptions: SelectItem[];
    _activeEditingJoinToQueryTableColumnOptions: SelectItem[];
    _activeEditingJoinToQueryTableOptions: SelectItem[];
    _activeEditingJoinToSideExistingQueryTable: boolean;
    _activeEditingJoinToSideExistingQueryTableColumn: boolean;
    _activeEditingJoinToSourceColumnOptions: SelectItem[];
    _activeEditingJoinToSourceTableOptions: SelectItem[];
    _cbc: IQueryBuilderController;
    _duplicateCreationColumnOptions: SelectItem[];
    _duplicateCreationDialogVisible: boolean;
    _duplicateCreationJoin: IQueryTableJoin;
    _duplicateCreationSourceTable: IQueryTable;
    _editingJoinsQueryTable: IQueryTable;
    _editingQueryTableJoinRelations: IJoinRelation[];
    _folderOptions: SelectItem[];
    _nodes: TreeNode[] = [];
    _selectedFolder: IFolder;
    _selectedFolderId: string;
    _selectedNodes: TreeNode[] = [];
    _sources: IQueryableTableSource[] = [];
    _sourceNodes: TreeNode[] = [];
    _selectedSourceNodes: TreeNode[] = [];
    _sourceTablesDialogVisible: boolean;
    _tableJoinsEditingDialogVisible: boolean;
    @ViewChild("tablesTree", { static: true }) private _tree: TreeTable;

    constructor(public queryBuilderComponent: QueryBuilderComponent,
                private messageService: MessageService,
                private confirmationService: ConfirmationService,
                private sectionService: SectionService) {
        this._cbc = queryBuilderComponent.cbc;
    }


    ngOnInit(): void {
        this._init();
    }


    refreshView(reCreateTableNodes = true): void {
        if (reCreateTableNodes) {
            this._createTablesTree();
        }

        this._buildSelectedNodes();
        this._setHiddenNodes();

        this._nodes = [...this._nodes];
    }


    _createDuplicateQueryTable(): void {
        this.loading = true;

        this._cbc.addDuplicateQueryTable(this._duplicateCreationJoin)
            .then(() => this._duplicateCreationDialogVisible = false)
            .finally(() => this.loading = false);
    }

    _getColumnName(queryTableColumnId: string): string {
        return this._cbc.queryColumnsMetadataMap[queryTableColumnId]?.staticData?.columnName;
    }

    _getNodeClass(nodeData: any): string {
        switch (nodeData.itemType) {
            case "table": return null;
            case "column": return nodeData.isPrimaryKey == true
                ? "vpn_key"
                : (nodeData.isForeignKey == true
                    ? "insert_link"
                    : "view_column");
            default: return "";
        }
    }

    _getTableName(sourceTableId: string): string {
        return this._cbc.tablesMetadata.find(x => x.tableId === sourceTableId)?.staticData?.tableName;
    }

    _onActiveEditingJoinFromSideExistingQueryTableColumnChanged(): void {
        if (this._activeEditingJoinFromSideExistingQueryTableColumn) {
            this._activeEditingJoin.sourceDbDocColumnId = null;
        } else {
            this._activeEditingJoin.sourceQueryTableColumnId = null;
        }
    }

    _onActiveEditingJoinToSideExistingQueryTableChanged(): void {
        if (this._activeEditingJoinToSideExistingQueryTable) {
            this._activeEditingJoin.destinationDbDocTableId = null;
            this._activeEditingJoin.destinationQueryTableColumnId = null;
            this._activeEditingJoin.destinationDbDocColumnId = null;
            this._activeEditingJoinToSideExistingQueryTableColumn = true;

            this._refreshActiveEditingJoinToQueryTableColumnOptions();
            this._refreshActiveEditingJoinToSourceColumnOptions();
        } else {
            this._activeEditingJoinToSideExistingQueryTableColumn = false;
            this._activeEditingJoin.destinationQueryTableId = null;
            this._activeEditingJoin.destinationQueryTableColumnId = null;
            this._activeEditingJoin.destinationDbDocColumnId = null;

            this._activeEditingJoinToQueryTableColumnOptions = [];
            this._refreshActiveEditingJoinToSourceColumnOptions();
        }
    }

    _onActiveEditingJoinToSideExistingQueryTableColumnChanged(): void {
        if (this._activeEditingJoinToSideExistingQueryTableColumn) {
            this._activeEditingJoin.destinationDbDocColumnId = null;
        } else {
            this._activeEditingJoin.destinationQueryTableColumnId = null;
        }
    }

    _onDuplicateCreationDialogHide(): void {
        this._duplicateCreationJoin = null;
        this._duplicateCreationSourceTable = null;
        this._duplicateCreationColumnOptions = null;
    }

    _onDuplicateCreationFromColumnChange(): void {
        if (this._duplicateCreationJoin.fromDbDocColumnId === this._duplicateCreationJoin.toDbDocColumnId) {
            this._duplicateCreationJoin.toDbDocColumnId = null;
        }
    }

    _onDuplicateCreationToColumnChange(): void {
        if (this._duplicateCreationJoin.fromDbDocColumnId === this._duplicateCreationJoin.toDbDocColumnId) {
            this._duplicateCreationJoin.fromDbDocColumnId = null;
        }
    }

    _onDuplicateQueryTableClick(rowNode: any): void {
        this._duplicateCreationJoin = <IQueryTableJoin> {
            fromDbDocTableId: rowNode.node.data.queryTable.sourceTableId,
            fromQueryTableId: rowNode.node.data.queryTable.id
        };

        this._duplicateCreationSourceTable = rowNode.node.data.queryTable;

        this._cbc.loadFullTableMetadata(rowNode.node.data.tableMetadata.id)
            .then(() => {
                this._duplicateCreationColumnOptions = rowNode.node.data.tableMetadata.columns
                    .map(x => <SelectItem> { label: x.staticData.columnName, value: x.columnId });

                this._duplicateCreationDialogVisible = true;
            });
    }

    _onEditQueryTableJoinsClick(rowNode: any): void {
        if (!this._isTableTreeNode(rowNode.node) || !rowNode.node.data.queryTable) return;

        this._editingJoinsQueryTable = rowNode.node.data.queryTable;
        this._refreshEditingQueryTableJoinsRelations();
        this._tableJoinsEditingDialogVisible = true;
    }

    _onNodeExpand(event: any): void {
        if (!event.node || event.node.data.loaded || !this._isTableTreeNode(event.node) || !event.node.data.tableMetadata) return;

        this.loading = true;
        this._cbc.loadFullTableMetadata(event.node.data.tableMetadata.id)
            .then(tableMetadata => {
                this._createColumnNodes(event.node, tableMetadata);
                event.node.data.loaded = true;
                this.refreshView(false);
            })
            .finally(() => this.loading = false);
    }

    _onSelectedFolderChanged(): void {
        this.loading = true;

        this._cbc.loadFolderStructure(this._selectedFolderId)
            .then(() => {
                this._setSelectedFolder();
                this._createTablesTree();
            })
            .finally(() => this.loading = false);
    }

    async _onSelectNode(event: any): Promise<void> {
        this.loading = true;

        if (this._isTableTreeNode(event.node)) {
            await this._cbc.loadFullTableMetadata(event.node.data.tableMetadata.id);
            event.node.data.queryTable = await this._cbc.addQueryTable(event.node.data.tableMetadata.id);
            this._tree.onNodeExpand.emit(event);
        }

        if (this._isColumnTreeNode(event.node)) {
            const parentQueryTable = event.node.parent.data.queryTable;

            await this._cbc.addQueryTableColumn(event.node.data.columnMetadata.id, parentQueryTable?.id_original);

            if (parentQueryTable?.alias) {
                const queryTableColumn = parentQueryTable.columns.find(x =>
                    x.dbDocColumnId == event.node.data.columnId);

                if (queryTableColumn) {
                    event.node.data.queryTableColumn = queryTableColumn;
                }
            }
        }

        this.loading = false;
    }

    _onTableJoinsEditingDialogHide(): void {
        this._activeEditingJoin = null;
        this._editingQueryTableJoinRelations = null;
        this._editingJoinsQueryTable = null;
    }

    _onToQueryTableChange(): void {
        this._refreshActiveEditingJoinToQueryTableColumnOptions();
        this._refreshActiveEditingJoinToSourceColumnOptions();
    }

    _onToSourceDbDocTableChange(): void {
        this._activeEditingJoinToQueryTableColumnOptions = [];
        this._refreshActiveEditingJoinToSourceColumnOptions();
    }

    _onShowSourceTablesDialogClick(): void {
        this._sourceNodes = [];
        this._selectedSourceNodes = [];

        this._sourceTablesDialogVisible = true;

        this.sectionService.getQueryableTableSources().then((sources: IQueryableTableSource[]) => {
            this._sources = sources;
            const sourceNodes = [];

            sources.forEach(source => {
                const sourceNode = {
                    data: {
                        label: source.sourceName,
                        itemType: "source",
                        sourceCode: source.sourceCode
                    },
                    leaf: false,
                    expanded: true
                } as TreeNode;

                sourceNode.children = this._createSourceTablesNodes(source);
                sourceNodes.push(sourceNode);
            });

            this._sourceNodes = sourceNodes;            
        });        
    }

    async _onAddSourceTablesClick(): Promise<void> {
        this._sourceTablesDialogVisible = false;
        const selectedTableNodes = this._selectedSourceNodes.filter(x => x.data.itemType === "table");

        const selectedSources: IQueryableTableSource[] =
            this._sources.filter(x => selectedTableNodes.some(y => y.parent.data.sourceCode === x.sourceCode));

        selectedSources.forEach(source => {
            source.tables = source.tables.filter(x =>
                selectedTableNodes.some(y => y.parent.data.sourceCode === source.sourceCode && y.data.label === x.friendlyName));
        });

        await this._cbc.addQueryTablesFromSource(selectedSources);
    }

    _onUnselectNode(event: any): void {
        this.loading = true;

        if (this._isTableTreeNode(event.node)) {
            const queryTable = event.node.data.queryTable
                ?? this.queryBuilderComponent.query.queryTables.find(x => x.sourceTableId == event.node.data.tableMetadata.tableId);
            this._cbc.deleteQueryTable(queryTable)
                .then(() => {
                    event.node.data.queryTable = null;

                    this._nodes = event.node.data.alias || event.node.data.queryTable?.sourceCode === "form"
                        ? this._nodes.filter(x => x.data.id !== event.node.data.id)
                        : [...this._nodes];
                })
                .finally(() => this.loading = false);
        }

        if (this._isColumnTreeNode(event.node)) {
            if (event.node.data.columnMetadata.hidden) {
                this.messageService.add(Message.Warning("The unselected column was already marked as hidden in DB Documenting feature and therefore cannot be selected again."));
            }

            this._cbc.deleteQueryTableColumn(
                event.node.data.queryTableColumn
                ?? this.queryBuilderComponent.query.queryTables
                        .find(x => x.sourceTableId == event.node.parent.data.tableMetadata.tableId).columns
                        .find(x => x.sourceColumnId == event.node.data.columnId))
                .then(() => {
                    if (!event.node.parent.data.queryTable.columns.length) {
                        event.node.parent.data.queryTable = null;

                        this._nodes = event.node.parent.data.alias
                            ? this._nodes.filter(x => x.data.id !== event.node.parent.data.id)
                            : [...this._nodes];
                    }
                })
                .finally(() => this.loading = false);
        }
    }

    _saveActiveEditingJoin(): void {
        let promise: Promise<IQueryTableJoin>;
        if (this._activeEditingJoin.queryTableJoinId) {
            const savingJoinIndex = this.queryBuilderComponent.query.queryTableJoins.findIndex(x => x.id == this._activeEditingJoin.queryTableJoinId);
            const savingJoin = { ...this.queryBuilderComponent.query.queryTableJoins[savingJoinIndex] };
            if (savingJoin.fromQueryTableId === this._editingJoinsQueryTable.id) {
                savingJoin.fromQueryTableColumnId = this._activeEditingJoin.sourceQueryTableColumnId;
                savingJoin.fromDbDocColumnId = this._activeEditingJoin.sourceDbDocColumnId;
                savingJoin.toQueryTableId = this._activeEditingJoin.destinationQueryTableId;
                savingJoin.toDbDocTableId = this._activeEditingJoin.destinationDbDocTableId;
                savingJoin.toQueryTableColumnId = this._activeEditingJoin.destinationQueryTableColumnId;
                savingJoin.toDbDocColumnId = this._activeEditingJoin.destinationDbDocColumnId;
            } else {
                savingJoin.toQueryTableColumnId = this._activeEditingJoin.sourceQueryTableColumnId;
                savingJoin.toDbDocColumnId = this._activeEditingJoin.sourceDbDocColumnId;
                savingJoin.fromQueryTableId = this._activeEditingJoin.destinationQueryTableId;
                savingJoin.fromDbDocTableId = this._activeEditingJoin.destinationDbDocTableId;
                savingJoin.fromQueryTableColumnId = this._activeEditingJoin.destinationQueryTableColumnId;
                savingJoin.fromDbDocColumnId = this._activeEditingJoin.destinationDbDocColumnId;
            }

            promise = this._cbc.updateQueryTableJoin(savingJoin);
        } else {
            promise = this._cbc.addQueryTableJoin(<IQueryTableJoin> {
                queryId: this.queryBuilderComponent.query.id,
                fromQueryTableId: this._editingJoinsQueryTable.id,
                fromQueryTableColumnId: this._activeEditingJoin.sourceQueryTableColumnId,
                fromDbDocColumnId: this._activeEditingJoin.sourceDbDocColumnId,
                toQueryTableId: this._activeEditingJoin.destinationQueryTableId,
                toDbDocTableId: this._activeEditingJoin.destinationDbDocTableId,
                toQueryTableColumnId: this._activeEditingJoin.destinationQueryTableColumnId,
                toDbDocColumnId: this._activeEditingJoin.destinationDbDocColumnId
            });
        }

        promise.then(() => {
            this.refreshView();
            this._refreshEditingQueryTableJoinsRelations();
            this._activeEditingJoin = null;
        });
    }

    _startJoinEditing(joinRelation?: IJoinRelation): void {
        this._activeEditingJoin = joinRelation ?? <IJoinRelation> {};
        this._activeEditingJoinFromSideExistingQueryTableColumn = true;
        this._activeEditingJoinToSideExistingQueryTable = true;
        this._activeEditingJoinToSideExistingQueryTableColumn = true;

        this._activeEditingJoinFromQueryTableColumnOptions = this._editingJoinsQueryTable.columns
            .map(x => <SelectItem> {
                label: this._cbc.queryColumnsMetadataMap[x.id]?.staticData?.columnName,
                value: x.id
            });

        this._activeEditingJoinFromSourceColumnOptions = this._selectedFolder.tables
            .find(x => x.tableId === this._editingJoinsQueryTable.sourceTableId).columns
            .filter(x => this._editingJoinsQueryTable.columns.every(y => y.sourceColumnId !== x.columnId))
            .map(x => <SelectItem> {
                label: x.staticData.columnName,
                value: x.columnId
            });

        this._activeEditingJoinToQueryTableOptions = this.queryBuilderComponent.query.queryTables
            .filter(x => x.id != this._editingJoinsQueryTable.id)
            .map(x => <SelectItem> {
                label: x.sourceCode === "form"
                    ? x.sourceTableId
                    : (this._cbc.queryTablesMetadataMap[x.id]?.staticData?.tableName + (x.alias ? ` as ${x.alias}` : "")),
                value: x.id
            });

        this._activeEditingJoinToSourceTableOptions = this._selectedFolder.tables
            .filter(x => this.queryBuilderComponent.query.queryTables.every(y => y.sourceTableId !== x.tableId))
            .map(x => <SelectItem> {
                label: x.staticData.tableName,
                value: x.tableId
            });

        this._refreshActiveEditingJoinToQueryTableColumnOptions();
        this._refreshActiveEditingJoinToSourceColumnOptions();
    }

    _startJoinDeleting(joinRelation: IJoinRelation): void {
        this.confirmationService.confirm({
            message: "Are you sure you want to delete this JOIN?",
            accept: () => this._cbc.deleteQueryTableJoin(this.queryBuilderComponent.query.queryTableJoins
                .find(x => x.id === joinRelation.queryTableJoinId))
                .then(() => {
                    this._createTablesTree();
                    this.refreshView();
                    this._refreshEditingQueryTableJoinsRelations();
                    if (this._editingJoinsQueryTable.onlyForJoin && !this._editingQueryTableJoinRelations.length) {
                        this._tableJoinsEditingDialogVisible = false;
                    }
                })
        });
    }


    private _buildSelectedNodes(): void {
        this._selectedNodes = [];

        if (!this._nodes?.length || !this.queryBuilderComponent.query?.queryTables?.length) return;

        this.queryBuilderComponent.query.queryTables.forEach(selectedQueryTable => {
            if (selectedQueryTable.sourceCode === "form") {
                const formTableNode = this._nodes.find(x => x.data.queryTable.id == selectedQueryTable.id);
                if (formTableNode) {
                    this._selectedNodes.push(formTableNode);
                    formTableNode.children.forEach(selectedTableColumnNode => {
                        if (selectedQueryTable.columns.some(x => x.sourceColumnId == selectedTableColumnNode.data.columnId)) {
                            this._selectedNodes.push(selectedTableColumnNode);
                        }
                    });
                }
            } else {
                if (selectedQueryTable.alias) {
                    const aliasTableNode = this._nodes.find(x =>
                        x.data.tableMetadata.tableId == selectedQueryTable.sourceTableId && x.data.alias == selectedQueryTable.alias);
                    if (aliasTableNode) {
                        this._selectedNodes.push(aliasTableNode);
                        aliasTableNode.children.forEach(selectedTableColumnNode => {
                            if (selectedQueryTable.columns.some(x => x.sourceColumnId == selectedTableColumnNode.data.columnId)) {
                                this._selectedNodes.push(selectedTableColumnNode);
                            }
                        });
                    }
                } else {
                    const selectedTableNode = this._nodes.find(x => x.data.tableMetadata?.tableId == selectedQueryTable.sourceTableId);
                    if (selectedTableNode && !selectedQueryTable.onlyForJoin) {
                        this._selectedNodes.push(selectedTableNode);
                        selectedTableNode.children.forEach(selectedTableColumnNode => {
                            if (selectedQueryTable.columns.some(x => !x.onlyForJoin && x.sourceColumnId == selectedTableColumnNode.data.columnId)) {
                                this._selectedNodes.push(selectedTableColumnNode);
                            }
                        });
                    }
                }
            }
        });
    }

    private _createTablesTree(): void {
        if (!this._selectedFolder.tables?.length) return;

        const tablesList = [];

        // Forms
        this.queryBuilderComponent.query.queryTables
            .filter(x => x.sourceCode === "form")
            .forEach(formQueryTable => {
                const tableNode = {
                    data: {
                        id: `form-${formQueryTable.sourceTableId}`,
                        label: formQueryTable.sourceTableId,
                        itemType: "table",
                        queryTable: formQueryTable
                    },
                    leaf: false,
                    expanded: false
                } as TreeNode;

                const columnNodes = [];
                formQueryTable.columns.forEach(formQueryTableColumn => {
                    const columnNode = {
                        data: {
                            label: formQueryTableColumn.sourceColumnId,
                            columnId: formQueryTableColumn.sourceColumnId,
                            isPrimaryKey: false,
                            isForeignKey: false,
                            itemType: "column",
                            queryTableColumn: formQueryTableColumn
                        },
                        parent: tableNode,
                        leaf: true
                    } as TreeNode;
                    columnNodes.push(columnNode);
                });
                columnNodes.sort((a, b) => a.data.label.localeCompare(b.data.label));
                tableNode.children = columnNodes;

                tablesList.push(tableNode);
        });

        tablesList.sort((a, b) => a.data.label.localeCompare(b.data.label));

        // Tables
        this._selectedFolder.tables.forEach(tableMetadata => {
            const tableTreeNodeId = `${tableMetadata.tableId}-${tableMetadata.id}`;
            const tableTreeNodeExpanded = this._nodes.find(x => x.data.id === tableTreeNodeId)?.expanded;
            const tableTreeNode = {
                data: {
                    id: tableTreeNodeId,
                    label: tableMetadata.staticData ? tableMetadata.staticData.tableName : "<No static data found>",
                    tableMetadata: tableMetadata,
                    itemType: "table",
                    queryTable: this.queryBuilderComponent.query.queryTables
                        .find(x => !x.alias && x.sourceTableId === tableMetadata.tableId)
                },
                leaf: false,
                expanded: tableTreeNodeExpanded,
                children: []
            } as TreeNode;

            if (tableTreeNodeExpanded) {
                this._createColumnNodes(tableTreeNode, tableMetadata);
            }

            tablesList.push(tableTreeNode);

            this.queryBuilderComponent.query.queryTables
                .filter(x => x.alias && x.sourceTableId === tableMetadata.tableId)
                .forEach(x => {
                    const aliasTableTreeNodeId = `${x.alias}-${tableMetadata.tableId}-${tableMetadata.id}`;
                    const aliasTableTreeNodeExpanded = this._nodes.find(x => x.data.id === tableTreeNodeId)?.expanded;
                    const aliasTableTreeNode = <TreeNode> {
                        data: {
                            id: aliasTableTreeNodeId,
                            label: tableMetadata.staticData ? tableMetadata.staticData.tableName : "<No static data found>",
                            tableMetadata: tableMetadata,
                            itemType: "table",
                            alias: x.alias,
                            queryTable: x
                        },
                        expanded: tableTreeNodeExpanded,
                        leaf: false,
                        children: []
                    };

                    if (aliasTableTreeNodeExpanded) {
                        this._createColumnNodes(aliasTableTreeNode, tableMetadata);
                    }

                    tablesList.push(aliasTableTreeNode);
                });
        });

        this._nodes = tablesList;
    }

    private _createColumnNodes(tableTreeNode: TreeNode, tableMetadata: ITableMetadata): void {
        const l = tableMetadata.columns.map(columnMetadata => {
            const queryTableColumn = tableTreeNode.data.queryTable?.columns
                .find((x: IQueryTableColumn) => x.sourceColumnId == columnMetadata.columnId);

            const result = <TreeNode> {
                data: {
                    label: columnMetadata.staticData ? columnMetadata.staticData.columnName : "<No static data found>",
                    columnMetadata: columnMetadata,
                    columnId: columnMetadata.columnId,
                    dataType: columnMetadata.staticData?.clrTypeGroup,
                    isPrimaryKey: columnMetadata.staticData?.isPrimaryKey,
                    isForeignKey: columnMetadata.staticData?.isForeignKey,
                    itemType: "column",
                    queryTableColumn: queryTableColumn
                },
                parent: tableTreeNode,
                leaf: true
            };

            result.data.id = tableTreeNode.data.alias
                ? `${tableTreeNode.data.alias}-${columnMetadata.columnId}-${columnMetadata.id}`
                : `${columnMetadata.columnId}-${columnMetadata.id}`;

            return result;
        });

        l.sort((a, b) => {
            if (a.data.isPrimaryKey && !b.data.isPrimaryKey) return -1;
            if (!a.data.isPrimaryKey && b.data.isPrimaryKey) return 1;
            return a.data.label.localeCompare(b.data.label);
        });

        tableTreeNode.children = l;
    }

    private _createSourceTablesNodes(source: IQueryableTableSource) {
        const tablesList = [];

        // Tables
        source.tables.forEach(table => {
            const tableNode = {
                data: {
                    label: table.friendlyName,
                    itemType: "table"
                },
                leaf: false,
                expanded: false
            } as TreeNode;

            // Table columns
            const columnNodes = [];
            table.schemaColumns.forEach(column => {
                const columnNode = {
                    data: {
                        label: column.columnName,
                        itemType: "column"
                    },
                    leaf: true,
                    parent: table
                } as TreeNode;
                columnNodes.push(columnNode);
            });
            columnNodes.sort((a, b) => a.data.label.localeCompare(b.data.label));
            tableNode.children = columnNodes;


            tablesList.push(tableNode);
        });

        tablesList.sort((a, b) => a.data.label.localeCompare(b.data.label));

        return tablesList;
    }

    private async _init(): Promise<void> {
        this.loading = true;

        this._folderOptions = this._cbc.folders.map(x => <SelectItem> { label: x.name, value: x.id });
        this.initSelectedFolderId();
        await this._cbc.loadFolderStructure(this._selectedFolderId);
        this._setSelectedFolder();
        this.refreshView();

        this.loading = false;
    }

    private initSelectedFolderId(): void {
        if (!this._cbc.folders?.length) return;

        this._selectedFolderId = this.queryBuilderComponent.query?.dbDocFolderId
            ?? this._cbc.folders.find(x => x.name === ReportService.DefaultFolderName)?.id
                ?? this._cbc.folders[0]?.id;
    }

    private _refreshActiveEditingJoinToQueryTableColumnOptions(): void {
        this._activeEditingJoinToQueryTableColumnOptions = this._activeEditingJoin.destinationQueryTableId
            ? this.queryBuilderComponent.query.queryTables
                .reduce((a, b) => a.concat(b.columns), [])
                .filter((x: IQueryTableColumn) => x.queryTableId === this._activeEditingJoin.destinationQueryTableId)
                .map(x => <SelectItem> {
                    label: this._cbc.queryColumnsMetadataMap[x.id]?.staticData?.columnName,
                    value: x.id
                })
            : [];
    }

    private async _refreshActiveEditingJoinToSourceColumnOptions(): Promise<void> {
        let tableMetadata: ITableMetadata;
        if (this._activeEditingJoin.destinationDbDocTableId) {
            tableMetadata = this._selectedFolder.tables.find(x => x.tableId === this._activeEditingJoin.destinationDbDocTableId);
            await this._cbc.loadFullTableMetadata(tableMetadata.id);
            tableMetadata = this._selectedFolder.tables.find(x => x.tableId === this._activeEditingJoin.destinationDbDocTableId);
        } else {
            if (!this._activeEditingJoin.destinationQueryTableId) {
                this._activeEditingJoinToSourceColumnOptions = [];
                return;
            }

            tableMetadata = this._selectedFolder.tables.find(x => x.tableId === this.queryBuilderComponent.query.queryTables
                .find(y => y.id === this._activeEditingJoin.destinationQueryTableId)?.sourceTableId);
        }

        this._activeEditingJoinToSourceColumnOptions = tableMetadata
            ? tableMetadata.columns
                .filter(x => this._activeEditingJoinToQueryTableColumnOptions
                    .map(y => this._cbc.queryColumnsMetadataMap[y.value].columnId).every(y => y !== x.columnId))
                .map(x => <SelectItem> {
                    label: x.staticData.columnName,
                    value: x.columnId
                })
            : [];
    }

    private _refreshEditingQueryTableJoinsRelations(): void {
        this._editingQueryTableJoinRelations = [];
        this.queryBuilderComponent.query.queryTableJoins.forEach(x => {
            if (x.fromQueryTableId == this._editingJoinsQueryTable.id) {
                this._editingQueryTableJoinRelations.push(<IJoinRelation> {
                    queryTableJoinId: x.id,
                    sourceQueryTableColumnId: x.fromQueryTableColumnId,
                    destinationQueryTable: x.toQueryTable,
                    destinationQueryTableId: x.toQueryTableId,
                    destinationQueryTableColumnId: x.toQueryTableColumnId
                })
            }

            if (x.toQueryTableId == this._editingJoinsQueryTable.id) {
                this._editingQueryTableJoinRelations.push(<IJoinRelation> {
                    queryTableJoinId: x.id,
                    sourceQueryTableColumnId: x.toQueryTableColumnId,
                    destinationQueryTable: x.fromQueryTable,
                    destinationQueryTableId: x.fromQueryTableId,
                    destinationQueryTableColumnId: x.fromQueryTableColumnId
                })
            }
        });
    }

    private _setHiddenNodes(): void {
        this._nodes.forEach(tableNode => {
            tableNode.children.forEach(columnNode => {
                if (columnNode.data.columnMetadata?.hidden) {
                    if (this._selectedNodes.every(x =>
                        x.data.columnMetadata.id !== columnNode.data.columnMetadata.id)) {
                        columnNode.styleClass = columnNode.styleClass == "selected-hidden-node"
                            ? "selected-hidden-node hidden-node" : "hidden-node";
                        columnNode.selectable = false;
                    } else {
                        columnNode.styleClass = "selected-hidden-node";
                    }
                }
            });
        });
    }

    private _isTableTreeNode(node) {
        return node.data.itemType == "table";
    }

    private _isColumnTreeNode(node) {
        return node.data.itemType == "column";
    }

    private _setSelectedFolder(): void {
        if (!this._cbc.folders?.length) return;

        this._selectedFolder = this._cbc.folders.find(x => x.id == this._selectedFolderId);
    }    
}