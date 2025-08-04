import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {CrudService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {FormIOData} from "../dto/form-data";
import {EntityId} from "@bbwt/interfaces";
import {FormFieldDataUpdate} from "@features/bb-formio/components/bb-form-version-handler/bb-form-version-handler.models";

@Injectable()
export class FormIODataService extends CrudService<FormIOData> {
    public readonly url = "api/formio-data";
    public readonly entityTitle = "FormIO";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    async sendFormDataJson(formData: FormIOData): Promise<boolean> {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/form-data`, formData),
            this.handlersFactory.getDefault());
    }

    async getFormDataJson(id: string): Promise<string> {
        return await this.handleRequest<string>(
            this.http.post<string>(`${this.url}/form-data/${id}`, null),
            this.handlersFactory.getDefault());
    }

    async checkIfFormHasData(formDefinitionId: EntityId): Promise<boolean> {
        return await this.handleRequest<boolean>(
            this.http.get<boolean>(`${this.url}/has-data/${formDefinitionId}`),
            this.handlersFactory.getDefault()
        )
    }

    async updateFormData(formDefinitionId: EntityId, updates: FormFieldDataUpdate[]): Promise<void> {
        return await this.httpPost<void>(`update-form-data/${formDefinitionId}`, updates);
    }
}
