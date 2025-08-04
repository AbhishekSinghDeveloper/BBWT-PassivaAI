import { HttpClient, HttpErrorResponse, HttpParams } from "@angular/common/http";

import { Observable, firstValueFrom } from "rxjs";

import { HttpResponsesHandlersFactory, IHttpResponseHandlerSettings, IHttpResponsesHandler } from "./http-responses-handler";
import { flatten } from "../../utils";

export abstract class BaseDataService {
    protected constructor(protected http: HttpClient, protected handlersFactory: HttpResponsesHandlersFactory) {}

    readonly abstract url: string;

    protected noHandler: IHttpResponsesHandler = this.handlersFactory.getBypass();

    protected handle<TResult>(
        request: Observable<TResult>,
        responseHandler?: IHttpResponsesHandler
    ): Promise<TResult> {
        const promise = firstValueFrom(request);
        const handler = responseHandler ?? this.handlersFactory.getDefault();

        if (handler == this.noHandler) {
            return promise;
        } else {
            return promise
                .then((response: TResult) => {
                    if (handler.onSuccess) {
                        handler.onSuccess();
                    }
                    return Promise.resolve(response);
                })
                .catch((errorResponse: HttpErrorResponse) => {
                    if (handler.onError) {
                        handler.onError(errorResponse);
                    }
                    return Promise.reject(errorResponse);
                });
        }
    }

    protected fullUrl(actionUrl: string): string {
        let rightPart = actionUrl ? actionUrl : "";
        if (rightPart.length > 0 && !rightPart.startsWith("?") && !rightPart.startsWith("/")) {
            rightPart = "/" + rightPart;
        }
        return `${this.url}${rightPart}`;
    }

    protected httpGet<TResult>(actionUrl?: string, responseHandler?: IHttpResponsesHandler): Promise<TResult> {
        return this.handle(this.http.get<TResult>(this.fullUrl(actionUrl)), responseHandler);
    }

    protected httpGetByUrl<TResult>(url: string, responseHandler?: IHttpResponsesHandler): Promise<TResult> {
        return this.handle(this.http.get<TResult>(url), responseHandler);
    }

    protected httpPost<TResult>(actionUrl?: string, body?: any, responseHandler?: IHttpResponsesHandler): Promise<TResult> {
        return this.handle(this.http.post<TResult>(this.fullUrl(actionUrl), body), responseHandler);
    }

    protected httpPut<TResult>(actionUrl?: string, body?: any, responseHandler?: IHttpResponsesHandler): Promise<TResult> {
        return this.handle(this.http.put<TResult>(this.fullUrl(actionUrl), body), responseHandler);
    }

    protected httpDelete<TResult>(actionUrl?: string, responseHandler?: IHttpResponsesHandler): Promise<TResult> {
        return this.handle(this.http.delete<TResult>(this.fullUrl(actionUrl)), responseHandler);
    }

    protected defaultHandler(settings?: IHttpResponseHandlerSettings): IHttpResponsesHandler {
        return this.handlersFactory.getDefault(settings);
    };

    // We have to save this due to HttpParamsOptions.fromObject limitations,
    // Because it works correctly with string maps only.
    // See HttpParamsOptions definition for details.
    protected constructHttpParams(obj: any): HttpParams {
        if (!obj) return null;

        let params = new HttpParams();
        const flattenBody = flatten(obj);

        Object.keys(flattenBody).forEach(prop => {
            params = params.set(prop, flattenBody[prop]);
        });

        return params;
    }

    /// Obsolete. Use handle() instead
    protected handleRequest<TResult>(
        request: Observable<TResult>,
        responseHandler?: IHttpResponsesHandler
    ): Promise<TResult> {
        const promise = firstValueFrom(request);

        if (responseHandler) {
            return promise
                .then((response: TResult) => {
                    if (responseHandler.onSuccess) {
                        responseHandler.onSuccess();
                    }
                    return Promise.resolve(response);
                })
                .catch((errorResponse: HttpErrorResponse) => {
                    if (responseHandler.onError) {
                        responseHandler.onError(errorResponse);
                    }
                    return Promise.reject(errorResponse);
                });
        } else {
            return promise;
        }
    }
}
