import { HttpClient } from "@angular/common/http";

import { CrudService } from "@bbwt/modules/data-service/crud.service";
import {
    HttpResponsesHandlersFactory,
    IHttpResponseHandlerSettings
} from "@bbwt/modules/data-service/http-responses-handler";
import { IPagedCrudService } from "../interfaces/paged-crud-service";
import { IDateFilter, IQueryCommand } from "../../filter";
import { IPagedData } from "../interfaces/paged-data";
import { SetLocalTimeZone, SetUTCTimeZone } from "@bbwt/utils";



export abstract class PagedCrudService<TEntity> extends CrudService<TEntity> implements IPagedCrudService<TEntity> {
    readonly type: string;


    protected constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
        this.type = "pagedCrudService";
    }


    getPage(queryCommand: IQueryCommand, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<IPagedData<TEntity>> {
        if (!this.url) {
            Promise.resolve(<IPagedData<TEntity>>{ items: [], total: 0 });
            return;
        }

        if (this.extendQueryCommand) {
            this.extendQueryCommand(queryCommand);
        }

        this.prepareQueryCommandDates(queryCommand);

        return this.handle<IPagedData<TEntity>>(
            this.http.get<IPagedData<TEntity>>(`${this.url}/page`, { params: this.constructHttpParams(queryCommand) }),
            this.handlersFactory.getForReadByFilter(responseHandlerSettings)
        ).then(result => {
            result.items.forEach(item =>
                this.modelUtcDateFields.forEach(utcField => item[utcField] = SetLocalTimeZone(item[utcField])));
            return result;
        });
    }

    getPageGeneric<TPageEntity>(pageUriFragment: string, queryCommand: IQueryCommand, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<IPagedData<TPageEntity>> {
        this.prepareQueryCommandDates(queryCommand);

        return this.handle<IPagedData<TPageEntity>>(
            this.http.get<IPagedData<TPageEntity>>(`${this.url}/${pageUriFragment}`, { params: this.constructHttpParams(queryCommand) }),
            this.handlersFactory.getForReadByFilter(responseHandlerSettings)
        ).then(result => {
            result.items.forEach(item =>
                this.modelUtcDateFields.forEach(utcField => item[utcField] = SetLocalTimeZone(item[utcField])));
            return result;
        });
    }

    // To be overridden in child services (or components e.g. in ngOnInit to pass some data from the component to as the default filter conditions).
    // E.g.you might want to append some custom filter conditions without overriding of the getPage method
    public extendQueryCommand(queryCommand: IQueryCommand) { }


    private prepareQueryCommandDates(queryCommand: IQueryCommand): void {
        this.modelUtcDateFields.forEach(utcField => {
            const filter = queryCommand.filters.find(filterItem => filterItem.propertyName == utcField) as IDateFilter;
            if (filter && filter.value) {
                filter.value = SetUTCTimeZone(filter.value);
            }
        });
    }
}