import { ValidatorFn, Validators } from "@angular/forms";
import {
    IDateRangeValidationRule, IInputFormatValidationRule,
    IMaxLengthValidationRule, INumberRangeValidationRule, ITableMetadataResult, IValidationRule
} from "./dbdoc-models";
import { GridValidator, GridColumnViewSettings, IGridExternalMetadata } from "@features/grid";
import { rangeNumberValidator, rangeDateValidator, ValidationPatterns } from "@bbwt/modules/validation";
import { IHash } from "@bbwt/interfaces";


export function getGridExternalMetadataFromTableMetadataResult(metadataResult: ITableMetadataResult): IGridExternalMetadata {
    const result = <IGridExternalMetadata>{};
    const gridValidators = getGridValidatorsFromTableMetadataResult(metadataResult);
    const columnViewSettings = getGridViewSettingsFromTableMetadataResult(metadataResult);
    Object.keys(metadataResult).forEach(key => {
        result[key] = {
            validators: gridValidators[key],
            viewSettings: columnViewSettings[key]
        };
    });
    return result;
}

export function getGridValidatorsFromTableMetadataResult(metadataResult: ITableMetadataResult): IHash<GridValidator[]> {
    const result = {};

    if (!!metadataResult) {
        Object.keys(metadataResult).forEach(
            columnName => {
                result[columnName] = [];
                if (Array.isArray(metadataResult[columnName].validationRules)) {
                    metadataResult[columnName].validationRules.forEach(validationRuleItem => {
                        const gridValidationRule = getGridValidatorByMetadataValidationRule(validationRuleItem);
                        if (gridValidationRule) {
                            result[columnName].push(gridValidationRule);
                        }
                    });
                }
            }
        );
    }

    return result;
}

export function getGridValidatorByMetadataValidationRule(rule: IValidationRule): GridValidator {
    const validator = getValidatorByMetadataValidationRule(rule);
    return validator
        ? new GridValidator(validator, getErrorKeyByValidationRuleType(rule.$type), rule.errorMessage)
        : null;
}

export function getValidatorsByMetadataValidationRules(rules: IValidationRule[]): ValidatorFn[]  {
    return rules?.map(ruleItem => getValidatorByMetadataValidationRule(ruleItem));
}

export function getValidatorByMetadataValidationRule(rule: IValidationRule): ValidatorFn {
    switch (rule?.$type) {
        case "required": return Validators.required;
        case "input_format":
            const formatRule = rule as IInputFormatValidationRule;
            switch (formatRule.type) {
                case "phone": return Validators.pattern(ValidationPatterns.phone);
                case "email": return Validators.pattern(ValidationPatterns.email);
                case "url": return Validators.pattern(ValidationPatterns.uri);
                case "regex": return Validators.pattern(formatRule.format);
                default: return null;
            }
        case "max_length":
            const maxLengthRule = rule as IMaxLengthValidationRule;
            return Validators.maxLength(maxLengthRule.maxLength);
        case "number_range":
            const numberRangeRule = rule as INumberRangeValidationRule;
            return rangeNumberValidator(numberRangeRule.min, numberRangeRule.max);
        case "date_range":
            const dateRangeRule = rule as IDateRangeValidationRule;
            return rangeDateValidator(dateRangeRule.min, dateRangeRule.max);
        default:
            return null;
    }
}

export function getGridViewSettingsFromTableMetadataResult(metadataResult: ITableMetadataResult): IHash<GridColumnViewSettings> {
    const result = {};

    if (!!metadataResult) {
        Object.keys(metadataResult).forEach(
            columnName => {
                if (metadataResult[columnName].gridColumnView) {
                    result[columnName] =
                        new GridColumnViewSettings(metadataResult[columnName].gridColumnView);
                }
            }
        );
    }

    return result;
}


function getErrorKeyByValidationRuleType(ruleType: string): string {
    switch (ruleType) {
        case "required": return "required";
        case "input_format": return "pattern";
        case "max_length": return "maxlength";
        case "number_range": return "rangeNumber";
        case "date_range": return "rangeDate";
        default: return null;
    }
}
