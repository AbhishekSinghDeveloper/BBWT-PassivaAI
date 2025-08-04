import { Component } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { OrderService, IOrder } from "@demo/northwind";


@Component({
    templateUrl: "./orders-page-details.component.html"
})
export class OrdersPageDetailsComponent {
    order: IOrder;

    constructor(private orderService: OrderService, private router: Router, private route: ActivatedRoute) {
        this.route.params.subscribe(params => {
            const id = params["id"];
            this.orderService.get(id).then(data => {
                this.order = data;
            });
        });
    }

    back(): void {
        this.router.navigate(["/app/demo/grid-master-detail/page"]);
    }
}