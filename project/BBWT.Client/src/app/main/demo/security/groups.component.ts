import { Component, ViewChild } from "@angular/core";

import { GroupsService } from "./groups.service";
import { notEmptyValidator } from "@bbwt/modules/validation";
import {
    GridComponent,
    GridValidator,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";


@Component({
    selector: "groups",
    templateUrl: "./groups.component.html"
})
export class GroupsComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name",
                validators: [new GridValidator(notEmptyValidator)]
            }
        ]
    };
    gridSettings: IGridSettings = {
        exportEnabled: true,
        updateMode: UpdateMode.Inline,
        filtersRow: true
    };

    @ViewChild(GridComponent, { static: true }) grid: GridComponent;


    constructor(public groupService: GroupsService) {
        this.gridSettings.dataService = groupService;
    }
}