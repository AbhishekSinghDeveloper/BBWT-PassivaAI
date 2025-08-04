import { Component } from "@angular/core";

import { FilterMatchMode, SelectItem } from "primeng/api";

import { CreateMode, GridColumnViewSettings, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";
import { FilterInputType, FilterType } from "@features/filter";
import { AggregatedRoles } from "@bbwt/modules/security/aggregated-roles";
import { RoutesService } from "./routes.service";
import { RoleService } from "../roles/role.service";
import { PermissionService } from "../roles/permission.service";


@Component({
    selector: "routes",
    templateUrl: "./routes-roles.component.html"
})
export class RoutesRolesComponent {
    pagesTableSettings: ITableSettings;
    pagesGridSettings: IGridSettings = {
        readonly: true,
        filtersRow: true,
        exportEnabled: true
    };
    apiTableSettings: ITableSettings;
    apiGridSettings: IGridSettings = {
        readonly: true,
        filtersRow: true,
        exportEnabled: true
    };


    constructor(private routesService: RoutesService,
                private roleService: RoleService,
                private permissionService: PermissionService) {
        this.initWidgets();
    }


    private async initWidgets(): Promise<void> {
        const rolesOptions = [
            ...(await this.roleService.getCoreRoles()).map(x => <SelectItem>{ label: x.name, value: x.name }),
            ...(await this.roleService.getProjectRoles()).map(x => <SelectItem>{ label: x.name, value: x.name })
        ];
        rolesOptions.unshift(<SelectItem>{ label: "Authenticated", value: AggregatedRoles.Authenticated });
        rolesOptions.unshift(<SelectItem>{ label: "Anyone", value: AggregatedRoles.Anyone });

        const pagesTableSettings = {
            lazy: false,
            columns: <IGridColumn[]>[
                {
                    field: "title",
                    header: "Title",
                    filterSettings: {
                        matchMode: FilterMatchMode.CONTAINS
                    }
                },
                {
                    field: "path",
                    header: "Path",
                    filterSettings: {
                        matchMode: FilterMatchMode.CONTAINS
                    }
                },
                {
                    field: "roles",
                    header: "Roles",
                    sortable: false,
                    displayHandler: value => value.join(", "),
                    filterSettings: {
                        filterType: FilterType.Text,
                        inputType: FilterInputType.Dropdown,
                        dropdownOptions: rolesOptions,
                        matchMode: FilterMatchMode.CONTAINS
                    }
                }
            ],
            value: await this.routesService.getPageRoles()
        };
        const apiTableSettings = {
            lazy: false,
            columns: <IGridColumn[]>[
                {
                    field: "method",
                    header: "Method",
                    filterSettings: {
                        inputType: FilterInputType.Dropdown,
                        dropdownOptions: <SelectItem[]>[
                            { label: "CONNECT", value: "CONNECT" },
                            { label: "DELETE", value: "DELETE" },
                            { label: "GET", value: "GET" },
                            { label: "HEAD", value: "HEAD" },
                            { label: "OPTIONS", value: "OPTIONS" },
                            { label: "PATCH", value: "PATCH" },
                            { label: "POST", value: "POST" },
                            { label: "PUT", value: "PUT" },
                            { label: "TRACE", value: "TRACE" },
                        ]
                    },
                    viewSettings: new GridColumnViewSettings({ width: "150px" })
                },
                {
                    field: "path",
                    header: "Path",
                    filterSettings: {
                        matchMode: FilterMatchMode.CONTAINS
                    }
                },
                {
                    field: "roles",
                    header: "Roles",
                    sortable: false,
                    displayHandler: value => value.join(", "),
                    filterSettings: {
                        filterType: FilterType.Text,
                        inputType: FilterInputType.Dropdown,
                        dropdownOptions: rolesOptions,
                        matchMode: FilterMatchMode.CONTAINS
                    }
                }
            ],
            value: await this.routesService.getApiRouteRoles()
        };

        const permissionsColumn = <IGridColumn>{
            field: "permissions",
            header: "Permissions",
            sortable: false,
            displayHandler: value => value.join(", "),
            visible: this.roleService.isPermissionBasedModel,
            filterCellEnabled: this.roleService.isPermissionBasedModel
        };
        if (this.roleService.isPermissionBasedModel) {
            const permissionsOptions = (await this.permissionService.getAll()).map(x => <SelectItem>{ label: x.name, value: x.name });

            permissionsColumn.filterSettings = {
                inputType: FilterInputType.Dropdown,
                dropdownOptions: permissionsOptions,
                matchMode: FilterMatchMode.CONTAINS
            };
        }
        pagesTableSettings.columns.push(permissionsColumn);
        apiTableSettings.columns.push(permissionsColumn);

        this.pagesTableSettings = pagesTableSettings;
        this.apiTableSettings = apiTableSettings;
    }
}