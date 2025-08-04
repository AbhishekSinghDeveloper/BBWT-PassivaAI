import { Component } from "@angular/core";
import { Validators } from "@angular/forms";

import { SelectItem } from "primeng/api";

import { TwoFactorMandatoryMode, TwoFactorSettings } from "@main/system-configuration/classes/two-factor-settings";
import { SettingsSectionsName, SystemConfigurationService } from "@main/system-configuration";
import {
    CellEditInputType,
    DisplayMode,
    GridValidator,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { RoleService } from "./role.service";
import { PermissionService } from "./permission.service";
import { FilterInputType, FilterType } from "@features/filter";
import { notEmptyValidator } from "@bbwt/modules/validation";


@Component({
    selector: "roles",
    templateUrl: "./roles.component.html"
})
export class RolesComponent {
    tableSettings: ITableSettings;
    gridSettings: IGridSettings = {
        updateMode: UpdateMode.Inline
    };


    constructor(private roleService: RoleService,
                private permissionService: PermissionService,
                private systemConfigurationService: SystemConfigurationService) {
        this.init();
    }

    private async init(): Promise<void> {
        this.gridSettings.dataService = this.roleService;

        const permissions = await this.permissionService.getAll();

        this.tableSettings = {
            columns: <IGridColumn[]>[
                { field: "name", header: "Name", validators: [ new GridValidator(Validators.required), new GridValidator(notEmptyValidator()) ] },
                {
                    field: "permissions",
                    header: "Permissions",
                    cellEditingInputType: CellEditInputType.Multiselect,
                    dropdownOptions: permissions.map(permissionItem => <SelectItem>{
                        label: permissionItem.name,
                        value: permissionItem
                    }),
                    dropdownOptionsDataKey: "id",
                    placeholder: "Select permissions",
                    sortable: false,
                    filterSettings: {
                        filterType: FilterType.Numeric,
                        inputType: FilterInputType.Dropdown,
                        dropdownOptions: permissions.map(permissionItem => <SelectItem>{
                            label: permissionItem.name,
                            value: permissionItem.id
                        })
                    }
                }
            ]
        };

        if (this.systemConfigurationService.getSettingsSection<TwoFactorSettings>(SettingsSectionsName.TwoFactorSettings)?.mandatoryMode ==
            TwoFactorMandatoryMode.MandatoryForSpecificRoles) {
            this.tableSettings.columns.push(
                {
                    field: "authenticatorRequired",
                    header: "2FA",
                    label: "Enable 2-Factor Authentication (2FA) for this role",
                    displayMode: DisplayMode.Conditional,
                    displayConditionalTrueValue: "Yes",
                    displayConditionalFalseValue: "No",
                    cellEditingInputType: CellEditInputType.Checkbox,
                    filterCellEnabled: false,
                    sortable: false
                }
            );
        }
    }
}