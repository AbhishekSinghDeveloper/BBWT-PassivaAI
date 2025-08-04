import {ChartType, Plugin} from "chart.js";
import {AnyObject} from "chart.js/dist/types/basic";
import {IWidgetSource} from "@main/reporting.v3/core/reporting-models";
import * as moment from "moment/moment";

export function groupBy(list: any[], property: string): any[][] {
    if (!list?.length || !property) return [list ?? []];

    const taken: boolean[] = Array(list.length).fill(false);
    const groups: any[][] = [];

    for (let i = 0; i < list.length; i++) {
        if (taken[i]) continue;
        const items: any[] = [];

        for (let j = i; j < list.length; j++) {
            if (list[i]?.[property] === list[j]?.[property]) {
                taken[j] = true;
                items.push(list[j]);
            }
        }

        groups.push(items);
    }

    return groups;
}

export function sortBy(list: any[], property: string): any[] {
    if (!list?.length || !property) return list;

    // If all elements are numbers, sort by value.
    if (!list.some(item => typeof item?.[property] !== "number")) {
        return [...list].sort((a, b) => a[property] - b[property]);
    }

    // If all elements are dates, sort by value.
    if (!list.some(item => !(item?.[property] instanceof Date))) {
        return [...list].sort((a, b) => a[property].getTime() - b[property].getTime());
    }

    // Otherwise sort by string value.
    return list.sort((a, b) => String(a[property]).localeCompare(String(b[property])));
}

export function toStandardString(list: any[]): string[] {
    if (!list?.length) return list;

    // If all elements are numbers, return them with three decimals of precision.
    if (!list.some(item => typeof item !== "number")) {
        return list.map(item => (<number>item).toLocaleString("en-us", {maximumFractionDigits: 3}));
    }

    // If all elements are dates, return them in format DD/MM/YYYY.
    if (!list.some(item => !(item instanceof Date))) {
        return list.map(item => moment(item).format("DD/MM/YYYY"));
    }

    // Otherwise, return as string value.
    return list.map(item => String(item));
}

export function isNumericBy(list: any[], property: string): boolean {
    if (!list?.length || !property) return false;
    return !list.some(item => typeof item?.[property] !== "number");
}

export function minBy(list: any[], property: string): number {
    if (!list?.length || !property) return null;
    return Math.min(...list.map(item => item[property]));
}

export function maxBy(list: any[], property: string): number {
    if (!list?.length || !property) return null;
    return Math.max(...list.map(item => item[property]));
}

export function clone(object: any): any {
    if (object == null) return null;
    const clone: any = {};
    Object.keys(object).forEach(key => {
        clone[key] = Array.isArray(object[key])
            ? object[key].map(item => this.clone(item))
            : typeof object[key] === "object"
                ? this.clone(object[key])
                : object[key];
    });
    return clone;
}

export interface ChartSettings {
    // Type of the chart.
    type: ChartType;
    // Array of per-chart plugins to customize the chart behaviour.
    plugins?: Plugin[];
    // Width of the chart.
    width?: string;
    // Height of the chart.
    height?: string;
    // Options to customize the chart.
    options?: AnyObject;
}

export enum ChartTypeEnum {
    Bar = "bar",
    Bubble = "bubble",
    Doughnut = "doughnut",
    Line = "line",
    Pie = "pie",
    PolarArea = "polarArea",
    Radar = "radar",
    Scatter = "scatter"
}

export const ChartTypeEnumLabel = new Map<ChartTypeEnum, string>([
    [ChartTypeEnum.Bar, "Bar chart"],
    [ChartTypeEnum.Bubble, "Bubble chart"],
    [ChartTypeEnum.Doughnut, "Doughnut chart"],
    [ChartTypeEnum.Line, "Line chart"],
    [ChartTypeEnum.Pie, "Pie chart"],
    [ChartTypeEnum.PolarArea, "Polar area chart"],
    [ChartTypeEnum.Radar, "Radar chart"],
    [ChartTypeEnum.Scatter, "Scatter chart"]
]);

export enum ChartSourceEnum {
    Single = "single",
    Multiple = "multiple"
}

export const ChartSourceEnumLabel = new Map<ChartSourceEnum, string>([
    [ChartSourceEnum.Single, "Single Series"],
    [ChartSourceEnum.Multiple, "Multiple Series"]
]);

export enum ColumnPurpose {
    AxisX = "axisX",
    AxisY = "axisY",
    Series = "series",
    BubbleSize = "bubbleSize",
    Tooltip = "tooltip",
}

export enum Refresh {
    Metadata = 1,
    Columns = 2,
    Options = 3,
    Data = 4,
    Settings = 5
}

export enum ChartAxis {
    X = "x",
    Y = "y",
    R = "r"
}

export interface ChartAxisSettings {
    name?: string;
    unit?: string;
    label?: string;
    min?: number;
    max?: number;
    type?: string;
    required: boolean;
    display: boolean;
    visible: () => boolean;
    numeric: () => boolean;
    defaultMin: () => number;
    defaultMax: () => number;
}

export interface ColumnSearchingOptions {
    purpose: ColumnPurpose;
    predicate: (alias: string) => boolean;
    exclude: string[];
}

export interface IChartBuildDTO {
    id: string;
    chartSettingsJson?: string;

    // Foreign key and navigational properties.
    querySourceId: string;
    widgetSourceId?: string;

    widgetSource?: IWidgetSource;

    columns: IChartBuildColumnDTO[];
}

export interface IChartBuildColumnDTO {
    id: string;
    queryAlias: string;
    chartAlias?: string;
    columnPurpose: ColumnPurpose;
}

export interface IChartViewDTO {
    id: string;
    chartSettingsJson?: string;

    // Foreign key and navigational properties.
    querySourceId?: string;
    widgetSourceId: string;

    widgetSource: IWidgetSource;

    columns: IChartViewColumnDTO[];

    // Non-database properties.
    queryVariables: string[];
}

export interface IChartViewColumnDTO {
    id: string;
    queryAlias: string;
    chartAlias?: string;
    columnPurpose: ColumnPurpose;

    // Foreign keys and navigational properties.
    chartId: string;
}