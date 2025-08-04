import { Component } from "@angular/core";

import {
    CreateMode,
    DisplayMode,
    GridColumnViewSettings,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { EmailTemplateService } from "./email-template.service";
import { FilterInputType, FilterType } from "../../features/filter";


@Component({
    selector: "email-templates",
    templateUrl: "./email-templates.component.html"
})
export class EmailTemplatesComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "code", header: "Code",
                viewSettings: new GridColumnViewSettings({ width: "250px" })
            },
            { field: "title", header: "Title" },
            {
                field: "isSystem",
                header: "Is System",
                sortable: false,
                displayMode: DisplayMode.Conditional,
                trueConditionValue: "System",
                falseConditionValue: "User",
                viewSettings: new GridColumnViewSettings({ width: "150px" }),
                filterSettings: {
                    filterType: FilterType.Boolean,
                    inputType: FilterInputType.Checkbox,
                    ignoreIfConvertibleToFalse: true
                }
            }
        ],
        stateStorage: "session",
        stateKey: "email-templates-grid"
    };
    gridSettings: IGridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/email-templates/edit/0",
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/email-templates/edit/:id",
        exportEnabled: true,
        filtersRow: true
    };

    constructor(private emailTemplateService: EmailTemplateService) {
        this.gridSettings.dataService = emailTemplateService;
    }
}