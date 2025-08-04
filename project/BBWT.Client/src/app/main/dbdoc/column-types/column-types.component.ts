import { Component } from "@angular/core";

import "reflect-metadata";

import {
    CreateMode,
    IGridColumn,
    IGridSettings,
    ITableSettings,
    UpdateMode
} from "@features/grid";
import { ColumnTypeService } from "./column-type.service";
import { AppStorage } from "@bbwt/utils/app-storage";


@Component({
    templateUrl: "./column-types.component.html",
    styleUrls: ["./column-types.component.scss"]
})
export class ColumnTypesComponent {
    tableSettings: ITableSettings = {
        stateKey: `${AppStorage.ApplicationPrefix}.dbdoc.column-types-state`,
        stateStorage: "session",
        columns: <IGridColumn[]> [
            { field: "name", header: "Name" },
            { field: "group", header: "Group" }
        ]
    };
    gridSettings: IGridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/dbdoc/column-types/add",
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/dbdoc/column-types/edit/:id"
    };


    constructor(private columnTypesService: ColumnTypeService) {
        this.gridSettings.dataService = columnTypesService;
    }
}
