import { Component, ViewChild } from "@angular/core";
import { Validators } from "@angular/forms";

import { SelectItem } from "primeng/api";

import { RoutesService } from "@main/routes";
import {
    CellEditInputType, GridComponent,
    GridValidator,
    IGridColumn,
    IGridSettings,
    ITableSettings
} from "@features/grid";
import { FooterMenuService } from "./footer-menu.service";
import { notEmptyValidator } from "@bbwt/modules/validation";


@Component({
    selector: "footer-menu-designer",
    templateUrl: "footer-menu-designer.component.html"
})
export class FooterMenuDesignerComponent {
    tableSettings: ITableSettings;
    gridSettings: IGridSettings = {
        dataServiceGetPageMethodName: "getAll",
        sortEnabled: false,
        reorderableRows: true
    };
    @ViewChild("grid") private grid: GridComponent;


    constructor(private footerMenuService: FooterMenuService,
                private routesService: RoutesService) {
        this.init();
    }


    onRowReorder($event: any): void {
        this.grid.getTableProperty("value").forEach((item, index) => item.orderNo = index + 1);
        this.footerMenuService.updateOrderOfItems(this.grid.getTableProperty("value"))
            .then(() => this.grid.reload());
    }


    private async init(): Promise<void> {
        this.gridSettings.dataService = this.footerMenuService;

        this.tableSettings = {
            columns: <IGridColumn[]>[
                {
                    field: "name",
                    header: "Name",
                    validators: [ new GridValidator(Validators.required), new GridValidator(notEmptyValidator()) ]
                },
                {
                    field: "routerLink",
                    header: "Router Link",
                    cellEditingInputType: CellEditInputType.Dropdown,
                    dropdownOptions: (await this.routesService.getPageRoles())
                        .map(x => <SelectItem>{ label: x.path, value: x.path })
                        .sort((a, b) => a.label.localeCompare(b.label)),
                    dropdownFilterEnabled: true,
                    dropdownFilterBy: "label",
                    placeholder: "Choose a router link"
                }
            ],
            paginator: false
        };
    }
}