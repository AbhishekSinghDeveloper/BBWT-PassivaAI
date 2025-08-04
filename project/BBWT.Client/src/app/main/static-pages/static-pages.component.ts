import { Component } from "@angular/core";

import { StaticPageService } from "./static-page.service";
import {
    CreateMode,
    DisplayMode,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { FilterInputType, FilterType } from "../../features/filter";


@Component({
    templateUrl: "./static-pages.component.html"
})
export class StaticPagesComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "alias", header: "Alias" },
            { field: "heading", header: "Heading" },
            { field: "contentPreview", header: "Content Preview", sortable: false },
            {
                field: "lastUpdated", header: "Last Updated", displayMode: DisplayMode.Date,
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            }
        ]
    };
    gridSettings: IGridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/static/pages/edit/0",
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/static/pages/edit/:id",
        filtersRow: true
    };


    constructor(private staticPageService: StaticPageService) {
        this.gridSettings.dataService = staticPageService;
    }
}