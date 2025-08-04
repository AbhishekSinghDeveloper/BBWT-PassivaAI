import { BasePdfReportAbstract } from "@features/pdf-report-generator/pdf-generator";
import { FormioPDFResource } from "./formio-pdf-resource";

export class FormioPDFGenerator extends BasePdfReportAbstract {
    constructor(formioPDFResource: FormioPDFResource) {
        super(formioPDFResource);
    }
}