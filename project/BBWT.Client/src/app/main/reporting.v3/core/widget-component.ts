import {PdfConfiguration} from "@main/reporting.v3/core/reporting-models";

export interface IWidgetComponent extends IPdfExportable {
    output: string;
    widgetType: string;
    widgetVisible: boolean;
}

export interface IPdfExportable {
    getPdfConfiguration(): Promise<PdfConfiguration>;

    generatePdf(): Promise<void>;
}