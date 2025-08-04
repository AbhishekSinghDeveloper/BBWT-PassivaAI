import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { Observable } from "rxjs";

import { CookieService } from "@main/cookie/cookie.service";


/**
 * This interceptor allows developers to visit the application from
 * the port where front end part launched. He changes requests to API by adding
 * the part which leads to the back end origin. Thus front end becomes a root of
 * the site that is required by some technologies like PWA. Specify corresponding
 * origins for front and back parts into "frontEndOrigin" and "backEndOrigin" fields.
 * Ensure that back end allows Cross-Origin Resource Sharing (CORS).
 * */
@Injectable()
export class ApiInterceptor implements HttpInterceptor {
    private readonly frontEndOrigin = "http://localhost:4200"; // Origin where the front-end server runs.
    private readonly backEndOrigin = "http://localhost:50815"; // Origin where the back-end server runs.
    private readonly apiUrlSegment = "api/"; // A segment within an URL that uniquely identifies an API request.

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (req.url.includes(this.apiUrlSegment) && location.origin == this.frontEndOrigin) {
            let headers = req.headers;

            const requestToken = CookieService.GetCookie("XSRF-REQUEST-TOKEN");
            if (requestToken) {
                headers = headers.set("X-XSRF-TOKEN", requestToken);
            }

            req = req.clone({
                url: `${this.backEndOrigin}/${req.url}`,
                headers: headers,
                withCredentials: true
            });
        }

        return next.handle(req);
    }
}