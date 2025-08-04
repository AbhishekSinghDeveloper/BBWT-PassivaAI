import {IWidgetSource, WidgetSourceCode} from "../core/reporting-models";

export function getLayoutTypeName(code: LayoutType): string {
    switch (code) {
        case LayoutType.Cards:
            return "Cards";
        case LayoutType.Dividers:
            return "Dividers";
        case LayoutType.Plain:
            return "Plain";
    }

    return "";
}

// Dashboard page models.
export interface IDashboard {
    id: string;
    name: string;
    description?: string;
    urlSlug?: string;
    createdOn: Date;

    // Foreign keys and navigational properties.
    ownerId?: string;
    ownerName?: string;
    organizationIds: number[];
}

// Dashboard build models.
export interface IDashboardBuild {
    id: string;
    name: string;
    displayName: boolean;
    description?: string;
    urlSlug?: string;
    layout: LayoutType;
    widgetsMargin: number;
    widgetsPadding: number;

    // Foreign keys and navigational properties.
    widgets: IDashboardWidgetBuild[];
}

export interface IDashboardWidgetBuild {
    id: string;
    rowIndex: number;
    columnIndex: number;

    // Foreign keys and navigational properties.
    widgetSourceId: string;
    widgetSource: IWidgetSource;
}

// Dashboard view models.
export interface IDashboardView {
    id: string;
    name: string;
    displayName: boolean;
    layout: LayoutType;
    widgetsMargin: number;
    widgetsPadding: number;

    // Foreign keys and navigational properties.
    widgets: IDashboardWidgetView[];
}

export interface IDashboardWidgetView {
    id: string;
    rowIndex: number;
    columnIndex: number;

    // Foreign keys and navigational properties.
    widgetSourceId: string;
    widgetType: WidgetSourceCode;
}

export enum LayoutType {
    Cards = 0,
    Dividers = 1,
    Plain = 2
}