import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {FormIODefinition} from "@features/bb-formio";
import {FormDefinitionParameters, FormIODefinitionPageDTO} from "@features/bb-formio/dto/form-definition";
import {PagedCrudService} from "@features/grid";
import {PublishFormDTO} from "../dto/publishFormDTO";
import {BooleanFilter, IQueryCommand, StringFilter} from "@features/filter";
import {UserService} from "@main/users";
import {NumberArrayFilter} from "@features/filter/classes/number-array-filter";

@Injectable()
export class FormioViewerService extends PagedCrudService<FormIODefinition> {
    public readonly url = "api/formio";
    public readonly entityTitle = "FormIO form definition";

    constructor(http: HttpClient, private userService: UserService, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    public extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberArrayFilter("orgIds", this.GetOrgIds()));
        queryCommand.filters.push(new BooleanFilter("isAdmin", this.GetIsAdmin()));
        queryCommand.filters.push(new StringFilter("userId", this.GetUserId()));
    }

    // TODO: review
    async CopyForm(id: number) {
        return await this.handleRequest<number>(
            this.http.post<number>(`${this.url}/copy-form/${id}`, null),
            this.handlersFactory.getDefault());
    }

    async getFormJson(id: number, readOnly: boolean, body: FormDefinitionParameters): Promise<FormIODefinition> {
        return await this.handleRequest<FormIODefinition>(
            this.http.post<FormIODefinition>(`${this.url}/form-definition/${id}/${readOnly}`, body),
            this.handlersFactory.getDefault());
    }

    async PublishForm(form: PublishFormDTO) {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/publish`, form),
            this.handlersFactory.getForUpdate("Form publishing",
                {successMessage: "The form has being published to the selected organizations."}));
    }

    async ChangeOwnership(form: number, user: string) {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/owner`, {newOwnerId: user, formDefinitionId: form}),
            this.handlersFactory.getForUpdate("Form ownership",
                {successMessage: "The form has changed ownership."}));
    }

    async GetAvailableVersions() {
        return await this.handleRequest<string[]>(
            this.http.get<string[]>(`${this.url}/available-versions`,
                {
                    params:
                        {
                            "orgIds": this.GetOrgIds(),
                            "isAdmin": this.GetIsAdmin(),
                            "userId": this.GetUserId()
                        }
                }
            ),
            this.handlersFactory.getDefault()
        );
    }

    private GetOrgIds = () => this.userService.currentUser.organizations.map(x => x.id_original);
    private GetUserId = () => this.userService.currentUser.id;
    private GetIsAdmin = () => this.userService.currentUser.isSuperAdmin || this.userService.currentUser.isSystemAdmin;
}
