import { Component } from "@angular/core";
import { S3FileManagerService } from "./s3-file-manager.service";
import { DisplayMode, IGridActionsRowButton, IGridColumn, IGridSettings, ITableSettings } from "@features/grid";


@Component({
    selector: "s3-file-manager",
    templateUrl: "./s3-file-manager.component.html",
    styleUrls: ["./s3-file-manager.component.scss"]
})
export class S3FileManagerComponent {
    tableSettings: ITableSettings = {
        columns: <IGridColumn[]>[
            { field: "key", header: "Name" },
            { field: "size", header: "Size" },
            {
                field: "lastModifiedDate",
                header: "Last Modified",
                displayMode: DisplayMode.Date,
                displayDateMomentFormat: "L LTS"
            }
        ],
        dataKey: "key"
    };
    gridSettings: IGridSettings = {
        readonly: true,
        sortEnabled: false,
        additionalRowActions: [
            <IGridActionsRowButton> {
                label: "Create URL",
                handler: rowData => {
                    this.preSignedURL = "";
                    this.s3FileManagerService.getPresignedUrl(rowData.key)
                        .then((url: string) => this.preSignedURL = url);
                    this.displayDialog = true;
                }
            }
        ]
    };
    preSignedURL = "";
    displayDialog = false;


    constructor(public s3FileManagerService: S3FileManagerService) {
        this.gridSettings.dataService = s3FileManagerService;
    }
}