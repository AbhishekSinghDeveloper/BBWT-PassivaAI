import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";
import {HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {FormIOData} from "@features/bb-formio";
import {CountableFilterMatchMode, IQueryCommand, NumberFilter, StringFilter} from "@features/filter";
import {PagedCrudService} from "@features/grid";
import {UserService} from "@main/users";

@Injectable()
export class FormioPendingSurveysService extends PagedCrudService<FormIOData> {

    public readonly url = "api/formio-data";
    public readonly entityTitle = "FormIO form data";
    public formRevisionId: number = -1;

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory, private userservice: UserService) {
        super(http, handlersFactory);
    }

    public extendQueryCommand(queryCommand: IQueryCommand) {
        queryCommand.filters = queryCommand.filters || [];
        queryCommand.filters.push(new StringFilter("UserID", this.userservice.currentUser.id))
        queryCommand.filters.push(new StringFilter("Json", "{}"))
        queryCommand.filters.push(new NumberFilter("SurveyId", 0, CountableFilterMatchMode.GreaterThan))

    }
}