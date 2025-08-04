import { IEntity } from "@bbwt/interfaces";

export interface OrderDetails extends IEntity {
    productTitle: string;
    quantity: number;
    price: number;
    isReseller: boolean;
}
