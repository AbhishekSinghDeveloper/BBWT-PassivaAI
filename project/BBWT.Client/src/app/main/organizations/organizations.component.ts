import { Component } from "@angular/core";

import { CreateMode, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";
import { OrganizationService } from "./organization.service";


@Component({
    selector: "organizations",
    templateUrl: "./organizations.component.html"
})
export class OrganizationsComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "name", header: "Name" },
            { field: "description", header: "Description" }
        ]
    };
    gridSettings: IGridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/organizations/edit/0",
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/organizations/edit/:id",
        exportEnabled: false,
        filtersRow: true
    };


    constructor(organizationService: OrganizationService) {
        this.gridSettings.dataService = organizationService;
    }
}