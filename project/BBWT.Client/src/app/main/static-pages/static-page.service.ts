import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";

import { HttpResponsesHandlersFactory, IHttpResponseHandlerSettings } from "@bbwt/modules/data-service";
import { StaticPage } from "./static-page";
import { PagedCrudService } from "@features/grid";
import { BroadcastService } from "@bbwt/modules/broadcasting";


@Injectable()
export class StaticPageService extends PagedCrudService<StaticPage> {
    static readonly StaticPagesChangedEventName = "StaticPagesChanged";

    readonly url = "api/static-page";
    readonly entityTitle = "Static Page";


    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, private broadcastService: BroadcastService) {
        super(http, handlersFactory);
    }


    create(item: StaticPage, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<StaticPage> {
        return super.create(item, responseHandlerSettings).then(result => {
            this.broadcastService.broadcast(StaticPageService.StaticPagesChangedEventName);
            return result;
        });
    }

    update(id: number | string, item: StaticPage, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<StaticPage> {
        return super.update(id, item, responseHandlerSettings).then(result => {
            this.broadcastService.broadcast(StaticPageService.StaticPagesChangedEventName);
            return result;
        });
    }

    delete(id: number | string, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<any> {
        return super.delete(id, responseHandlerSettings)
            .then(result => this.broadcastService.broadcast(StaticPageService.StaticPagesChangedEventName));
    }

    getByUrl(url: string): Promise<StaticPage> {
        return this.httpGet(`by-url?url=${url}`);
    }

    checkExist(staticPage: StaticPage): Promise<boolean> {
        return this.httpPost("checkExist", staticPage);
    }
}