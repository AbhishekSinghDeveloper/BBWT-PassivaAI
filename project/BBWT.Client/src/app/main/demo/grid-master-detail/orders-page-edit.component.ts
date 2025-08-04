import { Component } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { IOrder, OrderService } from "@demo/northwind";


@Component({
    templateUrl: "./orders-page-edit.component.html",
    styleUrls: ["./orders-page-edit.component.scss"]
})
export class AddEditOrdersComponent {
    order: IOrder = {} as any;
    isColon: boolean;
    allLabelsRightAligned: boolean;
    firstLabelRightAligned: boolean;


    constructor(private router: Router,
                private route: ActivatedRoute,
                private orderService: OrderService) {
        route.params.subscribe(params => {
            const id = params["id"];
            if (id) {
                this.orderService.get(id).then(order => this.order = order);
            }
        });
    }


    back(): void {
        this.router.navigate(["/app/demo/grid-master-detail/page"]);
    }

    save(): void {
        if (this.order.id) {
            this.orderService.update(this.order.id, this.order).then(() => this.back());
        } else {
            this.orderService.create(this.order).then(() => this.back());
        }
    }
}