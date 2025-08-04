import { IEntity } from "@bbwt/interfaces/entity";

export interface ILoginAudit extends IEntity {
    Datetime: string;
    Ip: string;
    Location: string;
    Fingerprint: string;
    Email: string;
    BrowserInformation: string;
    LoginResult: string;
}