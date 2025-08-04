import { Component, ViewChild } from "@angular/core";
import { Router } from "@angular/router";

import {
    CreateMode,
    DisplayMode,
    GridColumnViewSettings,
    GridComponent,
    IGridActionsRowButton,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { IReport } from "../reporting-models";
import { ReportService } from "../services/report.service";
import { ConfirmationService } from "@main/admin/services";
import { FilterInputType, FilterType } from "@features/filter";


@Component({
    templateUrl: "./reports.component.html"
})
export class ReportsComponent {
    @ViewChild("reportsGrid", { static: true }) private reportsGrid: GridComponent;

    tableSettings: ITableSettings = {
        sortField: "name",
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Title",
            },
            {
                field: "urlSlug",
                header: "URL Slug",
                viewSettings: new GridColumnViewSettings({ width: "250px" })
            },
            {
                field: "access",
                header: "Reports",
                visible: false
            },
            {
                field: "roles",
                header: "Roles",
                sortable: false,
                filterCellEnabled: false,
                displayHandler: (cellValue, rowValue) => rowValue.access || cellValue.map(x => x.name).join(", ")
            },
            {
                field: "permissions",
                header: "Permissions",
                sortable: false,
                filterCellEnabled: false,
                displayHandler: (cellValue, rowValue) => rowValue.access || cellValue.map(x => x.name).join(", ")
            },
            {
                field: "updatedOn",
                header: "Published On",
                displayMode: DisplayMode.Date,
                displayDateMomentFormat: "L LT",
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            {
                field: "updatedBy",
                header: "Published By",
                filterSettings: {
                    matchModeSelectorVisible: false
                }
            }
        ]
    };
    gridSettings: IGridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/reporting/reports/create",
        updateMode: UpdateMode.Disabled,
        deletingEnabled: false,
        exportEnabled: true,
        filtersRow: true,
        actionsColumnWidth: "10rem",
        additionalRowActions: [
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-pencil",
                hint: "Edit report",
                handler: (report: IReport) =>
                    this.router.navigateByUrl("/app/reporting/reports/edit/" + report.id)
            },
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text",
                primeIcon: "pi pi-external-link",
                hint: "View published version of report in a new tab",
                handler: (report: IReport) =>
                    window.open(`/app/reporting/view/${report.urlSlug}`)
            },
            <IGridActionsRowButton>{
                buttonClass: "p-button-rounded p-button-text p-button-danger",
                primeIcon: "pi pi-trash",
                hint: "Delete report",
                handler: (report: IReport) => {
                    this.confirmationService.confirm({
                        message: `Are you sure that you want to delete report "${report.name}"?`,
                        accept: () =>
                            this.reportService.delete(report.id).then(() => {
                                this.reportsGrid.reload();
                            })
                    });
                }
            },
        ],
    };


    constructor(
        private reportService: ReportService,
        private router: Router,
        private confirmationService: ConfirmationService
    ) {
        this.gridSettings.dataService = reportService;
    }
}