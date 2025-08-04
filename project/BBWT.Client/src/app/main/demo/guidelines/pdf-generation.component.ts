import { Component, ElementRef } from "@angular/core";

import { PdfGenerationService } from "./pdf-generation.service";


@Component({
    selector: "pdf-generation",
    templateUrl: "./pdf-generation.component.html"
})

export class PdfGenerationComponent {
    constructor(private elementRef: ElementRef, private pdfGenerationService: PdfGenerationService) { }


    onGenerateClient() {
        this.pdfGenerationService.generateFromHtml(this.elementRef.nativeElement.innerHTML).then(blob => this.openPdf(blob));
    }

    onGenerateServer() {
        window.open("/report/demo");
    }

    onGenerateMultipageTest() {
        this.pdfGenerationService.generateFromUrl("https://en.wikipedia.org/wiki/Waterfall_model").then(blob => this.openPdf(blob));
    }


    private openPdf(blob: Blob): void {
        const link = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        document.body.appendChild(link);
        link.click();
        link.remove();
    }
}