import {Injectable} from "@angular/core";
import {HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse} from "@angular/common/http";
import {Observable, throwError} from "rxjs";
import {catchError} from "rxjs/operators";
import {Router} from "@angular/router";

@Injectable()
export class DashboardAuthorizationInterceptor implements HttpInterceptor {
    private readonly url = "api/reporting3/dashboard";

    constructor(private router: Router) {
    }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        if (!request.url.startsWith(this.url)) return next.handle(request);

        return next.handle(request).pipe(catchError((error: HttpErrorResponse) => {
                if (error.status === 403) {
                    this.router.navigate(["/app/reporting3/dashboards"]).then();
                }
                return throwError(() => error);
            })
        );
    }
}