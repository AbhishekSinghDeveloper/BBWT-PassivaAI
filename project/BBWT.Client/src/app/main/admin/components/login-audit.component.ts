import { Component } from "@angular/core";

import { FilterInputType, FilterType } from "@features/filter";
import {
    CreateMode,
    DisplayMode,
    GridColumnViewSettings,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    SortOrder,
    UpdateMode
} from "@features/grid";
import { LoginAuditService } from "../services";


@Component({
    selector: "login-audit",
    templateUrl: "./login-audit.component.html"
})
export class LoginAuditComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "ip", header: "IP",
                viewSettings: new GridColumnViewSettings({ width: "200px" })
            },
            {
                field: "datetime",
                header: "Datetime",
                displayMode: DisplayMode.Date,
                displayDateMomentFormat: "L LTS",
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            { field: "email", header: "Email" },
            { field: "browser", header: "Browser" },
            {
                field: "result",
                header: "Result",
                filterCellEnabled: false
            },
            // Calculation of "Location" field is not supported in the template code. It's up to a customer project to implement it.
            // Nevertheless, we do remain a corresponding field in the login audits model (and corresponding DB table's field).
            // Therefore this column is commented out.
            // { field: "location", header: "Location" },
            { field: "fingerprint", header: "Finger Print" }
        ],
        autoLayout: true,
        sortField: "datetime",
        sortOrder: SortOrder.Desc
    };
    gridSettings: IGridSettings = {
        readonly: true,
        exportEnabled: true,
        filtersRow: true
    };

    constructor(auditService: LoginAuditService) {
        this.gridSettings.dataService = auditService;
    }
}
