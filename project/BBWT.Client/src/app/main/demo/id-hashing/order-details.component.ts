import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { ValidationPatterns } from "@bbwt/modules/validation";
import { Order } from "./models";
import { IdHashingDemoService } from "./services";
import { CustomerService } from "../northwind";
import { EntityId } from "@bbwt/interfaces";
import { MessageService } from "@main/admin/services";
import { Message } from "@bbwt/classes";
import { firstValueFrom } from "rxjs";

@Component({
    selector: "order-hashing-details",
    templateUrl: "./order-details.component.html"
})
export class OrderDetailsComponent implements OnInit {
    order: Order;
    today: Date;
    customerOptions: { label: string; value: EntityId }[];
    loadingOrder = true;

    get _ValidationPatterns(): any {
        return ValidationPatterns;
    }

    constructor(
        private ordersService: IdHashingDemoService,
        private router: Router,
        private route: ActivatedRoute,
        private customersService: CustomerService,
        private messageService: MessageService
    ) {}

    async ngOnInit() {
        const params = await firstValueFrom(this.route.params);
        this.customersService.getAll().then(customers => {
            this.customerOptions = customers.map(customer => ({
                label: customer.code,
                value: customer.id
            }));
        });

        const id = params.id;
        this.today = new Date();

        if (!!id) {
            this.order = await this.ordersService.get(id).finally(() => (this.loadingOrder = false));
        } else {
            this.messageService.add(Message.Error("Order ID missing."));
            this.router.navigate(["../.."]);
        }
    }

    async save() {
        const item = this.customerOptions.filter(option => option.value === this.order?.customerId);
        if (item.length && this.order) {
            this.order.customerCode = item[0].label;
        }

        if (!this.order.id || +this.order.id === 0) {
            await this.ordersService.create(this.order);
        } else {
            await this.ordersService.update(this.order.id, this.order);
        }
        await this.back();
    }

    async back() {
        await this.router.navigate(["/app/demo/id-hashing"]);
    }
}
