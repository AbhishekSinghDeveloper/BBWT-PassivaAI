import { IEntity } from "@bbwt/interfaces";
import { EntityId } from "@bbwt/interfaces/entity";
import { ICustomer } from "./customer";
import { IEmployee } from "./employee";
import { IOrderDetails } from "./order-details";

export interface IOrder extends IEntity {
    customerCode: string;
    orderDate?: Date;
    requiredDate?: Date;
    shippedDate?: Date;
    isPaid: boolean;
    customerId?: EntityId;
    customer: ICustomer;
    employeeId?: EntityId;
    employee: IEmployee;
    orderDetails: IOrderDetails[];
}
