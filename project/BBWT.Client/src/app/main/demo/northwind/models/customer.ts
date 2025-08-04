import { IEntity } from "@bbwt/interfaces";

export interface ICustomer extends IEntity {
    code: string;
    companyName: string;
}