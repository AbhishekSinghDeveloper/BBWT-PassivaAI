import {Injectable} from "@angular/core";

import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {PdfConfiguration} from "../core/reporting-models";
import {HttpClient} from "@angular/common/http";


@Injectable()
export class PdfExportingService extends BaseDataService {
    readonly url = "api/reporting3/pdf-export";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    generateFromHtml(configuration: PdfConfiguration): Promise<Blob> {
        return this.handle(this.http.post(`${this.url}/html-to-pdf`, configuration, {responseType: "blob"}));
    }

    openPdf(blob: Blob): void {
        const link: HTMLAnchorElement = document.createElement("a");
        link.href = URL.createObjectURL(blob);
        document.body.appendChild(link);
        link.target = "_blank";
        link.click();
        link.remove();
    }
}