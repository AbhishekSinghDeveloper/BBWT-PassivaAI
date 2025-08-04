import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { IOrder } from "@demo/northwind";
import { SecurityAccessibleService } from "./security-accessible.service";


@Component({
    selector: "demo-security-groups",
    templateUrl: "./accessible-to-group.component.html"
})
export class AccessibleToGroupComponent {
    group: string;
    specifedOrder: IOrder;
    isBusySpecifedOrder: boolean;


    constructor(private service: SecurityAccessibleService, private route: ActivatedRoute) {
        route.params.subscribe(params => {
            this.group = params["group"].toUpperCase();

            this.loadOrder();
        });
    }


    private loadOrder () {
        this.isBusySpecifedOrder = true;
        this.service.getByGroupNameForAccessibleToGroup(this.group).then((result) => {
            this.specifedOrder = result;
            this.isBusySpecifedOrder = false;
        }).catch(() => this.isBusySpecifedOrder = false);
    }
}