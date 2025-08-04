import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Subject } from "rxjs";

import { CrudService, HttpResponsesHandlersFactory, IHttpResponseHandlerSettings } from "@bbwt/modules/data-service";
import { IMainMenuItem } from "./main-menu-item";
import { BroadcastService } from "@bbwt/modules/broadcasting";

@Injectable()
export class MainMenuService extends CrudService<IMainMenuItem> {
    static readonly MainMenuChangedEventName = "MainMenuChanged";

    readonly url = "api/menu";
    readonly entityTitle = "Menu";

    private menuSource = new Subject<string>();
    private resetSource = new Subject<void>();
    menuSource$ = this.menuSource.asObservable();
    resetSource$ = this.resetSource.asObservable();

    constructor(
        http: HttpClient,
        handlersFactory: HttpResponsesHandlersFactory,
        private broadcastService: BroadcastService
    ) {
        super(http, handlersFactory);
    }

    create(item: IMainMenuItem, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<IMainMenuItem> {
        return super.create(item, responseHandlerSettings).then(result => {
            this.broadcastService.broadcast(MainMenuService.MainMenuChangedEventName);
            return result;
        });
    }

    update(id: number | string, item: IMainMenuItem): Promise<IMainMenuItem> {
        return this.httpPost<IMainMenuItem>("update", item, this.handlersFactory.getForUpdate(this.entityTitle))
            .then(result => {
                this.broadcastService.broadcast(MainMenuService.MainMenuChangedEventName);
                return result;
            });
    }

    delete(id: number | string, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<any> {
        return super
            .delete(id, responseHandlerSettings)
            .then(result => this.broadcastService.broadcast(MainMenuService.MainMenuChangedEventName));
    }

    onMenuStateChange(key: string) {
        this.menuSource.next(key);
    }

    reset() {
        this.resetSource.next();
    }

    getForCurrentUser(): Promise<IMainMenuItem[]> {
        return this.httpGet("me", this.handlersFactory.getForReadAll());
    }
}
