import { IEntity } from "@bbwt/interfaces";

export interface Customer extends IEntity {
    code: string;
    companyName: string;
}
