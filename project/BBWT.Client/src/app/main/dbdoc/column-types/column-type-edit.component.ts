import { Component } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { ConfirmationService } from "primeng/api";

import { ColumnTypeService } from "./column-type.service";
import { IColumnType } from "./column-type-models";
import {
    getAnonymizationRuleOptions,
    getClrTypeGroupOptions,
    IColumnValidationMetadata,
    IColumnViewMetadata
} from "../dbdoc-models";


@Component({
    templateUrl: "./column-type-edit.component.html",
    styleUrls: ["./column-type-edit.component.scss"]
})
export class ColumnTypeEditComponent {
    columnType: IColumnType = <any>{};
    typeGroups = getClrTypeGroupOptions();
    anonymizationRuleOptions = getAnonymizationRuleOptions();


    constructor(private router: Router,
                private activatedRoute: ActivatedRoute,
                private confirmationService: ConfirmationService,
                private columnTypesService: ColumnTypeService) {
        activatedRoute.data.subscribe(data => {
            if (data["columnType"]) {
                this.columnType = data["columnType"];
            }
        });
    }


    back(): void {
        this.router.navigate(["/app/dbdoc/column-types"]);
    }

    save(): void {
        (!this.columnType.id
            ? this.columnTypesService.create(this.columnType)
            : this.columnTypesService.update(this.columnType.id, this.columnType))
            .then(result => {
                if (!this.columnType.id) {
                    this.router.navigate([`/app/dbdoc/column-types/edit/${result.id}`]);
                } else {
                    this.columnType = result;
                }
            });
    }

    addValidationMetadata(): void {
        this.columnTypesService.setValidationMetadata(
            this.columnType.id,
            <IColumnValidationMetadata>{ rules: [] }
        ).then(validationMetadata => this.columnType.validationMetadata = validationMetadata);
    }

    updateValidationMetadata(): void {
        this.columnTypesService.setValidationMetadata(
            this.columnType.id,
            this.columnType.validationMetadata
        ).then(validationMetadata => this.columnType.validationMetadata = validationMetadata);
    }

    startValidationMetadataDeleting(): void {
        if (!this.columnType.validationMetadata.rules?.length) {
            this.deleteValidationMetadata();
        } else {
            this.confirmationService.confirm({
                message: "Are you sure you want to delete validation metadata?",
                accept: () => this.deleteValidationMetadata()
            });
        }
    }

    deleteValidationMetadata(): void {
        this.columnTypesService.deleteValidationMetadata(this.columnType.id)
            .then(() => {
                this.columnType.validationMetadata = null;
                this.columnType.validationMetadataId = null;
            });
    }

    addViewMetadata(): void {
        this.columnTypesService.setViewMetadata(
            this.columnType.id,
            <IColumnViewMetadata><any>{}
        ).then(viewMetadata => this.columnType.viewMetadata = viewMetadata);
    }

    updateViewMetadata(): void {
        this.columnTypesService.setViewMetadata(
            this.columnType.id,
            this.columnType.viewMetadata
        ).then(viewMetadata => this.columnType.viewMetadata = viewMetadata);
    }

    startViewMetadataDeleting(): void {
        if (!this.columnType.viewMetadata.gridColumnView) {
            this.deleteViewMetadata();
        } else {
            this.confirmationService.confirm({
                message: "Are you sure you want to delete view metadata?",
                accept: () => this.deleteViewMetadata()
            });
        }
    }

    deleteViewMetadata(): void {
        this.columnTypesService.deleteViewMetadata(this.columnType.id)
            .then(() => {
                this.columnType.viewMetadata = null;
                this.columnType.viewMetadataId = null;
            });
    }
}
