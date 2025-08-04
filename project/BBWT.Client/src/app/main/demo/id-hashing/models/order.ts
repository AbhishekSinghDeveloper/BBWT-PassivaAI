import { IEntity } from "@bbwt/interfaces";
import { Customer, OrderDetails } from ".";
import * as moment from "moment";

export interface Order extends IEntity {
    customerCode: string;
    customerId: number;
    customerCompanyName: string;
    orderDate?: moment.Moment;
    requiredDate?: moment.Moment;
    shippedDate?: moment.Moment;
    isPaid: boolean;
    hasResellerItems: boolean;
    customer: Customer;
    orderDetails: OrderDetails[];
}
