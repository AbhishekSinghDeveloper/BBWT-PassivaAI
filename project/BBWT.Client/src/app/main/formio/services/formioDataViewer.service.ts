import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {SurveyPendingDTO} from "@features/bb-formio/dto/form-survey-pending";
import {BooleanFilter, IQueryCommand, NumberFilter} from "@features/filter";
import {NumberArrayFilter} from "@features/filter/classes/number-array-filter";
import {PagedCrudService} from "@features/grid";
import {UserService} from "@main/users";

@Injectable()
export class FormioDataViewerService extends PagedCrudService<SurveyPendingDTO> {

    public readonly url = "api/formio-data";
    public readonly entityTitle = "FormIO form data";
    public formDefinitionId: number = -1;

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, private userService: UserService) {
        super(http, handlersFactory);
    }

    public extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberFilter("formDefinitionId", this.formDefinitionId));
        queryCommand.filters.push(new BooleanFilter("isAdmin", this.GetIsAdmin()));
        queryCommand.filters.push(new NumberArrayFilter("organizationId", this.GetOrgIds()));
    }

    async GetAvailableVersions() {
        return await this.handleRequest<string[]>(
            this.http.get<string[]>(`${this.url}/available-versions`,
                {
                    params:
                        {
                            "orgIds": this.GetOrgIds(),
                            "isAdmin": this.GetIsAdmin()
                        }
                }
            ),
            this.handlersFactory.getDefault()
        );
    }

    async DeleteMultiple(idsToDelete: number[]) {
        return await this.handleRequest(
            this.http.delete(`${this.url}/multiple`,
                {
                    params: {
                        "idsToDelete": idsToDelete
                    }
                }
            ),
            this.handlersFactory.getForDelete(this.entityTitle)
        )
    }

    private GetOrgIds = () => this.userService.currentUser.organizations.map(x => x.id_original);
    private GetIsAdmin = () => this.userService.currentUser.isSuperAdmin || this.userService.currentUser.isSystemAdmin;
}
