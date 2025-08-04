import { IReportResource } from "@features/pdf-report-generator/IReportResource";
import { PDFLibrary } from "@features/pdf-report-generator/pdf-libraries";

export class FormioPDFResource implements IReportResource {
    library: PDFLibrary;
    html: string;
    jsonData: object;
}