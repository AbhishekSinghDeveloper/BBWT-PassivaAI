import { Component, Input, OnInit } from "@angular/core";

import { ConfirmationService, SelectItem } from "primeng/api";

import { DbDocService } from "../dbdoc.service";
import {
    getAnonymizationRuleOptions,
    IColumnMetadata,
    IColumnValidationMetadata,
    IColumnViewMetadata, ICopyTableMetadataToFolderRequest,
    IFolder, ISetColumnTypeMetadataForColumnMetadataRequest
} from "../dbdoc-models";
import { ColumnTypeService, IColumnType } from "../column-types";
import { DbExplorerComponent } from "./db-explorer.component";


@Component({
    selector: "column-metadata-editor",
    templateUrl: "./column-metadata-editor.component.html",
    styleUrls: ["./column-metadata-editor.component.scss"]
})
export class ColumnMetadataEditorComponent implements OnInit {
    @Input() columnMetadata: IColumnMetadata;

    anonymizationRuleOptions = getAnonymizationRuleOptions();
    suitableColumnTypes: IColumnType[];
    selectedColumnType: IColumnType;
    columnTypeOptions: SelectItem[];
    containingFolder: IFolder;
    copyCustomColumnTypeEnabled = false;
    copyingCustomColumnTypeId: string;


    constructor(public dbExplorerComponent: DbExplorerComponent,
                private confirmationService: ConfirmationService,
                private dbDocService: DbDocService,
                private columnTypeService: ColumnTypeService) {
        columnTypeService.getAll().then(result => {
            this.suitableColumnTypes = result
                .filter(x => x.group == this.columnMetadata.staticData.clrTypeGroup);
            this.columnTypeOptions = this.suitableColumnTypes
                .map(x => <SelectItem> { label: x.name, value: x.id });

            this.setSelectedColumnType();
        });
    }


    ngOnInit(): void {
        this.containingFolder = this.dbExplorerComponent.getColumnMetadataContainingFolder(this.columnMetadata.id);
    }


    save(): void {
        const savingColumnMetadata = {...this.columnMetadata};
        this.dbDocService.updateColumnMetadata(savingColumnMetadata.id, savingColumnMetadata);
    }

    addValidationMetadata(): void {
        this.dbDocService.setColumnValidationMetadata(
            this.columnMetadata.id,
            <IColumnValidationMetadata>{ rules: [] }
        ).then(validationMetadata => {
            this.columnMetadata.validationMetadataId = validationMetadata.id;
            this.columnMetadata.validationMetadata = validationMetadata;
        });
    }

    updateValidationMetadata(): void {
        this.dbDocService.setColumnValidationMetadata(
            this.columnMetadata.id,
            this.columnMetadata.validationMetadata
        ).then(validationMetadata => this.columnMetadata.validationMetadata = validationMetadata);
    }

    startValidationMetadataDeleting(): void {
        if (!this.columnMetadata.validationMetadata.rules?.length) {
            this.deleteValidationMetadata();
        } else {
            this.confirmationService.confirm({
                message: "Are you sure you want to delete validation metadata?",
                accept: () => this.deleteValidationMetadata()
            });
        }
    }

    deleteValidationMetadata(): void {
        this.dbDocService.deleteColumnValidationMetadata(this.columnMetadata.id)
            .then(() => {
                this.columnMetadata.validationMetadata = null;
                this.columnMetadata.validationMetadataId = null;
            });
    }

    addViewMetadata(): void {
        this.dbDocService.setColumnViewMetadata(
            this.columnMetadata.id,
            <IColumnViewMetadata>{}
        ).then(viewMetadata => {
            this.columnMetadata.viewMetadataId = viewMetadata.id;
            this.columnMetadata.viewMetadata = viewMetadata;
        });
    }

    updateViewMetadata(): void {
        this.dbDocService.setColumnViewMetadata(
            this.columnMetadata.id,
            this.columnMetadata.viewMetadata
        ).then(viewMetadata => this.columnMetadata.viewMetadata = viewMetadata);
    }

    startViewMetadataDeleting(): void {
        if (!this.columnMetadata.viewMetadata.gridColumnView) {
            this.deleteViewMetadata();
        } else {
            this.confirmationService.confirm({
                message: "Are you sure you want to delete view metadata?",
                accept: () => this.deleteViewMetadata()
            });
        }
    }

    deleteViewMetadata(): void {
        this.dbDocService.deleteColumnViewMetadata(this.columnMetadata.id)
            .then(() => {
                this.columnMetadata.viewMetadata = null;
                this.columnMetadata.viewMetadataId = null;
            });
    }

    setSelectedColumnType(): void {
        this.selectedColumnType = this.columnMetadata.columnTypeId
            ? this.suitableColumnTypes.find(x => x.id == this.columnMetadata.columnTypeId)
            : null;
    }

    performCustomColumnTypeCopying(): void {
        this.dbDocService.setColumnTypeMetadataForColumnMetadata({
            columnMetadataId: this.columnMetadata.id,
            columnTypeId: this.copyingCustomColumnTypeId
        } as ISetColumnTypeMetadataForColumnMetadataRequest)
            .then(result => {
                this.dbExplorerComponent.setSelectedNodeItemData(result);
                this.columnMetadata = result;
                this.setSelectedColumnType();
            });
    }
}