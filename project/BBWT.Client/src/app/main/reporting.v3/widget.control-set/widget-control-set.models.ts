import {DataType, InputType, IWidgetSource} from "../core/reporting-models";
import {SelectItem} from "primeng/api";
import {IFilterRule} from "@main/reporting.v3/core/variables/variable-models";


export type ControlValueEmitType = "standalone" | "grouped" | "singleAndGrouped";

export function getControlValueEmitTypeEnumAsOptions(): SelectItem[] {
    return <SelectItem[]>[
        {label: "Standalone", value: "standalone"},
        {label: "Grouped", value: "grouped"},
        {label: "Single and grouped", value: "singleAndGrouped"}
    ];
}

export interface IControlSetView {
    id: string;

    // Foreign keys and navigational properties.
    widgetSourceId: string;
    widgetSource: IWidgetSource;
    items: IControlSetViewItem[];
}

export interface IControlSetViewItem {
    id: string;
    name: string;
    sortOrder: number;
    dataType: DataType;
    inputType: InputType;

    hintText: string;
    extraSettings: any;
    emptyFilterIfFalse: boolean;
    userCanChangeOperator: boolean;
    valueEmitType: ControlValueEmitType;

    tableId?: string;
    folderId?: string;
    sourceCode?: string;
    parentTableId?: string;
    valueColumnId?: string;
    labelColumnId?: string;
    variableName: string;

    // Foreign keys and navigational properties.
    controlSetId: string;
    filterRuleId?: string;

    filterRule: IFilterRule;
}

export interface IControlSetDisplayView {
    id: string;

    // Foreign keys and navigational properties.
    widgetSourceId: string;
    widgetSource: IWidgetSource;
    items: IControlSetDisplayViewItem[];
}

export interface IControlSetDisplayViewItem {
    id: string;
    name: string;
    sortOrder: number;
    dataType: DataType;
    inputType: InputType;

    hintText: string;
    extraSettings: any;
    emptyFilterIfFalse: boolean;
    userCanChangeOperator: boolean;
    valueEmitType: ControlValueEmitType;

    tableId?: string;
    folderId?: string;
    sourceCode?: string;
    parentTableId?: string;
    valueColumnId?: string;
    labelColumnId?: string;
    variableName: string;

    // Foreign keys and navigational properties.
    filterRuleId?: string;

    filterRule: IFilterRule;
}