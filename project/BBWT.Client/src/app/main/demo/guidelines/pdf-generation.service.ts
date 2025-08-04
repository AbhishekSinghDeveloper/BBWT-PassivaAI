import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { FormioHTMLData } from "@main/formio/components/formio-pdf/formio-pdf-generator.component";


@Injectable({
    providedIn: "root"
})
export class PdfGenerationService extends BaseDataService {
    readonly url = "api/demo/converter";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }


    generateFromHtml(html: string | HTMLElement | any): Promise<Blob> {
        return this.handle(this.http.post(`${this.url}/html-to-pdf`, html, { responseType: "blob" }));
    }

    generateFromFormio(data: FormioHTMLData): Promise<Blob> {
        return this.handle(this.http.post(`${this.url}/html-to-pdf-formio`, data, { responseType: "blob" }));
    }

    generateFromUrl(url: string): Promise<Blob> {
        const params = new HttpParams().set("url", url);
        return this.handle<any>(this.http.get(`${this.url}/url-to-pdf`, { params, responseType: "blob" }));
    }
}