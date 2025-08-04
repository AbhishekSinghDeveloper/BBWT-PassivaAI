import { Component } from "@angular/core";
import { Validators } from "@angular/forms";

import { GridValidator, IGridColumn, IGridSettings, ITableSettings } from "@features/grid";
import { ParametersStoreService } from "../parameters-store.service";
import { notEmptyValidator } from "@bbwt/modules/validation";


@Component({
    selector: "parameters-store",
    templateUrl: "./parameters-store.component.html"
})
export class ParametersStoreComponent {
    parametersStoreAvailable: boolean;
    tableSettings: ITableSettings = {
        paginator: false,
        dataKey: "name",
        columns: <IGridColumn[]>[
            {
                field: "name",
                header: "Name",
                editableAfterCreation: false,
                validators: [new GridValidator(Validators.required), new GridValidator(notEmptyValidator())]
            },
            {
                field: "value",
                header: "Value",
                validators: [new GridValidator(Validators.required), new GridValidator(notEmptyValidator())]
            }
        ]
    };
    gridSettings: IGridSettings = {
        dataServiceGetPageMethodName: "getAppSettings",
        manuallyCreatableDataKey: true
    };


    constructor(private parametersStoreService: ParametersStoreService) {
        this.gridSettings.dataService = parametersStoreService;
        parametersStoreService.isEnabled().then(isEnabled => this.parametersStoreAvailable = isEnabled);
    }
}