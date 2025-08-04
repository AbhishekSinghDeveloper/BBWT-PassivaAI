import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from "@angular/common/http";

import { Observable } from "rxjs";
import { tap } from "rxjs/operators";


export class DatesConversionInterceptor implements HttpInterceptor {
    private readonly _iso8601 = /^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d(\.\d+)?(([+-]\d\d:\d\d)|Z)?$/;


    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            tap((event: HttpEvent<any>) => {
                if (event instanceof HttpResponse) {
                    this.convertDates(event.body);
                }
            }),
        );
    }


    private isIso8601(value: string): boolean {
        if (!value) return false;
        return this._iso8601.test(value);
    }

    private convertDates(obj: any): void {
        if (!obj) return;

        Object.keys(obj).forEach(key => {
            if (typeof obj[key] === "string") {
                if (this.isIso8601(obj[key])) {
                    obj[key] = new Date(obj[key]);
                }
            } else if (typeof obj[key] === "object") {
                this.convertDates(obj[key]);
            }
        });
    }
}