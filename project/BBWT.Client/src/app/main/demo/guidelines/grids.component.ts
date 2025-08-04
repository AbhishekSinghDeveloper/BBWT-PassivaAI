import { Component, OnInit } from "@angular/core";
import { LazyLoadEvent } from "primeng/api";
import { PageResult } from "@bbwt/modules/data-service";
import { IQueryCommand, QueryCommand, DateFilter, NumberFilter, StringFilter } from "@features/filter";
import { IOrder, OrderService} from "@demo/northwind";

@Component({
    selector: "grids",
    templateUrl: "./grids.component.html"
})
export class GridsComponent implements OnInit {
    queryCommand: IQueryCommand;
    filter: { customerCode: string, id: number, orderDate: Date, requiredDate: Date, shippedDate: Date };
    selectedOrders: IOrder[];
    page: PageResult<IOrder> = {
        items: [],
        total: 0
    };

    constructor(private orderService: OrderService) {}

    ngOnInit() {
        this.selectedOrders = [];
        this.initDefaultQueryCommand();
        this.load(this.queryCommand);
    }

    onShowSelected() {
        let str = "";
        this.selectedOrders.forEach(item => {
            str += item.id.toString() + ";";
        });
        alert(str);
    }

    load(queryCommand: IQueryCommand) {
        this.orderService.getPage(queryCommand).then((result: PageResult<IOrder>) => {
            if (result) {
                this.page.items = result.items;
                this.page.total = result.total;
            }
        });
    }

    onGridLazyLoad(event: LazyLoadEvent) {
        // Map grid lazy load event to filter
        this.queryCommand = new QueryCommand(event);
        this.fillFilters();

        // Load data
        this.load(this.queryCommand);
    }

    clearFilter() {
        this.initDefaultQueryCommand();
        this.load(this.queryCommand);
    }

    search() {
        this.fillFilters();
        this.load(this.queryCommand);
    }

    private initDefaultQueryCommand() {
        this.queryCommand = {
            skip: 0,
            take: 10,
            sortingDirection: 1,
            filters: []
        };

        this.filter = {
            customerCode: null,
            id: null,
            orderDate: null,
            requiredDate: null,
            shippedDate: null
        };
    }

    private fillFilters() {
        this.queryCommand.filters = [];

        if (this.filter.id) {
            this.queryCommand.filters.push(new NumberFilter("id", this.filter.id));
        }

        if (this.filter.customerCode) {
            this.queryCommand.filters.push(new StringFilter("CustomerCode", this.filter.customerCode));
        }

        if (this.filter.orderDate) {
            this.queryCommand.filters.push(new DateFilter("OrderDate", this.filter.orderDate));
        }

        if (this.filter.shippedDate) {
            this.queryCommand.filters.push(new DateFilter("ShippedDate", this.filter.shippedDate));
        }

        if (this.filter.requiredDate) {
            this.queryCommand.filters.push(new DateFilter("RequiredDate", this.filter.requiredDate));
        }
    }
}