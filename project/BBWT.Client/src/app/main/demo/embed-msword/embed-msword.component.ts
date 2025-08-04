import { Component } from "@angular/core";
import { DomSanitizer, SafeResourceUrl } from "@angular/platform-browser";

@Component({
    selector: "embed-msword",
    templateUrl: "./embed-msword.component.html"
})
export class EmbedMSWordComponent {
    url: SafeResourceUrl;
    documentId = 0;
    sanitizer: DomSanitizer;
    docs: any[];
    selectedDocument: any;

    constructor(sanitizer: DomSanitizer) {
        this.sanitizer = sanitizer;
        this.selectedDocument = {};
        this.docs = [];
        this.docs.push({ label: "Welcome", value: { id: 0, url: "https://1drv.ms/w/s!AvUFW-54mQzQjCztCq3VUMFPNs5p"} });
        this.docs.push({ label: "Recommendation", value: { id: 1, url: "https://1drv.ms/w/s!AvUFW-54mQzQjBkZl-5L50sIYrTM"} });
        this.docs.push({ label: "Absence", value: { id: 2, url: "https://1drv.ms/w/s!AvUFW-54mQzQjBjpS1xoVPH1sVz_"} });

        this.UpdateIFrame(this.docs[0].value.url);
    }

    UpdateIFrame(wordUrl) {
        const baseAddress = window.location.origin;
        this.url = this.sanitizer.bypassSecurityTrustResourceUrl(baseAddress + "/api/demo/embed-msword/request-page?url=" + wordUrl);
    }

    onChange() {
        this.UpdateIFrame(this.selectedDocument.url);
    }
}
