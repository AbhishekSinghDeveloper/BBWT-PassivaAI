import { HttpClient } from "@angular/common/http";
import { Injectable, Injector } from "@angular/core";

import { Message } from "@bbwt/classes";
import { BroadcastService } from "@bbwt/modules/broadcasting";
import {
    BaseHttpResponsesHandler,
    HttpResponsesHandlersFactory,
    IHttpResponseHandlerSettings
} from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { IFooterMenuItem } from "./footer-menu-item";


class UpdateOrderOfItemsResponseHandler extends BaseHttpResponsesHandler {
    constructor(injector: Injector) {
        super(injector);
    }

    onSuccess(message?: string): void {
        this.showMessage(Message.Success("The order of Footer Menu Items has been successfully changed"));
    }
}

@Injectable()
export class FooterMenuService extends PagedCrudService<IFooterMenuItem> {
    static readonly FooterMenuChangedEventName = "FooterMenuChanged";

    readonly url = "api/footer-menu";
    readonly entityTitle = "Footer Menu";


    constructor(http: HttpClient,
        handlersFactory: HttpResponsesHandlersFactory,
        private injector: Injector,
        private broadcastService: BroadcastService) {
        super(http, handlersFactory);
    }


    create(item: IFooterMenuItem, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<IFooterMenuItem> {
        return super.create(item, responseHandlerSettings)
            .then((result) => {
                this.broadcastService.broadcast(FooterMenuService.FooterMenuChangedEventName);
                return result;
            });
    }

    update(id: number | string, item: IFooterMenuItem, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<IFooterMenuItem> {
        return super.update(id, item, responseHandlerSettings)
            .then((result) => {
                this.broadcastService.broadcast(FooterMenuService.FooterMenuChangedEventName);
                return result;
            });
    }

    delete(id: number | string, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<any> {
        return super.delete(id, responseHandlerSettings)
            .then((result) => {
                this.broadcastService.broadcast(FooterMenuService.FooterMenuChangedEventName);
                return result;
            });
    }


    updateOrderOfItems(items: IFooterMenuItem[]): Promise<any> {
        return this.httpPost("update-order", items, new UpdateOrderOfItemsResponseHandler(this.injector))
            .then(() => this.broadcastService.broadcast(FooterMenuService.FooterMenuChangedEventName));
    }
}