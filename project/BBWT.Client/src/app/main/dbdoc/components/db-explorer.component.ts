import { Component, OnInit, ViewChild } from "@angular/core";
import { NgForm } from "@angular/forms";

import { ConfirmationService, SelectItem, TreeDragDropService, TreeNode } from "primeng/api";
import "reflect-metadata";

import { downloadFileFromBlob } from "@bbwt/utils";
import { DatabaseType, IAddDatabaseRequest, ICopyTableMetadataToFolderRequest, IFolder, ITableDataViewSettings, ITableMetadata, getDatabaseSourceTypeOptions } from "../dbdoc-models";
import { DbDocService } from "../dbdoc.service";
import { DbDocTableDataService } from "../dbdoc-table-data.service";
import { DbDocFolderService } from "../dbdoc-folder.service";
import { IHash } from "../../../bbwt/interfaces";


@Component({
    templateUrl: "./db-explorer.component.html",
    styleUrls: ["./db-explorer.component.scss"],
    providers: [TreeDragDropService]
})
export class DbExplorerComponent implements OnInit {
    folderNodes: TreeNode[] = [];
    copyToNodes: TreeNode[] = [];
    selectedNode: TreeNode;

    loading: boolean;
    copyToFoldersSelectItems = [];

    tableDataViewSettings: ITableDataViewSettings;

    private folders: IFolder[];
    private folderDetailsLoaded: IHash<boolean> = {};
    folderOwnerTypes: string[];

    nodeDataTypeFolder = "folder";
    nodeDataTypeViews = "views";
    nodeDataTypeTable = "table";
    nodeDataTypeColumn = "column";

    @ViewChild("form", { static: false }) form: NgForm;

    // Create folder form's properties
    newFolder: IFolder = <any>{};
    loadFolderFromDatabase: boolean;
    connectDbConnectionString: string;
    connectDbDatabaseType: DatabaseType;
    databaseSourceTypeOptions = getDatabaseSourceTypeOptions();
    createFolderDialogVisible = false;
    requestingCreateFolder = false;
    requestingRefreshFolder = false;


    constructor(
        private confirmationService: ConfirmationService,
        private dbDocFolderService: DbDocFolderService,
        private dbDocService: DbDocService,
        private dbDocTableDataService: DbDocTableDataService
    ) {        
    }

    ngOnInit() {
        this.loadFolders();
        this.dbDocTableDataService.GetTableDataViewSettings().then(x => this.tableDataViewSettings = x);
        this.dbDocFolderService.getFolderOwnerTypes().then(x => this.folderOwnerTypes = x);
    }

    private loadFolders(): void {
        this.loading = true;

        this.dbDocFolderService.getFolders().then(folders => {
            this.folders = folders;
            this.loadFolderNodesTree(folders);
        }).finally(() => this.loading = false);
    }

    private loadFolder(folderId: string): Promise<IFolder> {
        this.loading = true;

        return this.dbDocFolderService.getFolder(folderId).then(folder => {
            this.updateFolders([folder]);
            return Promise.resolve(folder);
        })
            .finally(() => this.loading = false );
    }

    private checkLoadFolder(folderId: string) {
        // Loading folder details just once (details can be heavy)
        if (!this.folderDetailsLoaded[folderId]) {
            this.loadFolder(folderId)
                .then(folder => this.folderDetailsLoaded[folderId] = true);
        }
    }

    onNodeExpand($event) {
        const item = $event.node.data;
        if (item.itemType == this.nodeDataTypeFolder) {
            this.checkLoadFolder(item.itemData.id);
        }
    }

    onNodeDrop(event: any): void {
        this.copyTableMetadataToFolder(event.dragNode.data.itemData.id, event.dropNode.key);
    }

    onFolderChanged(folder: IFolder): void {
        this.updateFolders([folder]);
    }

    updateFolders(updatedFolders: IFolder[]) {
        if (!updatedFolders.length) return;

        const nodes = [...this.folderNodes];

        updatedFolders.forEach(updatedFolder => {
            const index = this.folders.findIndex(x => x.id === updatedFolder.id);

            if (index >= 0) {
                this.folders[index] = updatedFolder;
                const folderNode = this.createFolderNodesTree(updatedFolder);
                const nodeIndex = nodes.findIndex(x => x.key === updatedFolder.id);
                folderNode.expanded = nodes[nodeIndex].expanded;
                nodes[nodeIndex] = folderNode;
            }
        });

        this.updateFoldersTreeNodes(nodes);
    }

    startFolderCreation(): void {
        this.form.reset();

        this.loadFolderFromDatabase = false;
        this.connectDbConnectionString = null;

        this.createFolderDialogVisible = true;
    }

    // TODO: we should handle slow DB scanning and for user to have an ability to cancel folder creating,
    // also avoid duplicate Create clicks. Also add a spinner to take into account slow DB scanning cases
    createFolder(): void {
        let createFolderFunc: Promise<IFolder>;

        if (this.loadFolderFromDatabase) {
            createFolderFunc = this.dbDocFolderService.createFolderFromDb(<IAddDatabaseRequest>{
                folderName: this.newFolder.name,
                folderDescription: this.newFolder.description,
                connectionString: this.connectDbConnectionString,
                databaseType: this.connectDbDatabaseType
            });
        } else {
            createFolderFunc = this.dbDocFolderService.createFolder(this.newFolder);
        }

        this.loading = true;
        this.requestingCreateFolder = true;

        createFolderFunc.then(folder => {
            this.folders.push(folder);

            const nodes = [...this.folderNodes];
            const newFolderNode = this.createFolderNodesTree(folder);
            nodes.push(newFolderNode);

            this.updateFoldersTreeNodes(nodes);

            this.selectedNode = newFolderNode;
            this.selectedNode.expanded = true;

            this.createFolderDialogVisible = false;
        })
            .finally(() => {
                this.loading = false;
                this.requestingCreateFolder = false;
            });
    }

    updateFoldersTreeNodes(nodes: TreeNode[]) {
        // Sorting folder nodes
        nodes.sort((a, b) => {
            // Showing protected folders on top (reason - "All Tables" of main DB)
            if (a.data.protected) return -1; 
            if (b.data.protected) return 1;
            const folderA = <IFolder>a.data.itemData;
            const folderB = <IFolder>b.data.itemData;
            if (folderA.isSourceFolder == folderB.isSourceFolder) {
                return a.label.localeCompare(b.label);
            } else {
                return folderA.isSourceFolder ? -1 : 1;
            }
        });

        // By changing reference of the root node we solve PrimeNG tree's refreshing issue
        this.folderNodes = [...nodes];

        this.updateCopyToFolders();
    }

    startFolderDeleting(): void {
        const deleteFolderId = this.selectedNode.data.itemData.id;

        const deleteFunc = () => {
            this.loading = true;

            this.dbDocFolderService.deleteFolder(deleteFolderId)
                .then((affectedFolders) => {
                    this.folders = this.folders.filter(x => x.id !== deleteFolderId);

                    const nodes = this.folderNodes.filter(x => x.key !== deleteFolderId);
                    this.updateFoldersTreeNodes(nodes);

                    this.updateFolders(affectedFolders);
                    this.selectedNode = null;
                })
                .finally(() => this.loading = false);
        }

        this.confirmationService.confirm({
            message:
                "Are you sure you want to delete this folder? All related tables' metadata will be deleted as well.",
            accept: () => deleteFunc()
        });
    }

    startTableMetadataDeleting(): void {
        this.confirmationService.confirm({
            message: "Are you sure you want to delete this table metadata?",
            accept: () => this.dbDocService.deleteTableMetadata(this.selectedNode.data.itemData.id)
                .then(folder => this.updateFolders([folder]))
        });
    }

    onDeleteTreeNodeClick() {
        switch (this.selectedNode.data.itemType) {
            case this.nodeDataTypeFolder: this.startFolderDeleting(); return;
            case this.nodeDataTypeTable: this.startTableMetadataDeleting(); return;
        }
    }

    onSyncFolderFromDbClick() {
        const folderId = this.selectedNode.data.itemData.id;

        this.requestingRefreshFolder = true;
        this.loading = true;

        this.dbDocFolderService.syncFolderFromDb(folderId)
            .then(folder => {
                this.updateFolders([folder]);
            })
            .finally(() => {
                this.loading = false;
                this.requestingRefreshFolder = false;
            });
    }

    exportAnonymizationXml(): void {
        const folderId = this.selectedNode.data.itemData.id;

        this.dbDocService.exportAnonymizationXml(folderId).then(data => {
            downloadFileFromBlob(new Blob([data], { type: "text/xml" }), "anonymization.xml");
        });
    }

    selectNodeByTableMetadataId(tableMetadataId: number | string): void {
        this.folderNodes.forEach(folderNode => {
            folderNode.children.forEach(tableMetadataNode => {
                if (tableMetadataNode.data.itemData.id == tableMetadataId) {
                    this.selectedNode = tableMetadataNode;
                }
            });
        });
    }

    selectNodeByColumnMetadataId(columnMetadataId: number | string): void {
        this.folderNodes.forEach(folderNode => {
            folderNode.children.forEach(tableMetadataNode => {
                tableMetadataNode.children.forEach(columnMetadataNode => {
                    if (columnMetadataNode.data.itemData.id == columnMetadataId) {
                        this.selectedNode = columnMetadataNode;
                    }
                });
            });
        });
    }

    findTableMetadataInFolder(uniqueTableId: string, folderId: string): ITableMetadata {
        return this.folders
            .find(x => x.id == folderId)?.tables
            .find(x => x.tableId == uniqueTableId);
    }

    isTableMetadataExistsInFolder(uniqueTableId: string, folderId: string): boolean {
        const folder = this.folders.find(x => x.id == folderId);

        if (!folder) return false;

        return folder.tables.some(x => x.tableId == uniqueTableId);
    }

    getColumnMetadataContainingFolder(columnMetadataId: number): IFolder {
        for (const folder of this.folders) {
            for (const tableMetadata of folder.tables) {
                for (const columnMetadata of tableMetadata.columns) {
                    if (columnMetadata.id == columnMetadataId) {
                        return folder;
                    }
                }
            }
        }

        return null;
    }

    setSelectedNodeItemData(data: any): void {
        this.selectedNode.data.itemData = data;
    }

    private loadFolderNodesTree(folders: IFolder[]): void {
        const nodes = [];
        folders.forEach(folder => {
            const folderNode = this.createFolderNodesTree(folder);
            nodes.push(folderNode);
        });

        if (nodes.length == 1) {
            this.checkLoadFolder(folders[0].id);
            nodes[0].expanded = true;
        }

        this.updateFoldersTreeNodes(nodes);
    }

    private createFolderNodesTree(folder: IFolder): any {
        // creating folder node
        const folderNode = this.createFolderNode(folder);

        //#region creating table nodes
        const tables = folder.tables.filter(x => !x.staticData?.isView);

        tables.forEach(tableMetadata => {
            const node = this.createTableNodesTree(folder, tableMetadata);
            if (node) {
                folderNode.children.push(node);
            }
        });
        folderNode.children.sort((a, b) => a.label.localeCompare(b.label));
        //#endregion

        //#region creating view nodes
        const views = folder.tables.filter(x => x.staticData?.isView);        

        if (views.length > 0) {
            const viewsParentNode = this.createViewsNode(folder);

            folderNode.children.splice(0, 0, viewsParentNode);

            views.forEach(tableMetadata => {
                const node = this.createTableNodesTree(folder, tableMetadata);
                if (node) {
                    viewsParentNode.children.push(node);
                }
            });
        }
        //#endregion Create view nodes

        return folderNode;
    }

    private createFolderNode(folder: IFolder) {
        return {
            label: folder.name,
            leaf: false,
            expanded: false,
            draggable: false,
            droppable: false,
            selectable: true,
            expandedIcon: folder.isSourceFolder ? "pi pi-database" : "pi pi-folder-open",
            collapsedIcon: folder.isSourceFolder ? "pi pi-database" : "pi pi-folder",
            key: folder.id,
            data: {
                itemData: folder,
                itemType: this.nodeDataTypeFolder,
                protected: folder.protected,
                isSourceFolder: folder.isSourceFolder
            },
            children: []
        };
    }

    private createViewsNode(folder: IFolder) {
        return {
            label: "Views",
            leaf: false,
            draggable: false,
            droppable: false,
            icon: "pi pi-clone",
            key: folder.id + "_views",
            data: {
                itemData: {},
                itemType: this.nodeDataTypeViews,
                protected: folder.protected,
                isSourceFolder: folder.isSourceFolder
            },
            children: []
        };
    }

    private createTableNodesTree(folder: IFolder, tableMetadata: ITableMetadata) {
        // Basic validation for a case if e.g. folder's database source wasn't loaded correctly
        if (!tableMetadata.staticData) return null;

        const tableMetadataNode = {
            label: tableMetadata.staticData.tableName,
            draggable: folder.isSourceFolder,
            droppable: false,
            icon: tableMetadata.staticData.isView ? "pi pi-clone" : "pi pi-table",
            key: `${tableMetadata.id}-${tableMetadata.tableId}`,
            data: {
                itemData: tableMetadata,
                itemType: this.nodeDataTypeTable,
                protected: folder.protected,
                isSourceFolder: folder.isSourceFolder
            },
            children: []
        };

        tableMetadata.columns.forEach(columnMetadata => {
            // Basic validation for a case if e.g. folder's database source wasn't loaded correctly
            if (!columnMetadata.staticData) return;

            tableMetadataNode.children.push({
                label: columnMetadata.staticData.columnName,
                draggable: false,
                droppable: false,
                icon: columnMetadata.staticData.isPrimaryKey || columnMetadata.staticData.isForeignKey
                    ? "pi pi-key"
                    : "pi pi-bars",
                key: `${columnMetadata.id}-${columnMetadata.columnId}`,
                data: {
                    itemData: columnMetadata,
                    itemType: this.nodeDataTypeColumn
                }
            });
        });

        return tableMetadataNode;
    }

    private updateCopyToFolders(): void {
        const copyToNodes = [];

        this.folders.forEach(folder => {
            if (!folder.isSourceFolder) {
                copyToNodes.push({
                    label: folder.name,
                    expanded: true,
                    icon: "pi pi-folder",
                    key: folder.id
                });
            }
        });

        this.copyToNodes = copyToNodes;

        this.copyToFoldersSelectItems = this.copyToNodes.map(x => <any>{
            label: x.label,
            command: () => this.copyTableMetadataToFolder(this.selectedNode.data.itemData.id, x.key)
        });
    }

    private copyTableMetadataToFolder(copyingTableMetadataId: number, folderIdCopyTo: string): void {
        this.dbDocService.copyTableMetadataToFolder(<ICopyTableMetadataToFolderRequest>{
            copyingTableMetadataId,
            folderIdCopyTo
        }).then(folderCopyTo => this.updateFolders([folderCopyTo]));
    }
}