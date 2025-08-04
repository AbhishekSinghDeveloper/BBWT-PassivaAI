import { HttpClient, HttpEvent, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { Observable } from "rxjs";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service/http-responses-handler";
import { IOrganization } from "./organization";
import { PagedCrudService } from "@features/grid";
import { BroadcastService } from "@bbwt/modules/broadcasting";


@Injectable({
    providedIn: "root"
})
export class OrganizationService extends PagedCrudService<IOrganization> {
    static readonly OrganizationChangedEventName = "OrganizationChanged";

    readonly url = "api/organization";
    readonly entityTitle = "Organization";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, private broadcastService: BroadcastService) {
        super(http, handlersFactory);
    }


    update(id: string, item: IOrganization): Promise<IOrganization> {
        return super.update(id, item).then(result => {
            this.broadcastService.broadcast(OrganizationService.OrganizationChangedEventName, result);
            return result;
        });
    }

    exists(organization: any): Promise<boolean> {
        return this.httpPost("exists", organization, this.handlersFactory.getForReadAll());
    }

    getAllPlain(): Promise<IOrganization[]> {
        return this.httpGet("all-plain", this.handlersFactory.getForReadAll());
    }

    uploadLogoImage(formData: FormData): Observable<HttpEvent<any>> {
        return this.http.request(new HttpRequest(
            "POST", `${this.url}/upload-logo-image`, formData, { reportProgress: true }
        ));
    }

    uploadLogoIcon(formData: FormData): Observable<HttpEvent<any>> {
        return this.http.request(new HttpRequest(
            "POST", `${this.url}/upload-logo-icon`, formData, { reportProgress: true }
        ));
    }
}