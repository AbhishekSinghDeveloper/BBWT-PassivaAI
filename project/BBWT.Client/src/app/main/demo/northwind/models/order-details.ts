import { IEntity } from "@bbwt/interfaces";
import { EntityId } from "@bbwt/interfaces/entity";
import { IProduct } from "./product";

export interface IOrderDetails extends IEntity {
    orderId: EntityId;
    productId: EntityId;
    product: IProduct;
    price: number;
    quantity: number;
    isReseller: boolean;
}
