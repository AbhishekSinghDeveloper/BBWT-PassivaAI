import {Component, ElementRef, ViewChild} from "@angular/core";
import {textFieldMultipleOptionsHtml, textFieldMultipleOptionsJSON} from "@features/bb-formio/classes/pdf-generator/constants";
import {FormioPDFGenerator} from "@features/bb-formio/classes/pdf-generator/formio-pdf-generator";

import {IReportGenerator} from "@features/pdf-report-generator/IReportGenerator";
import {IReportResource} from "@features/pdf-report-generator/IReportResource";
import {PDFLibrary} from "@features/pdf-report-generator/pdf-libraries";

export type FormioHTMLData = {
    htmlContent: string;
    formData?: any;
}

@Component({
    selector: "formio-pdf",
    templateUrl: "./formio-pdf-generator.component.html",
})
export class FormioPDFGeneratorComponent {
    @ViewChild("textAreaHTML") textAreaHTML: ElementRef<HTMLInputElement>;
    @ViewChild("textAreaJsonData") textAreaJsonData: ElementRef<HTMLInputElement>;

    public html = textFieldMultipleOptionsHtml;
    public jsonData = textFieldMultipleOptionsJSON;

    generateJsPDFReport() {
        let _jsonData: any;
        try {
            _jsonData = JSON.parse(this.textAreaJsonData.nativeElement.value);
        } catch (err) {
            _jsonData = this.jsonData;
        }

        const jsPDF: IReportGenerator = new FormioPDFGenerator(<IReportResource>{
            html: this.textAreaHTML.nativeElement.value,
            jsonData: _jsonData,
            library: PDFLibrary.jsPDF
        });

        jsPDF.generateReport().then();
    }

    async generateJsReportPDF() {
        let _jsonData: any;
        try {
            _jsonData = JSON.parse(this.textAreaJsonData.nativeElement.value);
        } catch (err) {
            _jsonData = this.jsonData;
        }

        const jsReport: IReportGenerator = new FormioPDFGenerator(<IReportResource>{
            html: this.textAreaHTML.nativeElement.value,
            jsonData: _jsonData,
            library: PDFLibrary.jsReport
        });

        jsReport.generateReport().then();
    }
}
