import { HttpClient } from "@angular/common/http";
import { HttpResponsesHandlersFactory, IHttpResponseHandlerSettings } from "./http-responses-handler";
import { BaseDataService } from "./base.data.service";
import { SetUTCTimeZone, SetLocalTimeZone } from "@bbwt/utils";
import { EntityId } from "@bbwt/interfaces";


export abstract class CrudService<TEntity> extends BaseDataService {
    readonly abstract entityTitle: string;

    protected constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    /* Field those time should be time zone independent. */
    protected get modelUtcDateFields(): string[] {
        return []; 
    }

    //#region Read Methods
    get(id: EntityId, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<TEntity> {
        return this.httpGet<TEntity>(`${id}`,
            this.handlersFactory.getForReadById(this.entityTitle, responseHandlerSettings)
        ).then(result => {
            this.modelUtcDateFields.forEach(utcField => result[utcField] = SetLocalTimeZone(result[utcField]));
            return result;
        });
    }

    getAll(responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<TEntity[]> {
        return this.httpGet<TEntity[]>("", this.handlersFactory.getForReadAll(responseHandlerSettings))
            .then(result => {
                result.forEach(item =>
                    this.modelUtcDateFields.forEach(utcField => item[utcField] = SetLocalTimeZone(item[utcField])));
                return result;
            });
    }
    //#endregion

    //#region Edit Methods
    create(item: TEntity, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<TEntity> {
        this.modelUtcDateFields.forEach(utcField => item[utcField] = SetUTCTimeZone(item[utcField]));

        return this.httpPost<TEntity>("", item,
            this.handlersFactory.getForCreate(this.entityTitle, responseHandlerSettings)
        );
    }

    update(id: EntityId, item: TEntity, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<TEntity> {
        this.modelUtcDateFields.forEach(utcField => item[utcField] = SetUTCTimeZone(item[utcField]));

        return this.httpPut<TEntity>(`${id}`, item,
            this.handlersFactory.getForUpdate(this.entityTitle, responseHandlerSettings));
    }

    delete(id: EntityId, responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<any> {
        return this.httpDelete(`${id}`, this.handlersFactory.getForDelete(this.entityTitle, responseHandlerSettings));
    }

    deleteAll(responseHandlerSettings?: IHttpResponseHandlerSettings): Promise<any> {
        return this.httpDelete("", this.handlersFactory.getForDelete(this.entityTitle, responseHandlerSettings));
    }
    //#endregion
}