import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";

import { MessageService } from "primeng/api";

import { DisplayMode, IGridColumn, IGridSettings, ITableSettings } from "@features/grid";
import { CellEditInputType } from "../../../features/grid/enums/cell-edit-input-type";
import { FormioParameterListService } from "../formio-parameterlist.service";
import { SystemConfigurationService } from "../system-configuration.service";
import { FormioSettings } from "../classes/formio-settings";
import { SettingsSection } from "../classes/settings-section";
import { SettingsSectionsName } from "../settings-sections-name";

@Component({
    selector: "formio-settings",
    templateUrl: "formio-settings.component.html",
})
export class FormioSettingsComponent implements OnInit {
    settings: FormioSettings;
    public tableSettingsFormioParameterViewer: ITableSettings;
    public formioViewerParameterGridSettings: IGridSettings = {
    }

    constructor(
        private route: ActivatedRoute,
        private systemConfigurationService: SystemConfigurationService,
        private formioParameterListService: FormioParameterListService,
        private messageService: MessageService) { }

    ngOnInit() {
        this.settings = FormioSettings.parseSection(this.route.snapshot.data["sysConfig"]);

        this.formioViewerParameterGridSettings.dataService = this.formioParameterListService;
        this.tableSettingsFormioParameterViewer = {
            columns: <IGridColumn[]>[
                {
                    field: "name",
                    header: "Parameter Name",

                },
                {
                    field: "position",
                    header: "Value Position",
                    displayMode: DisplayMode.Number,
                    defaultValue: -1,
                    cellEditingInputType: CellEditInputType.Number,
                    displayHandler: (cellValue: any, rowValue?: any) => {
                        return (cellValue == null || cellValue < 0) ? "-" : cellValue;
                    }
                },
                {
                    field: "tableName",
                    header: "Table Name",
                },
                {
                    field: "keyField",
                    header: "Field Name",
                    displayHandler: (cellValue: any, rowValue?: any) => {
                        return (cellValue == null || cellValue == "") ? "-" : cellValue;
                    }
                },
            ]
        };
    }

    saveFeatureEnabled() {
        this.systemConfigurationService.saveSettings(
            new SettingsSection(SettingsSectionsName.FormioSettings, this.settings));
    }
}