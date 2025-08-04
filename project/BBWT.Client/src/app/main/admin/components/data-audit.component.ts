import { Component } from "@angular/core";

import { SelectItem } from "primeng/api";

import { FilterInputType, FilterType } from "@features/filter";
import {
    CreateMode, DisplayMode,
    GridColumnViewSettings,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    SortOrder,
    UpdateMode
} from "@features/grid";
import { DataAuditService } from "../services";


@Component({
    selector: "data-audit",
    templateUrl: "./data-audit.component.html",
    styleUrls: ["./data-audit.component.scss"]
})
export class DataAuditComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]> [    
            {
                field: "userName",
                header: "User",
                viewSettings: new GridColumnViewSettings({ width: "15%" })
            },
            {
                field: "dateTime",
                header: "Date",
                viewSettings: new GridColumnViewSettings({ width: "10%" }),
                displayMode: DisplayMode.Date,
                displayDateMomentFormat: "L LTS",
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            {
                field: "state",
                header: "State",
                viewSettings: new GridColumnViewSettings({ width: "10%" }),
                filterSettings: {
                    filterType: FilterType.Numeric,
                    inputType: FilterInputType.Dropdown,
                    dropdownOptions: <SelectItem[]>[
                        { label: "Unchanged", value: 1 },
                        { label: "Deleted", value: 2 },
                        { label: "Modified", value: 3 },
                        { label: "Added", value: 4 },
                    ]
                }
            },
            {
                field: "tableName",
                header: "Table",
                viewSettings: new GridColumnViewSettings({ width: "12%" })
            },
            {
                field: "entityId",
                header: "Entity ID",
                viewSettings: new GridColumnViewSettings({ minWidth: "120px" })
            },
            {
                field: "changeLogItems",
                header: "Change Log",
                sortable: false,
                viewSettings: new GridColumnViewSettings({ width: "50%" }),
                filterCellEnabled: false
            }
        ],
        autoLayout: true,
        sortField: "dateTime",
        sortOrder: SortOrder.Desc
    };
    gridSettings: IGridSettings = {
        readonly: true,
        exportEnabled: true,
        filtersRow: true
    };

    readonly rowsPerChangeLogPage = 20;

    constructor(auditChangesService: DataAuditService) {
        this.gridSettings.dataService = auditChangesService;
    }
}