import { IEntity } from "@bbwt/interfaces";

export interface SimpleOrder extends IEntity {
    customerCompanyName: string;
    isPaid: boolean;
}
