import { Component } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";

import { AllowedIp, IAllowedRole, IAllowedUser } from "./allowed-ip-models";
import { AllowedIpService } from "./allowed-ip.service";
import { UserService } from "@main/users";
import {
    CreateMode,
    DisplayMode, GridColumnViewSettings,
    IGridColumn,
    IGridSettings, IPagedData,
    ITableSettings,
    UpdateMode
} from "@features/grid";


@Component({
    selector: "edit-allowed-ip",
    templateUrl: "edit-allowed-ip.component.html",
    styleUrls: ["./edit-allowed-ip.component.scss"]
})
export class EditAllowedIpComponent {
    usersTableSettings: ITableSettings = {
        columns: <IGridColumn[]> [
            {
                field: "isAllowedIp",
                header: "Is IP Allowed",
                sortable: false,
                filterCellEnabled: false,
                displayMode: DisplayMode.Checkbox,
                outputValueChangedHandler: rowValue => {
                    if (rowValue.isAllowedIp) {
                        this.allowedIp.users.push(rowValue);
                    } else {
                        const removingUserIndex = this.allowedIp.users.findIndex(item => item.id == rowValue.id);
                        if (removingUserIndex >= 0) {
                            this.allowedIp.users.splice(removingUserIndex, 1);
                        }
                    }
                },
                columnViewSettings: new GridColumnViewSettings({ width: 40 })
            },
            { field: "firstName", header: "First Name" },
            { field: "lastName", header: "Last Name" },
            { field: "email", header: "Email" },
        ]
    };
    rolesTableSettings: ITableSettings = {
        columns: <IGridColumn[]> [
            {
                field: "isAllowedIp",
                header: "Is IP Allowed",
                sortable: false,
                displayMode: DisplayMode.Checkbox,
                outputValueChangedHandler: rowValue => {
                    if (rowValue.isAllowedIp) {
                        this.allowedIp.roles.push(rowValue);
                    } else {
                        const removingRoleIndex = this.allowedIp.roles.findIndex(item => item.id == rowValue.id);
                        if (removingRoleIndex >= 0) {
                            this.allowedIp.roles.splice(removingRoleIndex, 1);
                        }
                    }
                },
                columnViewSettings: new GridColumnViewSettings({ width: 40 })
            },
            { field: "name", header: "Name", sortable: false }
        ],
        paginator: false
    };
    usersGridSettings: IGridSettings = {
        readonly: true,
        filtersRow: true,
        loadedDataHandler: data => {
            data.items.forEach(item => {
                item["isAllowedIp"] = this.allowedIp.users.some(x => x.id == item.id);
            });

            return <IPagedData<IAllowedUser>><any>data;
        }
    };
    rolesGridSettings: IGridSettings = {
        dataServiceGetPageMethodName: "getAllRoles",
        readonly: true,
        loadedDataHandler: data => {
            data.forEach(item => {
                item["isAllowedIp"] = this.allowedIp.roles.some(x => x.id == item.id);
            });

            return <IPagedData<IAllowedRole>>{ items: data, total: data.length };
        }
    };
    allowedIp: AllowedIp = new AllowedIp();


    constructor(private router: Router,
                private route: ActivatedRoute,
                private userService: UserService,
                private allowedIpService: AllowedIpService) {
        this.usersGridSettings.dataService = userService;
        this.rolesGridSettings.dataService = userService;
        route.params.subscribe(params => {
            const id = <number>params["id"];
            if (!!id && id != 0) {
                allowedIpService.get(id).then(data => this.allowedIp = data);
            }
        });
    }


    save(): void {
        if (this.allowedIp.id) {
            this.allowedIpService.update(this.allowedIp.id, this.allowedIp).then(() => this.back());
        } else {
            this.allowedIpService.create(this.allowedIp).then(() => this.back());
        }
    }

    back(): void {
        this.router.navigate(["/app/system"]);
    }
}