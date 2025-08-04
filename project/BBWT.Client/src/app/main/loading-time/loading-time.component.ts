import { Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { SettingsSection, SettingsSectionsName, SystemConfigurationService } from "@main/system-configuration";
import {
    FilterInputType,
    FilterType
} from "@features/filter";
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
import { LoadingTimeService } from "./loading-time.service";
import { LoadingTimeSettings } from "./loading-time-settings";
import { FilterMatchMode } from "primeng/api";


@Component({
    selector: "loading-time",
    templateUrl: "loading-time.component.html",
    styleUrls: ["loading-time.component.scss"]
})
export class LoadingTimeComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "dateTime",
                header: "Date",
                displayMode: DisplayMode.Date,
                displayDateMomentFormat: "L LTS",
                filterSettings: {
                    filterType: FilterType.Date,
                    inputType: FilterInputType.Calendar
                }
            },
            { field: "account", header: "Account" },
            { field: "route", header: "Route" },
            {
                field: "userAgent",
                header: "User Agent",
                filterSettings: {
                    matchMode: FilterMatchMode.CONTAINS
                }
            },
            {
                field: "time",
                header: "Time (ms)",
                filterSettings: {
                    filterType: FilterType.Numeric,
                    inputType: FilterInputType.Number,
                    matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO
                },
                viewSettings: new GridColumnViewSettings({ width: "150px" })
            }
        ],
        sortField: "dateTime",
        sortOrder: SortOrder.Desc
    };
    gridSettings: IGridSettings = {
        readonly: true,
        exportEnabled: true,
        filtersRow: true
    };
    settings: LoadingTimeSettings;


    constructor(private loadingTimeService: LoadingTimeService,
                private systemConfigurationService: SystemConfigurationService,
                private activatedRoute: ActivatedRoute) {
        this.gridSettings.dataService = loadingTimeService;
        this.settings = LoadingTimeSettings.parse(activatedRoute.snapshot.data["sysConfig"]);
    }


    save(): void {
        this.systemConfigurationService.saveSettings(new SettingsSection(SettingsSectionsName.LoadingTimeSettings, this.settings));
    }

    getShortedUserAgentName(userAgent: string) {
        if (userAgent) {
            const matchRes = userAgent.match(/^[a-zA-Z]+\/\d+\.\d+/);
            if (matchRes) {
                return matchRes[0];
            }
            return userAgent;
        }

        return "Unknown";
    }
}