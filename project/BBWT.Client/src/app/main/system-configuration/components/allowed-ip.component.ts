import { Component } from "@angular/core";

import { CreateMode, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "@features/grid";
import { AllowedIpService } from "../../allowed-ip";


@Component({
    selector: "allowed-ip",
    templateUrl: "allowed-ip.component.html"
})
export class AllowedIpComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "ipAddressFirst", header: "IP From" },
            { field: "ipAddressLast", header: "IP To" }
        ]
    };
    gridSettings: IGridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/allowed-ip/edit/0",
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/allowed-ip/edit/:id"
    };

    constructor (public allowIpService: AllowedIpService) {
        this.gridSettings.dataService = allowIpService;
    }
}