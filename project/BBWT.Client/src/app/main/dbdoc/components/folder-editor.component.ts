import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";

import { SelectItem } from "primeng/api";

import { DatabaseType, IFolder, getDatabaseSourceTypeOptions } from "../dbdoc-models";
import { DbDocFolderService } from "../dbdoc-folder.service";


@Component({
    selector: "folder-editor",
    templateUrl: "./folder-editor.component.html",
    styleUrls: ["./folder-editor.component.scss"]
})
export class FolderEditorComponent implements OnInit {
    databaseTypeOptions = getDatabaseSourceTypeOptions();

    _folder: IFolder;
    @Input() set folder(value: IFolder) {
        this._folder = value;

        //TODO: temporarily disabled this code in development
        //this.dbPathMacros = [];
        //this.dbDocFolderService.getDbPathMacrosAllAliases(this._folder.id).then(x => this.dbPathMacros = x);
    }
    @Input() folderOwnerTypes: string[];
    @Output() changed = new EventEmitter<IFolder>();

    folderOwnerTypeItems: SelectItem[];
    dbPathMacros: any[];

    constructor(private dbDocFolderService: DbDocFolderService) {
        this.initWidgets();
    }

    save(): void {
        const savingFolder = { ...this._folder };
        savingFolder.databaseSource = null;
        savingFolder.tables = null;
        delete savingFolder.changedOn;
        this.dbDocFolderService.updateFolder(savingFolder.id, savingFolder)
            .then((f: IFolder) => this.changed.emit(f));
    }

    private async initWidgets(): Promise<void> {       
    }

    ngOnInit(): void {
        this.folderOwnerTypeItems = this.folderOwnerTypes.map(x => <SelectItem>{ label: x, value: x });        
    }

    private toDbTypeName(type: DatabaseType): string {
        return this.databaseTypeOptions.find(x => x.value === type).label;
    }
}