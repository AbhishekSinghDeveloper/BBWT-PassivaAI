import { OrdersComponent } from "./orders.component";
import { OrderDetailsComponent } from "./order-details.component";

export const idHashingRoute = {
    path: "id-hashing",
    children: [
        {
            path: "",
            component: OrdersComponent,
            data: { title: "ID Hashing" }
        },
        {
            path: "details/:id",
            component: OrderDetailsComponent,
            data: { title: "Order details" }
        }
    ]
};
