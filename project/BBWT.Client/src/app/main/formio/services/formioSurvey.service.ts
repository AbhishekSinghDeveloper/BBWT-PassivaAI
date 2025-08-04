import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {FormRevisionSuggestionDTO, SurveyDTO, SurveyFormDataDTO, UserSuggestionDTO} from "@features/bb-formio/dto/form-survey";
import {BooleanFilter, IQueryCommand, QueryCommand, StringFilter} from "@features/filter";
import {NumberArrayFilter} from "@features/filter/classes/number-array-filter";
import {PagedCrudService} from "@features/grid";
import {UserService} from "@main/users";

@Injectable()
export class FormIOSurveyService extends PagedCrudService<SurveyDTO> {
    public readonly url = "api/formio-survey";
    public readonly entityTitle = "FormIO Surveys";

    constructor(http: HttpClient, private userService: UserService, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    queryFilter(): QueryCommand {
        const queryCommand = <QueryCommand>{filters: []};
        queryCommand.filters.push(new NumberArrayFilter("orgIds", this.userService.currentUser.organizations.map(x => x.id_original)));
        queryCommand.filters.push(new BooleanFilter("isAdmin", this.userService.currentUser.isSuperAdmin || this.userService.currentUser.isSystemAdmin));
        queryCommand.filters.push(new StringFilter("userId", this.userService.currentUser.id));

        return queryCommand;
    }

    extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new NumberArrayFilter("orgIds", this.userService.currentUser.organizations.map(x => x.id_original)));
        queryCommand.filters.push(new BooleanFilter("isAdmin", this.userService.currentUser.isSuperAdmin || this.userService.currentUser.isSystemAdmin));
        // queryCommand.filters.push(new StringFilter("userID", this.userService.currentUser.id));
    }

    async getAllUserSuggestions(): Promise<UserSuggestionDTO[]> {
        return this.handle<UserSuggestionDTO[]>(
            this.http.get<UserSuggestionDTO[]>(`${this.url}/get-all-users`, {params: this.constructHttpParams(this.queryFilter())}),
            this.handlersFactory.getDefault())
    }

    async getAllFormsSuggestions(): Promise<FormRevisionSuggestionDTO[]> {
        return this.handle<FormRevisionSuggestionDTO[]>(
            this.http.get<FormRevisionSuggestionDTO[]>(`${this.url}/get-all-forms`, {params: this.constructHttpParams(this.queryFilter())}),
            this.handlersFactory.getDefault())
    }

    async getSurveyData(id: string): Promise<SurveyFormDataDTO[]> {

        return this.handle<SurveyFormDataDTO[]>(
            this.http.get<SurveyFormDataDTO[]>(`${this.url}/get-survey-data/${id}`),
            this.handlersFactory.getDefault())
    }

}