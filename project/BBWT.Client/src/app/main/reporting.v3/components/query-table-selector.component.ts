import {Component, EventEmitter, Input, Output} from "@angular/core";
import {MessageService, SelectItem, TreeNode} from "primeng/api";
import {TableSetService} from "../api/table-set.service";
import {ITableSet, ITableSetColumn, ITableSetFolder, ITableSetFolderInfo, ITableSetTable} from "@main/reporting.v3/core/reporting-models";
import {TreeTableNodeExpandEvent} from "primeng/treetable";
import {DropdownChangeEvent} from "primeng/dropdown";
import {IHash} from "@bbwt/interfaces";
import {Message} from "@bbwt/classes";


@Component({
    selector: "query-table-selector",
    templateUrl: "./query-table-selector.component.html",
    styleUrls: ["./query-table-selector.component.scss"]
})
export class QueryTableSelectorComponent {
    // Table-set settings.
    folders: ITableSetFolder[] = [];
    folderTrees: IHash<TreeNode> = {};

    // General settings.
    folderOptions: SelectItem[] = [];
    sourceCodeLabels: IHash<string> = {dbdoc: "Database Fields", forms: "Forms", formGrids: "Form Grids"};

    private _tableSet: ITableSet = {folderId: null} as ITableSet;

    @Input() collapsed: boolean;
    @Input() tablesOnly: boolean;
    @Input() tableSelection: ITableSetTable[] = [];
    @Input() columnSelection: ITableSetColumn[] = [];
    @Input() tableSelectionMode: "single" | "multiple";
    @Input() columnSelectionMode: "single" | "multiple";
    @Output() onTableClick: EventEmitter<any> = new EventEmitter<any>();
    @Output() onColumnClick: EventEmitter<any> = new EventEmitter<any>();
    @Output() tableSelectionChange: EventEmitter<ITableSetTable[]> = new EventEmitter<ITableSetTable[]>();
    @Output() columnSelectionChange: EventEmitter<ITableSetColumn[]> = new EventEmitter<ITableSetColumn[]>();
    @Output() onFolderChange: EventEmitter<ITableSetFolderInfo> = new EventEmitter<ITableSetFolderInfo>();

    constructor(private tableSetService: TableSetService,
                private messageService: MessageService) {
    }


    @Input() set tableSet(value: ITableSet) {
        this._tableSet = value ?? {folderId: null} as ITableSet;
        this.refreshTableSet().then();
    }

    get tableSet(): ITableSet {
        return this._tableSet;
    }

    @Input() set tableSetId(value: string) {
        if (!!value && value === this.tableSetId) return;
        if (!!value) {
            this.tableSetService.get(value)
                .then(tableSet => this.tableSet = tableSet);
        } else this.tableSet = null;
    }

    get tableSetId(): string {
        return this._tableSet?.id;
    }

    @Input() set folderId(value: string) {
        this._tableSet ??= {folderId: null} as ITableSet;
        this._tableSet.folderId = value;
        this.refreshTableSet().then();
    }

    get folderId(): string {
        return this._tableSet?.folderId;
    }


    // Refreshing methods.
    private async refreshSettings(): Promise<void> {
        this.folders = await this.tableSetService.getFolders() ?? [];

        this.folderOptions = this.folders.map(folder => <SelectItem>{
            label: folder.name,
            value: folder.id
        });

        this.folders.forEach(folder =>
            this.folderTrees[folder.id] = {
                type: "folder",
                leaf: false,
                children: [],
                expanded: true,
                label: this.sourceCodeLabels[folder.sourceCode],
            });
    }

    private async refreshTableSet(): Promise<void> {
        // Refresh settings if they are not loaded yet.
        if (!this.folders.length) await this.refreshSettings();

        // If there are no folders, return.
        if (!this.folders.length) return;

        // If there is no selected folder, auto-select the first valid folder.
        // Load selected folder tables.
        this.refreshSelectedFolder(this.tableSet.folderId ?? this.folders[0].id)
    }

    public refreshSelectedFolder(folderId: string): void {
        // If there is no folder id, or resources are already loaded and the folder id didn't change, return.
        if (!folderId || this.isSelectedFolder(folderId)) return;

        // Get the corresponding folder.
        const folder = this.folders?.find(folder => folder.id === folderId);
        if (!folder) return;

        this.tableSet.folderId = folderId;
        this.tableSet.folderSourceCode = folder.sourceCode;

        this.refreshTables(folderId).then();
        this.folderTrees[folderId].expanded = true;
    }

    public async refreshTables(folderId: string): Promise<ITableSetTable[]> {
        // If folder is not specified, use selected folder instead.
        if (!folderId) return;

        // Get the corresponding folder.
        const folder: ITableSetFolder = this.folders.find(folder => folder.id === folderId);
        const code: string = folder?.sourceCode;
        if (!folder) return;

        // Load folder tables if they weren't loaded yet.
        if (!this.folderTrees[folderId].children?.length) {
            const tables: ITableSetTable[] = await this.tableSetService.getFolderTables(code, folderId);
            if (!tables) return;

            this.folderTrees[folderId].children = tables.map(table => <TreeNode>{
                data: {...table, sourceCode: code, folderId: folderId},
                type: "table",
                children: [],
                leaf: false,
                label: this.getDisplayableLabel(table.name)
            });

            // Re-assign tables tree to refresh table selector.
            this.folderTrees[folderId] = {...this.folderTrees[folderId]};
        }

        // Get all tables related to this folder.
        const tables: ITableSetTable[] = this.folderTrees[folderId].children.map(table => table.data);

        const folderInfo: ITableSetFolderInfo = {
            id: folderId,
            tables: tables,
            sourceCode: code,
        };

        // Emit and return corresponding folder info.
        this.onFolderChange.emit(folderInfo);

        return folderInfo.tables;
    }

    public async refreshTable(folderId: string, sourceCode: string, tableId: string, parentTableId?: string): Promise<ITableSetColumn[]> {
        // If folder is not specified, use selected folder instead.
        if (!sourceCode || !tableId || !folderId) return;

        // If node is not passed as parameter, find the node by tableId.
        const node: TreeNode = !parentTableId
            ? this.folderTrees[folderId]?.children
                ?.find(table => table.data?.id === tableId)
            : this.folderTrees[folderId]?.children
                ?.find(table => table.data?.id === parentTableId)?.children
                ?.find(table => table.data?.id === tableId);
        if (!node) return;

        // Refresh table set if possible.
        if (!node.children?.length) {
            const table: ITableSetTable = await this.tableSetService.getTable(sourceCode, folderId, tableId, parentTableId);
            if (!table) return;

            const children: TreeNode[] = table.children.map(child => <TreeNode>{
                data: {...child, sourceCode: sourceCode, folderId: folderId, parentTableId: node.data?.id},
                leaf: false,
                type: "table",
                children: [],
                label: this.getDisplayableLabel(child.name),
            });

            const columns: TreeNode[] = table.columns.map(column => <TreeNode>{
                data: {...column, tableId: node.data?.id, table: node.data},
                leaf: true,
                type: "column",
                label: this.getDisplayableLabel(column.name),
            });

            node.children = [...children, ...columns];

            // Re-assign tables tree to refresh table selector.
            this.folderTrees[folderId] = {...this.folderTrees[folderId]};
        }

        // Return all columns related to this table
        return node.children.filter(child => child.type === "column").map(column => column.data);
    }

    // Auxiliary methods.
    private getDisplayableLabel(label: string): string {
        if (!label) return "";
        return label.length <= 30 ? label : `${label.slice(0, 30)}...`;
    }

    private isSelectedFolder(folderId: string): boolean {
        return folderId === this.tableSet?.folderId && !!this.folderTrees[folderId]?.children?.length;
    }

    private isSelectedTable(tableId: string): boolean {
        return !!this.tableSelection?.some(table => table.id === tableId);
    }

    private isSelectedColumn(columnId: string): boolean {
        return !!this.columnSelection?.some(column => column.id === columnId);
    }

    protected nodeVisible(node: TreeNode): boolean {
        return !this.tablesOnly || node.type === "table";
    }

    protected supportsNesting(node: TreeNode, level: number): boolean {
        return !this.tablesOnly || level === 1 && node.type === "table" && node.data?.sourceCode === "forms";
    }

    protected getNodeClass(node: TreeNode): string {
        if (!node) return;

        let styleClass: string = `tree-node ${node.type}-node`;
        if (this.tablesOnly) styleClass += " tables-only";

        if (node.type === "table" && this.isSelectedTable(node.data?.id) ||
            node.type === "column" && this.isSelectedColumn(node.data?.id)) {
            styleClass += " node-selected";
        }

        return styleClass;
    }

    protected getNodeIconClass(node: TreeNode): string {
        if (node.type !== "column") return null;

        return node.data?.isPrimaryKey
            ? "vpn_key"
            : node.data?.isForeignKey
                ? "insert_link"
                : "view_column";
    }

    protected onSelectedFolderChanged(event: DropdownChangeEvent): void {
        this.refreshSelectedFolder(event.value);
    }

    protected onNodeExpand(event: TreeTableNodeExpandEvent): void {
        const table: ITableSetTable = event?.node?.data;
        if (!table) return;
        this.refreshTable(table.folderId, table.sourceCode, table.id, table.parentTableId).then();
    }

    protected onNodeClick(_: MouseEvent, node: TreeNode): void {
        switch (node?.type) {
            case "table":
                this.onTableSelectionChange(node.data);
                this.onTableClick.emit(node.data);
                break;
            case "column":
                this.onColumnSelectionChange(node.data);
                this.onColumnClick.emit(node.data);
                break;
        }
    }

    protected onTableSelectionChange(table: ITableSetTable): void {
        if (!this.tableSelectionMode || !table?.id) return;

        // If this table is not selected, select it.
        if (!this.isSelectedTable(table.id)) {
            // If selection mode is "single", empty the list of selections first
            // (in single mode selection only one table can be selected at a time).
            if (this.tableSelectionMode === "single") {
                this.tableSelection = [];
                this.columnSelection = [];
            }
            this.tableSelection.push(table);
        } else {
            // If this table is selected, deselect it.
            this.tableSelection = this.tableSelection.filter(selection => selection.id !== table.id);
            this.columnSelection = this.columnSelection.filter(selection => selection.tableId !== table.id)
        }

        this.tableSelectionChange.emit(this.tableSelection);
    }

    protected onColumnSelectionChange(column: ITableSetColumn): void {
        if (!this.columnSelectionMode || !column?.id || !column?.table?.id) return;

        // If this column is not selected, select it.
        if (!this.isSelectedColumn(column.id)) {
            // If the table of this column is not selected, select it.
            if (!this.isSelectedTable(column.table.id)) this.onTableSelectionChange(column.table);

            // If selection mode is "single", empty the list of selections first
            // (in single mode selection only one column can be selected at a time).
            if (this.columnSelectionMode === "single") {
                this.columnSelection = [];
            }
            this.columnSelection.push(column);
        } else {
            // If this column is selected, deselect it.
            this.columnSelection = this.columnSelection.filter(selection => selection.id !== column.id);
        }

        this.columnSelectionChange.emit(this.columnSelection);
    }

    // Edition methods.
    async save(): Promise<string> {
        if (!this.tableSet?.folderId) return;

        const editionFunc = (tableSet: ITableSet): Promise<ITableSet> =>
            !this.tableSetId
                ? this.tableSetService.create(tableSet)
                : this.tableSetService.update(tableSet.id, tableSet);

        return this.editTableSet(editionFunc);
    }

    private async editTableSet(editionFunc: (tableSet: ITableSet) => Promise<ITableSet>): Promise<string> {
        const tableSet: ITableSet = this.tableSet;

        // Try to edit the table set. Restore table set if edition fails.
        const build: ITableSet = await editionFunc(tableSet).catch(error => {
            this.messageService.add(Message.Error(`There was an error saving entity:\n${error.error}`));
            return null;
        });

        // Update the build only if there was no error.
        if (build != null) this.tableSet = build;

        return build?.id ?? null;
    }
}