import { Component } from "@angular/core";
import { CreateMode, IGridColumn, IGridSettings, ITableSettings, UpdateMode } from "../../../features/grid";


@Component({
    templateUrl: "./named-queries.component.html",
    styleUrls: ["./named-queries.component.scss"]
})
export class NamedQueriesComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name"
            },
            {
                field: "tableUsed",
                header: "Tables Used"
            }
        ]
    };

    gridSettings: IGridSettings = {
        createMode: CreateMode.Redirect,
        createLink: "/app/reporting/named-queries/create",
        updateMode: UpdateMode.Redirect,
        updateLink: "/app/reporting/named-queries/edit/:id"
    }
}