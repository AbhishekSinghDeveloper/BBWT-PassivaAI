import { Directive, Input, OnInit } from "@angular/core";
import { NgForm, ValidatorFn } from "@angular/forms";

import { DbDocService } from "../dbdoc.service";
import { getValidatorByMetadataValidationRule } from "../metadata-converters";
import { IValidationRule } from "../dbdoc-models";


@Directive({ selector: "[dbDocForm]" })
export class DbDocFormDirective implements OnInit {
    @Input("dbDocForm") tableMetadataUniqueId: string;
    @Input() folderId: string;

    private validators: {[propertyName: string]: ValidatorFn[]};
    private rules: {[propertyName: string]: IValidationRule[]};

    constructor(private form: NgForm, private dbDocService: DbDocService) {}

    ngOnInit(): void {
        if (this.form) {
            setTimeout(() =>
                Object.keys(this.form.controls).forEach(key =>
                    this.form.controls[key].setErrors({ loading: true })));
        }

        this.validators = {};
        this.rules = {};
        this.dbDocService.getTableMetadata(this.tableMetadataUniqueId, this.folderId)
            .then(result => {
                Object.keys(result).forEach(key => {
                    if (result[key].validationRules?.length) {
                        this.rules[key] = result[key].validationRules;
                        this.validators[key] = result[key].validationRules.map(x => getValidatorByMetadataValidationRule(x));
                    }
                });

                if (this.form) {
                    Object.keys(this.form.controls).forEach(key => {
                        this.form.controls[key].setErrors(null);
                        this.form.controls[key].updateValueAndValidity();
                    });
                }
            });
    }

    getValidatorsForField(fieldName: string): ValidatorFn[] {
        const correctFieldName = Object.keys(this.validators).find(x => x.toLowerCase() == fieldName.toLowerCase());
        return correctFieldName ? this.validators[correctFieldName] : null;
    }

    getErrorMessagesForField(fieldName: string): string[] {
        const result = [];

        if (!this.form) return result;

        const correctControlName = Object.keys(this.form.controls).find(x => x.toLowerCase() == fieldName.toLowerCase());
        const errors = this.form.controls[correctControlName].dirty && this.form.controls[correctControlName].errors
            ? Object.keys(this.form.controls[correctControlName].errors)
            : [];

        if (!errors?.length) return result;

        const correctRuleName = Object.keys(this.rules).find(x => x.toLowerCase() == fieldName.toLowerCase());
        const rules = this.rules[correctRuleName];
        rules.forEach(ruleItem => {
            let addRuleErrorMessage = false;
            switch (ruleItem.$type) {
                case "required":
                    addRuleErrorMessage = errors.some(x => x.toLowerCase() == "required");
                    break;
                case "input_format":
                    addRuleErrorMessage = errors.some(x => x.toLowerCase() == "pattern");
                    break;
                case "max_length":
                    addRuleErrorMessage = errors.some(x => x.toLowerCase() == "maxlength");
                    break;
                case "number_range":
                    addRuleErrorMessage = errors.some(x => x.toLowerCase() == "rangeNumber");
                    break;
                case "date_range":
                    addRuleErrorMessage = errors.some(x => x.toLowerCase() == "rangeDate");
                    break;
            }

            if (addRuleErrorMessage) {
                result.push(ruleItem.errorMessage);
            }
        });

        return result;
    }
}