import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseDataService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { JobExecutionDetails, ServerInfo, TotalPage } from "./JobExecutionDetails";
import { IQueryCommand } from "@features/filter";
import { from, Observable } from "rxjs";

@Injectable({
    providedIn: "root"
})
export class SchedulerService extends BaseDataService {
    readonly url = "api/scheduler";
    readonly entityTitle = "JobExecutionDetails";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    getOverview(view?: string): Promise<any> {
        return this.handleRequest<any>(
            this.http.get<any>(`${this.url}/overview/${view}`),
            this.handlersFactory.getForReadAll()
        );
    }

    getJobsByStatus(status: string, queryCommand?: IQueryCommand): Promise<TotalPage> {
        return this.handleRequest<TotalPage>(
            this.http.get<TotalPage>(`${this.url}/${status}`, { params: this.constructHttpParams(queryCommand) }),
            this.handlersFactory.getForReadAll()
        );
    }

    getJobDetails(jobId: number): Promise<JobExecutionDetails> {
        return this.handleRequest<JobExecutionDetails>(
            this.http.get<JobExecutionDetails>(`${this.url}/details/${jobId}`),
            this.handlersFactory.getForReadAll()
        );
    }

    pauseJob(jobId: number): Promise<void> {
        return this.handleRequest<void>(
            this.http.post<void>(`${this.url}/pause/${jobId}`, jobId),
            this.handlersFactory.getForReadAll()
        );
    }

    resumeJob(jobId: number): Promise<void> {
        return this.handleRequest<void>(
            this.http.post<void>(`${this.url}/resume/${jobId}`, jobId),
            this.handlersFactory.getForReadAll()
        );
    }

    retriesJob(jobId: number): Promise<void> {
        return this.handleRequest<void>(
            this.http.post<void>(`${this.url}/retries/${jobId}`, jobId),
            this.handlersFactory.getForReadAll()
        );
    }

    deleteJob(jobId: number): Promise<void> {
        return this.handleRequest<void>(
            this.http.delete<void>(`${this.url}/${jobId}`),
        );
    }

    triggerJob(jobId: number): Promise<void> {
        return this.handleRequest<void>(
            this.http.post<void>(`${this.url}/trigger/${jobId}`, jobId),
            this.handlersFactory.getForReadAll()
        );
    }

    getServers(queryCommand?: IQueryCommand): Promise<ServerInfo[]> {
        return this.handleRequest<ServerInfo[]>(
            this.http.get<ServerInfo[]>(`${this.url}/servers`, { params: this.constructHttpParams(queryCommand) })
        );
    }

    getRecurring(queryCommand?: IQueryCommand): Promise<JobExecutionDetails[]> {
        return this.handleRequest<JobExecutionDetails[]>(
            this.http.get<JobExecutionDetails[]>(`${this.url}/recurring`, { params: this.constructHttpParams(queryCommand) })
        );
    }

    getRetries(queryCommand?: IQueryCommand): Promise<JobExecutionDetails[]> {
        return this.handleRequest<JobExecutionDetails[]>(
            this.http.get<JobExecutionDetails[]>(`${this.url}/retried`, { params: this.constructHttpParams(queryCommand) })
        );
    }

    saveJob(jobName: string, cronExpression: string): Promise<boolean> {
        const requestPayload = { JobName: jobName, CronExpression: cronExpression };
        return this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/saveJob`, requestPayload)
        );
      }
      
    ruleExists(name: string): Observable<boolean> {
        return from(this.httpGet<boolean>(`exists/${name}`));
    }
}