import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {CrudService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {FormIOData, FormIODataDraft} from "../dto/form-data";
import {EntityId} from "@bbwt/interfaces";

@Injectable()
export class FormIODataDraftService extends CrudService<FormIOData> {
    public readonly url = "api/formio-data-draft";
    public readonly entityTitle = "FormIO Draft";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    async sendFormDataDraftJson(formDataDraft: FormIODataDraft): Promise<FormIODataDraft> {
        return await this.handleRequest<FormIODataDraft>(
            this.http.post<FormIODataDraft>(`${this.url}/draft-data`, formDataDraft),
            this.handlersFactory.getDefault());
    }

    async getFormDataDraftJson(formDefinitionId: EntityId, userId: string): Promise<FormIODataDraft> {
        return await this.handleRequest<FormIODataDraft>(
            this.http.post<FormIODataDraft>(`${this.url}/draft-data/${formDefinitionId}/${userId}`, null),
            this.handlersFactory.getDefault());
    }

    async removeFormDataDraft(id: EntityId): Promise<string> {
        return await this.handleRequest<string>(
            this.http.get<string>(`${this.url}/remove-draft/${id}`),
            this.handlersFactory.getDefault());
    }
}
