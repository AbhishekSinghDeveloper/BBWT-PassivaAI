import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { EmailTemplateParameter } from "./email-template-parameter";
import { PagedCrudService } from "@features/grid";


@Injectable()
export class EmailTemplateParameterService extends PagedCrudService<EmailTemplateParameter> {
    readonly url = "api/email-template-parameter";
    readonly entityTitle = "Email Template Parameter";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }
}