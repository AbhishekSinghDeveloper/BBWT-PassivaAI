
import { Component, ViewChild } from "@angular/core";
import { Validators } from "@angular/forms";
import { LogService } from "./log.service";
import { IGridColumn, CellEditInputType, ITableSettings, IGridSettings, DisplayMode, SortOrder, GridComponent } from "../../features/grid";
import { IFilterSettings, FilterInputType, FilterType, CountableFilterMatchMode } from "../../features/filter";
import { FilterMatchMode, MessageService, SelectItem } from "primeng/api";
import { Message } from "../../bbwt/classes";
import * as moment from "moment";
import { ILog, TimeWindow } from "./log";
 
@Component({
    selector: "logs",
    styleUrls: ["./logs.component.scss"],
    templateUrl: "./logs.component.html"
})
export class LogsComponent { 
    gridSettings: IGridSettings;
    tableSettings: ITableSettings;
    filterSettings: IFilterSettings[];
    shownLog: ILog;
    detailsVisible: boolean;

    @ViewChild(GridComponent, { static: true }) private grid: GridComponent;

    constructor(public logService: LogService, private messageService: MessageService) {
        this.initGridSettings();
        this.initFilterSettings();
        this.initTableSettings();
    }

    private initTableSettings(){
        const tableSettings = {
            sortField: "timeStamp",
            sortOrder: SortOrder.Desc,
            stateStorage: "session",
            stateKey: "aggregated-logs-grid",
            columns: <IGridColumn[]>[
                {
                    field: "timeStamp",
                    header: "Time Stamp",
                    displayMode: DisplayMode.Date,
                    displayDateMomentFormat: "L LTS"
                },
                {
                    field: "message",
                    header: "Message"
                },
                {
                    field: "source",
                    header: "Source"
                },
                {
                    field: "level",
                    header: "Level"
                }
            ]
        } as ITableSettings;

        this.tableSettings = tableSettings;
    }

    private initGridSettings(){
        const gridSettings = {
            readonly: true,
            additionalRowActions: [
                {
                    label: "Details",
                    handler: row => {
                        this.shownLog = row;
                        this.detailsVisible = true;
                    }
                }
            ],
        } as IGridSettings;

        this.gridSettings = gridSettings;
        this.gridSettings.dataService = this.logService;
    }

    private initFilterSettings() {
        const filterSettings = <IFilterSettings[]>[
            {
                valueFieldName: "appName",
                header: "System"
            },
            {
                valueFieldName: "id",
                header: "Trace ID",
                filterType: FilterType.Numeric
            },
            {
                valueFieldName: "source",
                header: "Source"
            },
            {
                valueFieldName: "timeWindow",
                header: "Time Window",
                filterType: FilterType.Numeric,
                inputType: FilterInputType.Dropdown,
                dropdownOptions: <SelectItem[]>[
                    { label: "10 minutes", value: TimeWindow.TenMinutes },
                    { label: "30 minutes", value: TimeWindow.ThirtyMinutes },
                    { label: "1 hour", value: TimeWindow.OneHour },
                    { label: "3 hours", value: TimeWindow.ThreeHours },
                    { label: "12 hours", value: TimeWindow.TwelveHours }
                ],
                matchMode: CountableFilterMatchMode.GreaterThan,
                defaultValue: TimeWindow.OneHour
            },
            {
                valueFieldName: "level",
                header: "Level",
                filterType: FilterType.Text,
                inputType: FilterInputType.Dropdown,
                dropdownOptions: <SelectItem[]>[
                    {label: "Error", value: "Error"},
                    {label: "Information", value: "Information"},
                    {label: "Warning", value: "Warning"}
                ],
                defaultValue: "Error"
            }
        ];

        this.filterSettings = filterSettings;
    }
}