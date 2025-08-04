import { Injectable } from "@angular/core";

import { Observable, Subject } from "rxjs";
import { filter, map } from "rxjs/operators";


interface BroadcastEvent {
    key: string;
    data?: any;
}


@Injectable({
    providedIn: "root"
})
export class BroadcastService {
    private _eventBus: Subject<BroadcastEvent>;

    constructor() {
        this._eventBus = new Subject<BroadcastEvent>();
    }


    broadcast(key: string, data?: any) {
        this._eventBus.next(<BroadcastEvent> { key, data });
    }

    on<T>(key: any): Observable<T> {
        return this._eventBus.asObservable().pipe(
            filter(event => event.key === key),
            map(event => <T>event.data));
    }
}