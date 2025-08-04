import {IEntity} from "@bbwt/interfaces";
import {SelectItem} from "primeng/api";


export type ExpressionOperator = "equals" | "notEquals" | "more" | "moreOrEqual" | "less" | "lessOrEqual" |
    "between" | "contains" | "notContains" | "in" | "notIn" | "startsWith" | "endsWith" | "isSet";

export function getExpressionOperatorEnumAsOptions(...exclude: ExpressionOperator[]): SelectItem[] {
    return <SelectItem[]>[
        {label: "Is set", value: "isSet"},
        {label: "=", value: "equals"},
        {label: "≠", value: "notEquals"},
        {label: ">", value: "more"},
        {label: "≥", value: "moreOrEqual"},
        {label: "<", value: "less"},
        {label: "≤", value: "lessOrEqual"},
        {label: "Between", value: "between"},
        {label: "Contains", value: "contains"},
        {label: "In", value: "in"},
        {label: "Not in", value: "notIn"},
        {label: "Not contains", value: "notContains"},
        {label: "Starts with", value: "startsWith"},
        {label: "Ends with", value: "endsWith"},
    ].filter(item => !exclude.some(exclusion => exclusion === item.value));
}

export interface IVariableRule extends IEntity {
    id: number;
    variableName: string;
    operator: ExpressionOperator;
    operand?: any;
}

export interface IFilterRule extends IEntity {
    id: number;
    operand: string;
    operator: ExpressionOperator;
    tableColumnId: string;
}

export type EmittedVariableBehavior = "populate" | "clean"

export interface IEmittedVariable {
    data?: any;
    value: any;
    name: string;
    $type: string;
    empty: boolean;
    behaviorOnEmpty: EmittedVariableBehavior
}

export interface IQueryVariables {
    variables: IEmittedVariable[];
}