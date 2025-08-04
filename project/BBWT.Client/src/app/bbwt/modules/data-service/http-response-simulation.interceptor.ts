import {
    HttpErrorResponse,
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest,
    HttpResponse
} from "@angular/common/http";
import { Injectable, Injector } from "@angular/core";

import { Observable, of, throwError } from "rxjs";

import { AppStorage } from "../../utils/app-storage";
import { UserService } from "@main/users/user.service";

@Injectable()
export class HttpResponseSimulationInterceptor implements HttpInterceptor {
    constructor(private injector: Injector) {}

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const userService = this.injector.get(UserService);
        if (req.url.includes("api/") && !req.url.includes("api/account") && userService && userService.isLogged) {
            const savedHttpSimulatedCode = parseInt(AppStorage.getItem(AppStorage.SimulatedHttpStatusCodeKey));

            if (!Number.isNaN(savedHttpSimulatedCode)) {
                const responseObject = {
                    status: savedHttpSimulatedCode,
                    url: req.url,
                    statusText: "Simulated response."
                };

                if (savedHttpSimulatedCode < 400) {
                    return of(new HttpResponse(responseObject));
                } else {
                    return throwError(() => new HttpErrorResponse(responseObject));
                }
            }
        }

        return next.handle(req);
    }
}
