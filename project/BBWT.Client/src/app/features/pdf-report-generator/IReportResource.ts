import { PDFLibrary } from "./pdf-libraries";

export interface IReportResource {
    library: PDFLibrary;
    html: string;
    jsonData: object;
}