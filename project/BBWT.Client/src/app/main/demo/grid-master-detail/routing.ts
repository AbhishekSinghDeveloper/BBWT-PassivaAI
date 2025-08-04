import { CanDeactivateGuard } from "@bbwt/guards/can-deactivate.guard";
import {
    AddEditOrdersComponent, OrdersInlineComponent, OrdersPageComponent, OrdersPopupComponent, OrdersPageDetailsComponent
} from "./";

export const gridMasterDetailsRoute = {
    path: "grid-master-detail",
    children: [
        {
            path: "popup",
            component: OrdersPopupComponent,
            data: { title: "Popup Edit" }
        },
        {
            path: "create",
            component: AddEditOrdersComponent,
            data: { title: "Add Order" }
        },
        {
            path: "edit/:id",
            component: AddEditOrdersComponent,
            data: { title: "Edit Order" }
        },
        {
            path: "details/:id",
            component: OrdersPageDetailsComponent,
            data: { title: "Order Details" }
        },
        {
            path: "page",
            component: OrdersPageComponent,
            data: { title: "Page Edit" }
        },
        {
            path: "inline",
            component: OrdersInlineComponent,
            data: { title: "Inline Edit" },
            canDeactivate: [CanDeactivateGuard]
        }
    ]
};