import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";

import { ClrTypeGroup, IColumnValidationMetadata, IValidationRule } from "../../dbdoc-models";
import { SelectItem } from "primeng/api";


@Component({
    selector: "validation-editor",
    templateUrl: "./validation-editor.component.html",
    styleUrls: ["./validation-editor.component.scss"]
})
export class ValidationEditorComponent implements OnInit {
    @Input() columnValidationMetadata: IColumnValidationMetadata;
    @Input() typeGroup: ClrTypeGroup;
    // eslint-disable-next-line @angular-eslint/no-output-native
    @Output() change = new EventEmitter();

    ruleTypeOptions: SelectItem[];
    formatTypeOptions = <SelectItem[]>[
        { label: "Phone", value: "phone" },
        { label: "Email", value: "email" },
        { label: "Url", value: "url" },
        { label: "Regex", value: "regex" },
    ];
    editingRule: any;
    editingRuleIndex: number;
    editRuleDialogVisible: boolean;
    readonly calendarYearRange = `1900:${(new Date()).getFullYear()}`;

    ngOnInit(): void {
        this.refreshRuleTypeOptions();
    }

    refreshRuleTypeOptions(): void {
        this.ruleTypeOptions = [{ label: "Required", value: "required" }];

        switch (this.typeGroup) {
            case "string":
                this.ruleTypeOptions.push({label: "Format", value: "input_format"});
                this.ruleTypeOptions.push({label: "Max length", value: "max_length"});
                break;
            case "numeric":
                this.ruleTypeOptions.push({label: "Number", value: "number_range"});
                break;
            case "date":
                this.ruleTypeOptions.push({label: "Date", value: "date_range"});
                break;
        }

        if (this.columnValidationMetadata.rules) {
            this.ruleTypeOptions = this.ruleTypeOptions
                .filter(x => this.columnValidationMetadata.rules.every(y => y.$type != x.value));
        }
    }

    startRuleEditing(rule?: IValidationRule, index?: number): void {
        this.editingRule = rule ? {...rule} : {};
        this.editingRuleIndex = index;
        this.editRuleDialogVisible = true;
    }

    saveEditingRule(): void {
        if (this.editingRuleIndex == null) {
            this.columnValidationMetadata.rules.push(this.editingRule);
        } else {
            this.columnValidationMetadata.rules[this.editingRuleIndex] = this.editingRule;
        }

        this.change.emit();
        this.cancelRuleEditing();
        this.refreshRuleTypeOptions();
    }

    cancelRuleEditing(): void {
        this.editRuleDialogVisible = false;
        this.editingRule = null;
        this.editingRuleIndex = null;
    }

    startRuleDeleting(index: number): void {
        this.columnValidationMetadata.rules.splice(index, 1);
        this.change.emit();
        this.refreshRuleTypeOptions();
    }
}