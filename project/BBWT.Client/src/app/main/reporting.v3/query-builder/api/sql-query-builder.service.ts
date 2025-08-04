import {HttpClient} from "@angular/common/http";
import {Injectable} from "@angular/core";

import {BaseDataService, HttpResponsesHandlersFactory} from "@bbwt/modules/data-service";
import {ISqlQueryBuild} from "../query-builder-models";

@Injectable({
    providedIn: "root"
})
export class SqlQueryBuilderService extends BaseDataService {
    readonly url = "api/reporting3/query/sql";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getBuild(querySourceId: string): Promise<ISqlQueryBuild> {
        return this.httpGet(`${querySourceId}/build`);
    }

    create(queryBuild: ISqlQueryBuild): Promise<ISqlQueryBuild> {
        return this.httpPost(null, queryBuild);
    }

    createDraft(queryBuild: ISqlQueryBuild, querySourceReleaseId?: string): Promise<ISqlQueryBuild> {
        return this.httpPost(`create-draft${querySourceReleaseId ? "/" + querySourceReleaseId : ""}`, queryBuild);
    }

    releaseDraft(querySourceDraftId: string): Promise<string> {
        return this.httpPut(`release-draft/${querySourceDraftId}`);
    }

    update(id: string, queryBuild: ISqlQueryBuild): Promise<ISqlQueryBuild> {
        return this.httpPut(`${id}`, queryBuild);
    }
}