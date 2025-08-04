import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CrudService, HttpResponsesHandlersFactory } from "@bbwt/modules/data-service";
import { UserSignature, UserSignatureDTO } from "../dto/user-signature";


@Injectable()
export class UserSignatureService extends CrudService<UserSignature> {
    public readonly url = "api/user/usersignature";
    public readonly entityTitle = "User";

    constructor(http: HttpClient, handlersFactory: HttpResponsesHandlersFactory) {
        super(http, handlersFactory);
    }

    async getSignature(userId: string): Promise<UserSignature> {
        return await this.handleRequest<UserSignature>(
            this.http.get<UserSignature>(`${this.url}/${userId}`),
            this.handlersFactory.getDefault());
    }

    async setSignature(userSignature: UserSignatureDTO): Promise<boolean> {
        return await this.handleRequest<boolean>(
            this.http.post<boolean>(`${this.url}/${userSignature.userId}`, { parameterString: [userSignature.signature] }),
            this.handlersFactory.getDefault());
    }
}