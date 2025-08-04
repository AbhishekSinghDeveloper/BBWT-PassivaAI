import { Injectable } from "@angular/core";
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from "@angular/common/http";
import { Observable } from "rxjs";
import { BrowserInfoService } from "@bbwt/services/browser-info.service";

@Injectable()
export class BrowserInfoInterceptor implements HttpInterceptor {
    constructor(private browserInfo: BrowserInfoService) {}

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        const headers = request.headers
            .set("X-Browser-Id", this.browserInfo.browserId)
            .set("X-Browser-Fingerprint", this.browserInfo.browserFingerprint);

        const requestUpdate = request.clone({ headers });

        return next.handle(requestUpdate);
    }
}
