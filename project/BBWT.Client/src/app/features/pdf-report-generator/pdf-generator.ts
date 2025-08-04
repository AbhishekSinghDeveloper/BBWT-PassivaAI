import { jsPDF } from "jspdf";

import { ServiceLocator } from "@bbwt/utils/ServiceLocator";
import { FormioHtmlParserService } from "@features/bb-formio/classes/pdf-generator/formio-html-parser.service";
import { PdfGenerationService } from "@main/demo/guidelines";
import { FormioHTMLData } from "@main/formio/components/formio-pdf/formio-pdf-generator.component";
import { IReportGenerator } from "./IReportGenerator";
import { IReportResource } from "./IReportResource";
import { PDFLibrary } from "./pdf-libraries";

export abstract class BasePdfReportAbstract implements IReportGenerator {
    protected doc: jsPDF;

    private readonly _baseResource: IReportResource;

    // services
    private readonly _formioHtmlParserService = ServiceLocator.injector.get(FormioHtmlParserService);
    private readonly _pdfGenerationService = ServiceLocator.injector.get(PdfGenerationService);

    /**
     *
     */
    constructor(baseResource: IReportResource) {
        this._baseResource = baseResource;
        this.doc = new jsPDF("p", "mm", "letter");
    }

    /**
    * IMPORTANT: DO NOT override this method.
    **/
    generateReport = async () => {
        // this.doc.setProperties({ title: `${this.reportName}` });

        // const returnPromise = new Promise();

        const reportData = this.getPDFReportData();

        if (this._baseResource.library === PDFLibrary.jsPDF) {

            this.doc.html(reportData, {
                margin: [10, 10, 10, 10],
                autoPaging: "text",
                x: 0,
                y: 0,
                width: 190, //target width in the PDF document
                windowWidth: 675, //window width in CSS pixels
                callback: function (pdf) {
                    // Save the PDF or display it as needed
                    window.open(pdf.output("bloburl"), "_blank");
                    // pdf.save("HTMLGeneratioNTest" + ".pdf");
                }
            });
        }

        if (this._baseResource.library === PDFLibrary.jsReport) {

            const formData: FormioHTMLData = {
                htmlContent: reportData
            };

            const blob = await this._pdfGenerationService.generateFromFormio(formData);
            this.openPdf(blob);
        }
    }

    private getPDFReportData = (): string => {
        const htmlContent = this._baseResource.html;

        const jsonData = this._baseResource.jsonData;

        const parsedHtml = this._formioHtmlParserService.parse(htmlContent, jsonData);

        return parsedHtml.body.innerHTML;
    }

    private openPdf(blob: Blob): void {
        const link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        link.target = "_blank";
        document.body.appendChild(link);
        link.click();
        link.remove();
    }

}