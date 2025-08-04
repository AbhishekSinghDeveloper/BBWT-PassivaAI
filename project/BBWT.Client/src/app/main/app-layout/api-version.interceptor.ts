import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { tap } from "rxjs/operators";
import { ApiVersionService } from "./api-version.service";

@Injectable()
export class ApiVersionInterceptor implements HttpInterceptor {
    constructor(private service: ApiVersionService) { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(tap(event => {
            if (event instanceof HttpResponse) {
                this.service.version = event.headers.get("api-version");
            }
        }));
    }
}