import { Component, OnInit } from "@angular/core";
import { UntypedFormBuilder, UntypedFormControl, UntypedFormGroup } from "@angular/forms";
import { SelectItem, ConfirmationService } from "primeng/api";

import { downloadFileFromBlob } from "@bbwt/utils";
import { CellDataType, ColumnDefinition } from "@main/data-import";
import { DemoDataImportService } from "./demo-data-import.service";

type EditingMode = "edit" | "add";

const DATA_TYPES: { value: CellDataType, label: string }[] = [
    { value: CellDataType.String, label: "String" },
    { value: CellDataType.Number, label: "Number" },
    { value: CellDataType.Date, label: "Date" },
    { value: CellDataType.Phone, label: "Phone" },
    { value: CellDataType.Email, label: "Email" },
    { value: CellDataType.Custom, label: "Custom" },
    { value: CellDataType.DateTimeOffset, label: "DateTimeOffset" },
    { value: CellDataType.Decimal, label: "Decimal" }
];

const CUSTOM_VALIDATION_TYPES: { value: string, label: string }[] = [
    { value: "JobRole", label: "Job role" }
];


@Component({
    selector: "import-config",
    templateUrl: "./import-config.component.html",
    styleUrls: ["./import-config.component.scss"]
})
export class ImportConfigComponent implements OnInit {
    dataTypes = DATA_TYPES;
    customValidationTypes = CUSTOM_VALIDATION_TYPES;
    selectedDef: ColumnDefinition;
    editingDef: ColumnDefinition;
    originalDef: ColumnDefinition;
    editingMode: EditingMode;
    dialogIsVisible: boolean;
    SampleXls: SelectItem[];
    selectedXls: string;
    dialogForm: UntypedFormGroup;

    constructor(private dataImportService: DemoDataImportService, private confirmationService: ConfirmationService, private fb: UntypedFormBuilder) {
        this.dialogIsVisible = false;
    }

    get firstRow(): number {
        return this.dataImportService.firstRow;
    }

    set firstRow(value: number) {
        this.dataImportService.setFirstRow(value);
    }

    get maxErrorsCount(): number {
        return this.dataImportService.maxErrorsCount;
    }

    set maxErrorsCount(value: number) {
        this.dataImportService.setMaxErrorsCount(value);
    }

    get columnDefinitions(): ColumnDefinition[] {
        return this.dataImportService.columnDefinitions;
    }

    set columnDefinitions(value: ColumnDefinition[]) {
        this.dataImportService.setColumnDefinitions(value);
    }

    static swap(i1: number, i2: number, collection: any[]) {
        const tmp = collection[i1];
        collection[i1] = collection[i2];
        collection[i2] = tmp;
    }

    ngOnInit(): void {
        this.dialogForm = this.fb.group({
            "orderNumber": new UntypedFormControl(),
            "position": new UntypedFormControl(),
            "targetFieldName": new UntypedFormControl(),
            "dataType": new UntypedFormControl(),
            "typeInfo_min": new UntypedFormControl(),
            "typeInfo_max": new UntypedFormControl(),
            "typeInfo_dateFormats": new UntypedFormControl(),
            "customValidationTypes": new UntypedFormControl(),
            "isAllowNulls": new UntypedFormControl(),
            "defaultValue": new UntypedFormControl("", (validator: UntypedFormControl) => {
                if (!validator.value) {
                    return null;
                }

                const dataType = validator.parent.controls["dataType"];

                switch (dataType.value) {
                    case CellDataType.String:
                    case CellDataType.Custom:
                    case CellDataType.Date:
                    case CellDataType.DateTimeOffset:
                        return null;
                    case CellDataType.Email:
                        let pattern = /^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$/;
                        if (pattern.exec(validator.value)) {
                            return null;
                        }

                        return { valid: false, error: "Email is not valid" };
                    case CellDataType.Number:
                    case CellDataType.Decimal:

                        if (isNaN(validator.value)) {
                            return { valid: false, error: "Number is not valid" };
                        }

                        const typeInfo_min = validator.parent.controls["typeInfo_min"];

                        if (typeInfo_min && Number(typeInfo_min.value) > Number(validator.value)) {
                            return { valid: false, error: "Number must be less " + typeInfo_min.value + "" };
                        }

                        const typeInfo_max = validator.parent.controls["typeInfo_max"];

                        if (typeInfo_max && Number(typeInfo_max.value) < Number(validator.value)) {
                            return { valid: false, error: "Number must be greater " + typeInfo_max.value + "" };
                        }

                        return null;
                    case CellDataType.Phone:
                        // eslint-disable-next-line max-len
                        pattern = /^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$/;
                        if (pattern.exec(validator.value)) {
                            return null;
                        }

                        return { valid: false, error: "Phone is not valid" };
                }
            })
        });

        this.SampleXls = [
            { label: "Valid sample file", value: "sample-valid.csv" },
            { label: "Invalid sample file", value: "sample-invalid.csv" },
        ];
    }

    changeDataType(event) {
        this.validateDefaultValue();
    }

    changTypeInfo_min(event) {
        this.validateDefaultValue();
    }

    changTypeInfo_max(event) {
        this.validateDefaultValue();
    }

    private validateDefaultValue() {
        const defaultValue = this.dialogForm.controls["defaultValue"];
        if (defaultValue) {
            defaultValue.updateValueAndValidity();
        }
    }

    downloadSample() {
        if (this.selectedXls !== null) {
            this.dataImportService.getCsvFileSample(this.selectedXls)
                .then(data => {
                    downloadFileFromBlob(new Blob([data], { type: "text/csv" }), this.selectedXls);
                });
        }
    }

    addDef(): void {
        this.editingMode = "add";
        this.editingDef = new ColumnDefinition(this.columnDefinitions.length + 1);
        this.dialogIsVisible = true;
    }

    editDef(def: ColumnDefinition): void {
        this.editingMode = "edit";
        this.originalDef = this.selectedDef;
        this.editingDef = JSON.parse(JSON.stringify(this.selectedDef));
        this.dialogIsVisible = true;
    }

    deleteDef(def: ColumnDefinition): void {
        this.confirmationService.confirm({
            message: "Are you sure (Y/N) ?", accept: () => {
                this.columnDefinitions.splice(this.columnDefinitions.indexOf(def), 1);
                this.selectedDef = null;
            }
        });
    }

    commitEdit(): void {
        if (this.editingMode === "add") {
            this.columnDefinitions.push(this.editingDef);
            this.selectedDef = this.editingDef;
            this.handleOrderNumber(this.editingDef);
        } else {
            Object.assign(this.originalDef, this.editingDef);
            this.handleOrderNumber(this.originalDef);
        }

        this.dialogIsVisible = false;
    }

    finishEdit(): void {
        if (this.editingMode === "edit") {
            this.originalDef = null;
        }

        this.editingMode = null;
        this.editingDef = null;
    }

    getCellDataTypeName(type: CellDataType): string {
        return this.dataTypes.find(item => item.value === type).label;
    }

    private handleOrderNumber(def: ColumnDefinition) {
        for (let i = this.columnDefinitions.indexOf(def); i > 0; i--) {
            if (this.columnDefinitions[i].orderNumber <= this.columnDefinitions[i - 1].orderNumber) {
                ImportConfigComponent.swap(i - 1, i, this.columnDefinitions);
                this.columnDefinitions[i].orderNumber++;
            } else {
                break;
            }
        }

        for (let i = this.columnDefinitions.indexOf(def); i < this.columnDefinitions.length - 1; i++) {
            if (this.columnDefinitions[i].orderNumber >= this.columnDefinitions[i + 1].orderNumber) {
                ImportConfigComponent.swap(i, i + 1, this.columnDefinitions);
                this.columnDefinitions[i].orderNumber--;
            } else {
                break;
            }
        }
    }
}