import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { CrudService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { IOrganizationBrand } from "./organization-brand";
import { BroadcastService } from "@bbwt/modules/broadcasting";


@Injectable({
    providedIn: "root"
})
export class BrandService extends CrudService<IOrganizationBrand> {
    static readonly OrganizationBrandingChangedEventName = "OrganizationBrandingChanged";

    readonly url = "api/branding";
    readonly entityTitle = "Brandings";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, private broadcastService: BroadcastService) {
        super(http, handlersFactory);
    }


    update(id: string, item: IOrganizationBrand): Promise<IOrganizationBrand> {
        return super.update(id, item).then(result => {
            this.broadcastService.broadcast(BrandService.OrganizationBrandingChangedEventName, result);
            return result;
        });
    }

    getBrandForCurrentUser(): Promise<IOrganizationBrand> {
        return this.httpGet("me", this.handlersFactory.getForReadAll());
    }
}