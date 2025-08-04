import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { PagedCrudService } from "@features/grid";
import { FormCategoryDTO } from "../dto/formCategoryDTO";

@Injectable()
export class FormIOCategoryService extends PagedCrudService<FormCategoryDTO>{
    public readonly url = "api/formio-category";
    public readonly entityTitle = "FormIO Category";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    async getAllCategories(): Promise<FormCategoryDTO[]> {
        return await this.handleRequest<FormCategoryDTO[]>(
            this.http.get<FormCategoryDTO[]>(`${this.url}/all`),
            this.handlersFactory.getDefault());
    }
}