import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { EmailTemplate } from "./email-template";
import { PagedCrudService } from "@features/grid";
import { Email } from "./email";


@Injectable()
export class EmailTemplateService extends PagedCrudService<EmailTemplate> {
    readonly url = "api/email-template";
    readonly entityTitle = "Email Template";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    checkCodeUrl(): string {
        return this.url + "/check-code";
    }

    sendTestEmail(email: Email, attachments: File[]) {
        const formData: FormData = new FormData();

        for (let i = 0; i < attachments.length; i++) {
            formData.append(i.toString(), attachments[i], attachments[i].name);
        }

        return this.httpPost(`${email.emailTemplateId}/test-email?to=${email.to}`, formData, this.noHandler);
    }
}